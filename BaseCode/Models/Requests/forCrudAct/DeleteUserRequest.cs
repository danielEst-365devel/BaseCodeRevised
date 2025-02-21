using System.ComponentModel.DataAnnotations;

namespace BaseCode.Models.Requests.forCrudAct
{
    public class DeleteUserRequest
    {
        [Required(ErrorMessage = "User ID is required")]
        public string UserId { get; set; }  
    }
}