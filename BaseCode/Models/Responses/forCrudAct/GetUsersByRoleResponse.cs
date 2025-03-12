using System;
using System.Collections.Generic;
using BaseCode.Models.Requests.forCrudAct;

namespace BaseCode.Models.Responses.forCrudAct
{
    public class GetUsersByRoleResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public List<UserDetail> Users { get; set; } = new List<UserDetail>();
    }

    public class UserDetail
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int Age { get; set; }
        public DateTime Birthday { get; set; }
        public string CivilStatus { get; set; }
        public string AccountStatus { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public UserAddress Address { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
