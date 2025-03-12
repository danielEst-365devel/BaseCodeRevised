using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BaseCode.Models.Requests.forCrudAct
{
    public class CreateRoleRequest
    {
        [Required(ErrorMessage = "Role name is required")]
        [StringLength(50, ErrorMessage = "Role name must be 50 characters or fewer")]
        public string RoleName { get; set; }

        [StringLength(255, ErrorMessage = "Description must be 255 characters or fewer")]
        public string Description { get; set; }

        public List<int> PermissionIds { get; set; } = new List<int>();
    }

    public class UpdateRolePermissionsRequest
    {
        [Required(ErrorMessage = "Role ID is required")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "At least one permission is required")]
        public List<int> PermissionIds { get; set; } = new List<int>();
    }
}
