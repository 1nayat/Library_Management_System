using Library_Management_System.Models;
using Library_Management_System.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BCrypt.Net;

namespace Library_Management_System.Controllers
{
    public class AuthController : Controller
    {
        private readonly IUserRepository _userRepo;
        private readonly IConfiguration _config;

        public AuthController(IUserRepository userRepo, IConfiguration config)
        {
            _userRepo = userRepo;
            _config = config;
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginPost(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Email and password are required.");
                return View("Login");
            }

            var user = await _userRepo.GetByEmailAsync(email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid credentials.");
                return View("Login");
            }

            var token = GenerateJwtToken(user);

            Response.Cookies.Append("jwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false, 
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(_config.GetValue<int>("Jwt:ExpiryMinutes"))
            });

            return user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase)
                ? RedirectToAction("Dashboard", "Admin")
                : RedirectToAction("Dashboard", "Student");
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterPost(string name, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Email and password are required.");
                return View("Register");
            }

            var existing = await _userRepo.GetByEmailAsync(email);
            if (existing != null)
            {
                ModelState.AddModelError("", "Email is already registered.");
                return View("Register");
            }

            var user = new User
            {
                Name = name.Trim(),
                Email = email.Trim(),
                Role = "Student",
                CreatedAt = DateTime.UtcNow,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
            };

            var id = await _userRepo.CreateAsync(user);
            user.Id = id;

        
            var token = GenerateJwtToken(user);
            Response.Cookies.Append("jwtToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddMinutes(_config.GetValue<int>("Jwt:ExpiryMinutes"))
            });

            return RedirectToAction("Dashboard", "Student");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            if (Request.Cookies.ContainsKey("jwtToken"))
                Response.Cookies.Delete("jwtToken");

            return RedirectToAction("Login");
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSection = _config.GetSection("Jwt");
            var key = jwtSection.GetValue<string>("Key");
            var issuer = jwtSection.GetValue<string>("Issuer");
            var audience = jwtSection.GetValue<string>("Audience");
            var expiryMinutes = jwtSection.GetValue<int>("ExpiryMinutes");

            var keyBytes = Encoding.UTF8.GetBytes(key);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name ?? ""),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role) 
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
