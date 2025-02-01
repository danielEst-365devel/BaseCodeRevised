using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Responses
{
    public class GenericInsertUpdateResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int Id { get; set; }
    }
}
