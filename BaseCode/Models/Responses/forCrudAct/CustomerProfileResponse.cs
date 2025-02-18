using BaseCode.Models.Requests.forCrudAct;
using System;

namespace BaseCode.Models.Responses.forCrudAct
{
    public class CustomerProfileResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int CustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int Age { get; set; }
        public DateTime? Birthday { get; set; }
        public string CivilStatus { get; set; }
        public CustomerAddress Address { get; set; }
        public DateTime CreateDate { get; set; }
    }

}
