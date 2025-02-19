using System;

namespace BaseCode.Models.Responses.forCrudAct
{
    public class DeleteUserResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int CustomerId { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
