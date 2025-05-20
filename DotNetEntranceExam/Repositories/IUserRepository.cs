using DotNetEntranceExam.Entities;

namespace DotNetEntranceExam.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User> CreateAsync(User user);
    }
}
