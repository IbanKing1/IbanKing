using IBanKing.Models;
using IBanKing.Repositories;
using IBanKing.Services.Interfaces;
using System.Threading.Tasks;

namespace IBanKing.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> AuthenticateAsync(string email, string password)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null || user.IsBlocked || user.Password != password)
                return null;

            return user;
        }

        public async Task EditNameAsync(int userId, string newName)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.Name = newName;
                await _userRepository.UpdateAsync(user);
            }
        }

        public async Task EditEmailAsync(int userId, string newEmail)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null)
            {
                user.Email = newEmail;
                await _userRepository.UpdateAsync(user);
            }
        }

        public async Task IncrementFailedLoginAsync(User user)
        {
            user.FailedLoginAttempts++;

            if (user.FailedLoginAttempts >= 4)
            {
                user.IsBlocked = true;
            }

            await _userRepository.UpdateAsync(user);
        }

        public async Task ResetFailedLoginAsync(User user)
        {
            user.FailedLoginAttempts = 0;
            await _userRepository.UpdateAsync(user);
        }
    }
}
