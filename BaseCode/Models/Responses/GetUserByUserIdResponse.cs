using System.Collections.Generic;

namespace BaseCode.Models.Responses
{
    public class GetUserByUserIdResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public Dictionary<string, string> Data { get; set; }
    }
}
