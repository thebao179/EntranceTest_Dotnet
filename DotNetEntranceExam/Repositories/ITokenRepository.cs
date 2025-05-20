using DotNetEntranceExam.Entities;

namespace DotNetEntranceExam.Repositories
{
    public interface ITokenRepository
    {
        Task AddAsync(Token token);
        Task<Token?> GetByRefreshTokenAsync(string refreshTokenHash);
        Task RemoveAllByUserIdAsync(int userId);
        Task RemoveAsync(Token token);
    }
}
