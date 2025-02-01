using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Responses
{
    public class CreateUserInfoResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
    }
}
