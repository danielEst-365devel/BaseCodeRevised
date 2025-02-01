using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Requests
{
    public class GetUserProfileListRequest
    {
        public int UserId { get; set; }
        public int Id { get; set; }
    }
}
