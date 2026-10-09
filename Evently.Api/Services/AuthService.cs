using Evently.Api.Data;
using Evently.Api.DTOs.Auth;
using Evently.Api.DTOs.Common;
using Evently.Api.Models;
using Evently.Api.Models.Enums;
using Evently.Api.Services.Interfaces;
using Evently.Api.Services.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Evently.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly EventlyDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<AuthService> _logger;

        private readonly PasswordHasher<User>
            _passwordHasher;

        private const string GenericForgotMessage =
            "Si existe una cuenta asociada a ese correo, recibirás instrucciones para restablecer tu contraseña.";

        public AuthService(
            EventlyDbContext context,
            ITokenService tokenService,
            IEmailService emailService,
            IConfiguration configuration,
            IWebHostEnvironment environment,
            ILogger<AuthService> logger)
        {
            _context = context;
            _tokenService = tokenService;
            _emailService = emailService;
            _configuration = configuration;
            _environment = environment;
            _logger = logger;

            _passwordHasher =
                new PasswordHasher<User>();
        }

        public async Task<ServiceResult<AuthResponseDto>>
            RegisterAsync(
                RegisterDto dto)
        {
            var normalizedEmail =
                dto.Email
                    .Trim()
                    .ToLowerInvariant();

            var emailExists =
                await _context.Users
                    .AnyAsync(u =>
                        u.Email ==
                        normalizedEmail);

            if (emailExists)
            {
                return ServiceResult<AuthResponseDto>
                    .Failure(
                        "Ya existe una cuenta con este correo electrónico.");
            }

            var user = new User
            {
                FirstName =
                    dto.FirstName.Trim(),

                LastName =
                    dto.LastName.Trim(),

                Email =
                    normalizedEmail,

                Role =
                    UserRole.User,

                IsActive =
                    true,

                CreatedAt =
                    DateTime.UtcNow
            };

            user.PasswordHash =
                _passwordHasher
                    .HashPassword(
                        user,
                        dto.Password);

            _context.Users.Add(
                user);

            await _context
                .SaveChangesAsync();

            var response =
                CreateAuthResponse(
                    user);

            return ServiceResult<AuthResponseDto>
                .Ok(response);
        }

        public async Task<ServiceResult<AuthResponseDto>>
            LoginAsync(
                LoginDto dto)
        {
            var normalizedEmail =
                dto.Email
                    .Trim()
                    .ToLowerInvariant();

            var user =
                await _context.Users
                    .FirstOrDefaultAsync(
                        u =>
                            u.Email ==
                            normalizedEmail);

            if (user is null)
            {
                return ServiceResult<AuthResponseDto>
                    .Failure(
                        "Correo electrónico o contraseña incorrectos.");
            }

            if (!user.IsActive)
            {
                return ServiceResult<AuthResponseDto>
                    .Failure(
                        "Esta cuenta se encuentra desactivada.");
            }

            var verification =
                _passwordHasher
                    .VerifyHashedPassword(
                        user,
                        user.PasswordHash,
                        dto.Password);

            if (
                verification ==
                PasswordVerificationResult
                    .Failed)
            {
                return ServiceResult<AuthResponseDto>
                    .Failure(
                        "Correo electrónico o contraseña incorrectos.");
            }

            var response =
                CreateAuthResponse(
                    user);

            return ServiceResult<AuthResponseDto>
                .Ok(response);
        }

        public async Task<ServiceResult<MessageDto>>
            ForgotPasswordAsync(
                ForgotPasswordDto dto)
        {
            var normalizedEmail =
                dto.Email
                    .Trim()
                    .ToLowerInvariant();

            var user =
                await _context.Users
                    .FirstOrDefaultAsync(
                        u =>
                            u.Email ==
                                normalizedEmail &&
                            u.IsActive);

            if (user is null)
            {
                return GenericForgotResponse();
            }

            var now =
                DateTime.UtcNow;

            // Evita enviar muchos correos a la misma
            // cuenta en pocos segundos.
            var recentRequest =
                await _context
                    .PasswordResetTokens
                    .AnyAsync(t =>
                        t.UserId ==
                            user.Id &&
                        t.UsedAt ==
                            null &&
                        t.CreatedAt >
                            now.AddMinutes(-1));

            if (recentRequest)
            {
                return GenericForgotResponse();
            }

            // Invalidamos cualquier enlace anterior.
            var previousTokens =
                await _context
                    .PasswordResetTokens
                    .Where(t =>
                        t.UserId ==
                            user.Id &&
                        t.UsedAt ==
                            null)
                    .ToListAsync();

            foreach (
                var previousToken
                in previousTokens)
            {
                previousToken.UsedAt =
                    now;
            }

            var rawToken =
                GenerateResetToken();

            var tokenHash =
                HashToken(
                    rawToken);

            var resetToken =
                new PasswordResetToken
                {
                    UserId =
                        user.Id,

                    TokenHash =
                        tokenHash,

                    CreatedAt =
                        now,

                    ExpiresAt =
                        now.AddMinutes(30)
                };

            _context
                .PasswordResetTokens
                .Add(resetToken);

            await _context
                .SaveChangesAsync();

            var frontendBaseUrl =
                _configuration[
                    "Frontend:BaseUrl"]
                ?? "http://localhost:5173";

            var resetUrl =
                $"{frontendBaseUrl.TrimEnd('/')}/reset-password?token={Uri.EscapeDataString(rawToken)}";

            if (_environment
                .IsDevelopment())
            {
                _logger.LogInformation(
                    "Password reset URL for {Email}: {ResetUrl}",
                    user.Email,
                    resetUrl);
            }

            try
            {
                await _emailService
                    .SendPasswordResetAsync(
                        user.Email,
                        user.FirstName,
                        resetUrl);
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "No fue posible enviar el correo de recuperación a {Email}.",
                    user.Email);

                /*
                 * No devolvemos un error diferente porque
                 * permitiría descubrir qué correos existen.
                 */
            }

            return GenericForgotResponse();
        }

        public async Task<ServiceResult<MessageDto>>
            ResetPasswordAsync(
                ResetPasswordDto dto)
        {
            var tokenHash =
                HashToken(
                    dto.Token.Trim());

            var now =
                DateTime.UtcNow;

            var resetToken =
                await _context
                    .PasswordResetTokens
                    .Include(t =>
                        t.User)
                    .FirstOrDefaultAsync(t =>
                        t.TokenHash ==
                            tokenHash &&
                        t.UsedAt ==
                            null &&
                        t.ExpiresAt >
                            now);

            if (
                resetToken is null ||
                !resetToken.User.IsActive)
            {
                return ServiceResult<MessageDto>
                    .Failure(
                        "El enlace de recuperación no es válido o ha expirado.");
            }

            var user =
                resetToken.User;

            user.PasswordHash =
                _passwordHasher
                    .HashPassword(
                        user,
                        dto.Password);

            resetToken.UsedAt =
                now;

            // Invalidamos cualquier otro token
            // de recuperación pendiente.
            var otherTokens =
                await _context
                    .PasswordResetTokens
                    .Where(t =>
                        t.UserId ==
                            user.Id &&
                        t.Id !=
                            resetToken.Id &&
                        t.UsedAt ==
                            null)
                    .ToListAsync();

            foreach (
                var token
                in otherTokens)
            {
                token.UsedAt =
                    now;
            }

            await _context
                .SaveChangesAsync();

            return ServiceResult<MessageDto>
                .Ok(
                    new MessageDto
                    {
                        Message =
                            "Tu contraseña fue restablecida correctamente."
                    });
        }

        private ServiceResult<MessageDto>
            GenericForgotResponse()
        {
            return ServiceResult<MessageDto>
                .Ok(
                    new MessageDto
                    {
                        Message =
                            GenericForgotMessage
                    });
        }

        private static string GenerateResetToken()
        {
            var bytes =
                RandomNumberGenerator
                    .GetBytes(32);

            return WebEncoders
                .Base64UrlEncode(
                    bytes);
        }

        private static string HashToken(
            string token)
        {
            var bytes =
                Encoding.UTF8
                    .GetBytes(
                        token);

            var hash =
                SHA256.HashData(
                    bytes);

            return Convert
                .ToHexString(
                    hash);
        }

        private AuthResponseDto
            CreateAuthResponse(
                User user)
        {
            var expirationMinutes =
                int.Parse(
                    _configuration[
                        "Jwt:ExpirationMinutes"]
                    ?? "120");

            return new AuthResponseDto
            {
                Token =
                    _tokenService
                        .CreateToken(
                            user),

                ExpiresAt =
                    DateTime.UtcNow
                        .AddMinutes(
                            expirationMinutes),

                User =
                    new UserDto
                    {
                        Id =
                            user.Id,

                        FirstName =
                            user.FirstName,

                        LastName =
                            user.LastName,

                        Email =
                            user.Email,

                        Role =
                            user.Role
                                .ToString()
                    }
            };
        }
    }
}