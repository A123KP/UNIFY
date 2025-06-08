using Microsoft.AspNetCore.Http;
using UNIFY.Models;

namespace UNIFY.Services
{
    public interface IUserService //all the function goin to be implemented in UserService.cs
    {
        User GetUserByEmailOrUsername(string identifier);
        bool VerifyPassword(string rawPassword, string hashedPassword);
        void StartSession(HttpContext context, User user);

        int? GetUserIdFromSession(HttpContext context);

        string GetUserRoleFromSession(HttpContext context);
        bool IsUsernameTaken(string username);
        bool IsUserLoggedIn(HttpContext httpContext);
        bool IsEmailTaken(string email);
        void RegisterUser(User user, string confirmPassword);
        User GetUserById(int userId);
        void UpdateUserEmail(int userId, string newEmail);
    }
}