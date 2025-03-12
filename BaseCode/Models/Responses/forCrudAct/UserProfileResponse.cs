using BaseCode.Models.Requests.forCrudAct;
using System;
using System.Collections.Generic;

namespace BaseCode.Models.Responses.forCrudAct
{
    public class UserProfileResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int Age { get; set; }
        public DateTime? Birthday { get; set; }
        public string CivilStatus { get; set; }
        public UserAddress Address { get; set; }
        public DateTime CreateDate { get; set; }

        public List<string> Roles { get; set; } = new List<string>();
        public List<string> Permissions { get; set; } = new List<string>();
    }

}
