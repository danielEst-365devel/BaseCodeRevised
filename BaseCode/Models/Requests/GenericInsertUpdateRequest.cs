using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Requests
{
    public class GenericInsertUpdateRequest
    {
        public string query { get; set; }
        public string responseMessage { get; set; }
        public string errorMessage { get; set; }
        public bool isInsert { get; set; }
    }
}
