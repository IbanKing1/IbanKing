using IBanKing.Models;
using System.Threading.Tasks;

namespace IBanKing.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByEmailAsync(string email);
        Task<User> GetByIdAsync(int id);
        Task UpdateAsync(User user);
    }
}
