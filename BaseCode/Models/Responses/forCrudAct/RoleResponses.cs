using System.Collections.Generic;

namespace BaseCode.Models.Responses.forCrudAct
{
    public class RolePermission
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; }
        public string Description { get; set; }
    }

    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public string Description { get; set; }
        public List<RolePermission> Permissions { get; set; } = new List<RolePermission>();
    }

    public class GetRolesResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public List<Role> Roles { get; set; } = new List<Role>();
    }

    public class CreateRoleResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }

    public class UpdateRolePermissionsResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int RoleId { get; set; }
        public List<RolePermission> UpdatedPermissions { get; set; } = new List<RolePermission>();
    }
}
