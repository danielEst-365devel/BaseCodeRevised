using System.Collections.Generic;

namespace BaseCode.Models.Responses.forCrudAct
{
    public class PermissionResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int PermissionId { get; set; }
        public string PermissionName { get; set; }
        public string Description { get; set; }
    }

    public class PermissionListResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public List<PermissionResponse> Permissions { get; set; } = new List<PermissionResponse>();
    }

    public class AssignUserRoleResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}