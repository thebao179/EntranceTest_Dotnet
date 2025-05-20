using DotNetEntranceExam.Data;
using DotNetEntranceExam.DTOs;
using DotNetEntranceExam.Entities;
using DotNetEntranceExam.Helpers;
using DotNetEntranceExam.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.Ocsp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace DotNetEntranceExam.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenRepository _tokenRepository;
        private readonly IConfiguration _config;

        public AuthService(
            IUserRepository userRepository,
            ITokenRepository tokenRepository,
            IConfiguration config)
        {
            _userRepository = userRepository;
            _tokenRepository = tokenRepository;
            _config = config;
        }

        public async Task<UserDTO?> SignUpAsync(SignUpRequest request)
        {
            // Validate email & password
            if (!Validation.IsValidEmail(request.Email) || request.Password.Length < 8 || request.Password.Length > 20)
                return null;

            var existing = await _userRepository.GetByEmailAsync(request.Email);
            if (existing != null)
                return null;

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Hash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            user = await _userRepository.CreateAsync(user);

            return new UserDTO
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                DisplayName = $"{user.FirstName} {user.LastName}"
            };
        }

        public async Task<AuthResponse?> SignInAsync(SignInRequest request)
        {
            if (!Validation.IsValidEmail(request.Email) || request.Password.Length < 8 || request.Password.Length > 20)
                return null;

            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Hash))
                return null;

            var jwtToken = GenerateJwtToken(user);

            var expireAt = DateTime.UtcNow.AddDays(30);
            var refreshToken = Guid.NewGuid().ToString("N");

            var token = new Token
            {
                UserId = user.Id,
                RefreshToken = refreshToken,
                ExpiresIn = expireAt.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _tokenRepository.AddAsync(token);

            return new AuthResponse
            {
                User = new UserDTO
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    DisplayName = $"{user.FirstName} {user.LastName}"
                },
                Token = jwtToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthResponse?> RefreshTokenAsync(string refreshToken)
        {
            var existing = await _tokenRepository.GetByRefreshTokenAsync(refreshToken);
            if (existing == null)
                return null;

            if (!DateTime.TryParse(existing.ExpiresIn, out var expiresAt))
                return null;

            if (expiresAt < DateTime.UtcNow)
            {
                await _tokenRepository.RemoveAsync(existing);
                return null;
            }

            await _tokenRepository.RemoveAsync(existing);

            var newRawToken = Guid.NewGuid().ToString("N");
            var newExpiresAt = DateTime.UtcNow.AddDays(30);

            var newToken = new Token
            {
                UserId = existing.User.Id,
                RefreshToken = newRawToken,
                ExpiresIn = newExpiresAt.ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _tokenRepository.AddAsync(newToken);
            var jwtToken = GenerateJwtToken(existing.User);

            return new AuthResponse
            {
                Token = jwtToken,
                RefreshToken = newRawToken
            };
        }

        public async Task<bool> SignOutAsync(int userId)
        {
            await _tokenRepository.RemoveAllByUserIdAsync(userId);
            return true;
        }

        // Helpers

        private string GenerateJwtToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);

            var claims = new[]
            {
            new Claim("id", user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}")
        };

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(descriptor);
            return handler.WriteToken(token);
        }
    }
}
