using System.ComponentModel.DataAnnotations;

namespace SwineBreedingManager.Models
{
    public class UserViewModel
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Quyền hạn")]
        public string Role { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string? Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string? ConfirmPassword { get; set; }
    }
}
