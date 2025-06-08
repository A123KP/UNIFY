
using UNIFY.Data;
using UNIFY.Models;

namespace UNIFY.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }
        public bool IsUserLoggedIn(HttpContext context)
        {
            return context.Session.GetInt32("UserId").HasValue &&
                !string.IsNullOrEmpty(context.Session.GetString("UserRole"));
        }

        public User GetUserByEmailOrUsername(string identifier)
        {
            return _context.Users.FirstOrDefault(u => u.Email == identifier || u.Username == identifier);
        }

        public bool VerifyPassword(string rawPassword, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(rawPassword, hashedPassword);
        }

        public void StartSession(HttpContext context, User user) //roles cannot be null soo no need to check roles present as enum
        {
            context.Session.SetInt32("UserId", user.UserId);
            context.Session.SetString("UserRole", user.Role.ToString()); // store as string in session
        }
        public int? GetUserIdFromSession(HttpContext context) //get user role method
        {
            return context.Session.GetInt32("UserId");
        }
        public string GetUserRoleFromSession(HttpContext context)
        {
            return context.Session.GetString("UserRole");
        }

        public User GetUserById(int userId) //user by id method from controller user id will be taking in session
        {
            return _context.Users.FirstOrDefault(u => u.UserId == userId);
        }

        public void UpdateUserEmail(int userId, string newEmail)
        {
            var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
            if (user == null) throw new ArgumentException("User not found.");

            if (string.IsNullOrWhiteSpace(newEmail) || !newEmail.Contains("@"))
                throw new ArgumentException("Invalid email format.");

            if (_context.Users.Any(u => u.Email == newEmail && u.UserId != userId))
                throw new ArgumentException("Email already taken.");

            user.Email = newEmail;
            _context.SaveChanges();
        }

        public bool IsUsernameTaken(string username)
        {
            return _context.Users.Any(u => u.Username == username);
        }



        public bool IsEmailTaken(string email)
        {
            return _context.Users.Any(u => u.Email == email);
        }

        public void RegisterUser(User user, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Password))
                throw new ArgumentException("All fields are required.");

            if (IsUsernameTaken(user.Username))
                throw new ArgumentException("Username already exists.");

            if (IsEmailTaken(user.Email))
                throw new ArgumentException("Email already exists.");

            if (user.Password != confirmPassword)
                throw new ArgumentException("Passwords do not match.");

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.Role = UserRole.CUSTOMER; // Store as enum

            _context.Users.Add(user);
            _context.SaveChanges();
        }
    }
}