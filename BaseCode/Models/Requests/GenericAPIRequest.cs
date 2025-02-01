using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Requests
{
    public class GenericAPIRequest
    {
        public string ApiKey { get; set; }
        public string SessionId { get; set; }
    }
}
