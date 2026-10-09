using Evently.Api.Data;
using Evently.Api.DTOs.Auth;
using Evently.Api.Models;
using Evently.Api.Models.Enums;
using Evently.Api.Services.Interfaces;
using Evently.Api.Services.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Evently.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly EventlyDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly PasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AuthService(
            EventlyDbContext context,
            ITokenService tokenService,
            IConfiguration configuration)
        {
            _context = context;
            _tokenService = tokenService;
            _configuration = configuration;

            _passwordHasher = new PasswordHasher<User>();
        }

        public async Task<ServiceResult<AuthResponseDto>>
            RegisterAsync(RegisterDto dto)
        {
            var normalizedEmail =
                dto.Email.Trim().ToLower();

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email == normalizedEmail);

            if (emailExists)
            {
                return ServiceResult<AuthResponseDto>
                    .Failure(
                        "Ya existe una cuenta con este correo electrónico.");
            }

            var user = new User
            {
                FirstName = dto.FirstName.Trim(),
                LastName = dto.LastName.Trim(),
                Email = normalizedEmail,
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            user.PasswordHash =
                _passwordHasher.HashPassword(
                    user,
                    dto.Password);

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            var response = CreateAuthResponse(user);

            return ServiceResult<AuthResponseDto>
                .Ok(response);
        }

        public async Task<ServiceResult<AuthResponseDto>>
            LoginAsync(LoginDto dto)
        {
            var normalizedEmail =
                dto.Email.Trim().ToLower();

            var user = await _context.Users
                .FirstOrDefaultAsync(
                    u => u.Email == normalizedEmail);

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
                _passwordHasher.VerifyHashedPassword(
                    user,
                    user.PasswordHash,
                    dto.Password);

            if (verification ==
                PasswordVerificationResult.Failed)
            {
                return ServiceResult<AuthResponseDto>
                    .Failure(
                        "Correo electrónico o contraseña incorrectos.");
            }

            var response = CreateAuthResponse(user);

            return ServiceResult<AuthResponseDto>
                .Ok(response);
        }

        private AuthResponseDto CreateAuthResponse(User user)
        {
            var expirationMinutes =
                int.Parse(
                    _configuration["Jwt:ExpirationMinutes"]
                    ?? "120");

            return new AuthResponseDto
            {
                Token = _tokenService.CreateToken(user),

                ExpiresAt = DateTime.UtcNow
                    .AddMinutes(expirationMinutes),

                User = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role.ToString()
                }
            };
        }
    }
}