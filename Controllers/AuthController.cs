using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using CookingNotebookWebApp.Data;
using CookingNotebookWebApp.Models;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly string _googleClientId;
    private readonly AppDbContext _db;

    public AuthController(IConfiguration configuration, AppDbContext db)
    {
        _configuration = configuration;
        _googleClientId = _configuration["Google:ClientId"];
        _db = db;
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleTokenRequest request)
    {
        if (string.IsNullOrEmpty(request?.Token))
        {
            return BadRequest(new { message = "Không tìm thấy token" });
        }

        var settings = new GoogleJsonWebSignature.ValidationSettings()
        {
            Audience = new List<string>() { _googleClientId }
        };

        try
        {
            GoogleJsonWebSignature.Payload payload = await GoogleJsonWebSignature.ValidateAsync(request.Token, settings);

            // Tìm kiếm hoặc tạo mới người dùng
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

            if (user == null)
            {
                // Tạo người dùng mới
                user = new User
                {
                Email = payload.Email,
                FullName = payload.Name,
                GoogleId = payload.Subject,
                Role = "User", // Mặc định role
                CreatedAt = DateTime.UtcNow
                };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
            }
            else
            {
                // Cập nhật thông tin nếu cần
                user.GoogleId = payload.Subject;
                user.FullName = payload.Name;
                await _db.SaveChangesAsync();
            }

            return Ok(new
            {
            message = "Đăng nhập thành công!",
            userEmail = payload.Email,
            userId = user.UserId,
            fullName = user.FullName
            });
        }
        catch (InvalidJwtException)
        {
            return Unauthorized(new { message = "Token không hợp lệ" });
        }
    }
}
