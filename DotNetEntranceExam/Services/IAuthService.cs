using DotNetEntranceExam.DTOs;

namespace DotNetEntranceExam.Services
{
    public interface IAuthService
    {
        Task<UserDTO?> SignUpAsync(SignUpRequest request);
        Task<AuthResponse?> SignInAsync(SignInRequest request);
        Task<bool> SignOutAsync(int userId);
        Task<AuthResponse?> RefreshTokenAsync(string refreshToken);
    }
}
