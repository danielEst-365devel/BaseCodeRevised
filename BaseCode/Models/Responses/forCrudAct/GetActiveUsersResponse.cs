using BaseCode.Models.Requests.forCrudAct;
using System;
using System.Collections.Generic;

namespace BaseCode.Models.Responses.forCrudAct
{
    public class GetActiveUsersResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public List<ActiveUsers> Users { get; set; }
    }

    public class ActiveUsers
    {
        public string UserName { get; internal set; }
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int Age { get; set; }
        public DateTime Birthday { get; set; }
        public string CivilStatus { get; set; }
        public DateTime CreateDate { get; set; }
        public UserAddress Address { get; set; }
        
    }
}
