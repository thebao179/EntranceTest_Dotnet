using DotNetEntranceExam.Data;
using DotNetEntranceExam.Entities;
using Microsoft.EntityFrameworkCore;

namespace DotNetEntranceExam.Repositories
{
    public class TokenRepository: ITokenRepository
    {
        private readonly AppDbContext _context;
        public TokenRepository(AppDbContext context) => _context = context;

        public async Task AddAsync(Token token)
        {
            _context.Tokens.Add(token);
            await _context.SaveChangesAsync();
        }

        public async Task<Token?> GetByRefreshTokenAsync(string refreshTokenHash)
        => await _context.Tokens.Include(t => t.User)
            .FirstOrDefaultAsync(t => t.RefreshToken == refreshTokenHash);

        public async Task RemoveAllByUserIdAsync(int userId)
        {
            var tokens = _context.Tokens.Where(t => t.UserId == userId);
            _context.Tokens.RemoveRange(tokens);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(Token token)
        {
            _context.Tokens.Remove(token);
            await _context.SaveChangesAsync();
        }
    }
}
