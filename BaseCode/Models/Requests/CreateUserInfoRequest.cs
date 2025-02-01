using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Requests
{
    public class CreateUserInfoRequest
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Birthday { get; set; }
        public string Country { get; set; }

    }
}
