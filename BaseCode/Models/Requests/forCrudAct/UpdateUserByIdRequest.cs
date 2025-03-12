using System;
using System.ComponentModel.DataAnnotations;

namespace BaseCode.Models.Requests.forCrudAct
{
    public class UpdateUserByIdRequest : UpdateUserRequest
    {
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; }

        [Required(ErrorMessage = "Account status is required")]
        [RegularExpression("^[AI]$", ErrorMessage = "Account status must be either 'A' (Active) or 'I' (Inactive)")]
        [StringLength(1)]
        public string AccountStatus { get; set; }
    }
}