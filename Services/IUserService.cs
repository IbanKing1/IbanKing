using IBanKing.Models;
using System.Threading.Tasks;

namespace IBanKing.Services
{
    public interface IUserService
    {
        Task<User> AuthenticateAsync(string email, string password);
        Task EditNameAsync(int userId, string newName);
        Task EditEmailAsync(int userId, string newEmail);
        Task IncrementFailedLoginAsync(User user);
        Task ResetFailedLoginAsync(User user);
    }
}
