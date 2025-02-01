using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Requests
{
    public class GetUserListRequest
    {
        public int UserId { get; set; }
        public string FirstName { get; set;  }
    }
}
