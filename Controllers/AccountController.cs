using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using CookingNotebookWebApp.Models.ViewModels;
using CookingNotebookWebApp.Data;
using CookingNotebookWebApp.Models;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;

namespace CookingNotebookWebApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _googleClientId;

        public AccountController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _googleClientId = _configuration["Google:ClientId"];
        }

        // ================== LOGIN ==================
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Homepage", "Homepage");

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
        if (!ModelState.IsValid)
        return View(model);

        // 🔍 Tìm người dùng
        var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Email == model.Email);

        if (user == null)
        {
        ModelState.AddModelError("", "Tài khoản không tồn tại.");
        return View(model);
        }

            if (!user.Status)
            {
                ModelState.AddModelError("", "Tài khoản của bạn đang bị khóa.");
                return View(model);
            }

            // 🔒 Kiểm tra mật khẩu
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                ModelState.AddModelError("", "Mật khẩu không chính xác.");
                return View(model);
            }

            // 🪪 Tạo danh sách claim (định danh + quyền)
            var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
        new(ClaimTypes.Email, user.Email),
        new(ClaimTypes.Name, user.FullName ?? user.Email),
        new(ClaimTypes.Role, user.Role ?? "User")
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            // ✅ Đăng nhập và lưu cookie
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            // 🎯 Điều hướng theo quyền
            if (user.Role != null && user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            // Mặc định chuyển về trang người dùng
            return RedirectToAction("Homepage", "Homepage");
        }

        // ================== REGISTER ==================
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Homepage", "Homepage");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                return View(model);
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(model.Password);

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                PasswordHash = passwordHash,
                Role = "User",
                Status = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đăng ký thành công! Mời bạn đăng nhập.";
            return RedirectToAction("Login");
        }

        // ================== GOOGLE LOGIN ==================
        [HttpPost]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> GoogleLogin(string token)
        {
        if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is required");
            }

            var settings = new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new List<string>() { _googleClientId }
            };

            try
            {
                GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);

                // Tìm kiếm hoặc tạo mới người dùng
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

                if (user == null)
                {
                    // Tạo người dùng mới
                    user = new User
                    {
                    Email = payload.Email,
                    FullName = payload.Name,
                    GoogleId = payload.Subject,
                    Role = "User",
                    Status = true,
                    CreatedAt = DateTime.UtcNow
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    // Cập nhật thông tin
                    user.GoogleId = payload.Subject;
                    user.FullName = payload.Name;
                    await _context.SaveChangesAsync();
                }

                // Tạo claims và sign in
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new(ClaimTypes.Email, user.Email),
                    new(ClaimTypes.Name, user.FullName ?? user.Email),
                    new(ClaimTypes.Role, user.Role ?? "User")
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties
                );

                // Redirect theo role
                if (user.Role != null && user.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("Dashboard", "Admin");
                }

                return RedirectToAction("Homepage", "Homepage");
            }
            catch (InvalidJwtException)
            {
                return BadRequest("Invalid token");
            }
        }

        // ================== LOGOUT ==================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
