using System.ComponentModel.DataAnnotations;

namespace BaseCode.Models.Requests.forCrudAct
{
    public class ChangePasswordRequest
    {
        [Required]
        public string CurrentPassword { get; set; }
        
        [Required]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters long")]
        public string NewPassword { get; set; }
        
        [Required]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
