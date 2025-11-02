using System.ComponentModel.DataAnnotations;

namespace CookingNotebookWebApp.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        public string Email { get; set; } = default!;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = default!;

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; } = false;
    }
}