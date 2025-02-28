using System.ComponentModel.DataAnnotations;

namespace BaseCode.Models.Requests.forCrudAct
{
    public class RemoveRoleRequest
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Role ID is required")]
        public int RoleId { get; set; }
    }
}
