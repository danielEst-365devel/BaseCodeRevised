﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BaseCode.Models.Requests.forCrudAct
{
    public class PermissionCreateRequest
    {
        public string PermissionName { get; set; }
        public string Description { get; set; }
    }

    public class PermissionEditRequest
    {
        public int PermissionId { get; set; }
        public string PermissionName { get; set; }
        public string Description { get; set; }
    }

    public class AssignUserRoleRequest
    {
        public string UserId { get; set; }
        public string RoleId { get; set; }
    }
}