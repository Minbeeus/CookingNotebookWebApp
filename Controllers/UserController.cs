using CookingNotebookWebApp.Data;
using CookingNotebookWebApp.Models;
using CookingNotebookWebApp.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace CookingNotebookWebApp.Controllers
{
    [Authorize] // Chỉ cho phép người dùng đăng nhập
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // ================== TRANG CÁ NHÂN ==================
        public IActionResult Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = _context.Users.FirstOrDefault(u => u.UserId.ToString() == userId);
            if (user == null)
                return RedirectToAction("Login", "Account");

            return View(user);
        }

        // ================== CẬP NHẬT HỒ SƠ ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateProfile(User updatedUser)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var user = _context.Users.FirstOrDefault(u => u.UserId.ToString() == userId);
            if (user == null)
                return RedirectToAction("Login", "Account");

            user.FullName = updatedUser.FullName;
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("Profile");
        }

        // ================== TRANG ĐỔI MẬT KHẨU ==================
        [HttpGet("User/ChangePassword")]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // ================== XỬ LÝ ĐỔI MẬT KHẨU ==================
        [HttpPost("User/ChangePassword")]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            // Kiểm tra đầu vào hợp lệ
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Vui lòng nhập đầy đủ thông tin hợp lệ.";
                return View(model);
            }

            // Kiểm tra trùng mật khẩu xác nhận
            if (model.NewPassword != model.ConfirmPassword)
            {
                TempData["ErrorMessage"] = "Mật khẩu xác nhận không khớp.";
                return View(model);
            }

            // Kiểm tra độ mạnh mật khẩu (ít nhất 8 ký tự, có chữ hoa, chữ thường và số)
            if (!Regex.IsMatch(model.NewPassword, @"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d).{8,}$"))
            {
                TempData["ErrorMessage"] = "Mật khẩu phải có ít nhất 8 ký tự, gồm chữ hoa, chữ thường và số.";
                return View(model);
            }

            // Lấy thông tin người dùng hiện tại
            var email = User.Identity?.Name;
            if (email == null)
                return RedirectToAction("Login", "Account");

            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng.";
                return RedirectToAction("Login", "Account");
            }

            // Xác minh mật khẩu hiện tại
            if (!BCrypt.Net.BCrypt.Verify(model.CurrentPassword, user.PasswordHash))
            {
                TempData["ErrorMessage"] = "Mật khẩu hiện tại không đúng.";
                return View(model);
            }

            // Cập nhật mật khẩu mới
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);
            _context.SaveChanges();

            // Gửi thông báo và chuyển hướng
            TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("Profile", "User");
        }
    }
}