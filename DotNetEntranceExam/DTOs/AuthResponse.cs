namespace DotNetEntranceExam.DTOs
{
    public class AuthResponse
    {
        public UserDTO? User { get; set; }
        public string? Token { get; set; } = null!;
        public string? RefreshToken { get; set; } = null!;
    }
}
