using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace BaseCode.Models.Responses
{
    public class GenericGetDataResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public DataTable Data { get; set; }
    }
}
