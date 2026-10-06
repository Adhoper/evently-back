using Evently.Api.Data;
using Evently.Api.DTOs.Auth;
using Evently.Api.Models.Enums;
using Evently.Api.Services.Interfaces;
using Evently.Api.Services.Results;
using Microsoft.EntityFrameworkCore;

namespace Evently.Api.Services
{
    public class UserService : IUserService
    {
        private readonly EventlyDbContext _context;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public UserService(
            EventlyDbContext context,
            ITokenService tokenService,
            IConfiguration configuration)
        {
            _context = context;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        public async Task<UserDto?> GetByIdAsync(int userId)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    Email = u.Email,
                    Role = u.Role.ToString()
                })
                .FirstOrDefaultAsync();
        }

        public async Task<ServiceResult<AuthResponseDto>>
            BecomeOrganizerAsync(int userId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user is null)
            {
                return ServiceResult<AuthResponseDto>
                    .Missing("El usuario no existe.");
            }

            if (!user.IsActive)
            {
                return ServiceResult<AuthResponseDto>
                    .Failure("La cuenta se encuentra desactivada.");
            }

            if (user.Role == UserRole.Organizer)
            {
                return ServiceResult<AuthResponseDto>
                    .Failure("La cuenta ya es un organizador.");
            }

            if (user.Role == UserRole.Admin)
            {
                return ServiceResult<AuthResponseDto>
                    .Failure(
                        "Una cuenta administradora no necesita convertirse en organizador.");
            }

            user.Role = UserRole.Organizer;

            await _context.SaveChangesAsync();

            var expirationMinutes =
                int.Parse(
                    _configuration["Jwt:ExpirationMinutes"]
                    ?? "120");

            var response = new AuthResponseDto
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

            return ServiceResult<AuthResponseDto>
                .Ok(response);
        }
    }
}