using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Tables
{
    public class User
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Status { get; set; }
    }
    public class User_info
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Mobile { get; set; }
        public string Email { get; set; }
        public string Birthday { get; set; }
        public string Country { get; set; }
       
    }
}
