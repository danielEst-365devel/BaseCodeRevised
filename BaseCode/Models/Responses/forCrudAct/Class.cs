using System;

namespace BaseCode.Models.Responses.forCrudAct
{
    public class CreateUserResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
        public DateTime CreateDate { get; set; }
    }

    // Add new response class
    public class UpdateUserResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int UserId { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
