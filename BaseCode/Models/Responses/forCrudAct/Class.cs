using System;

namespace BaseCode.Models.Responses.forCrudAct
{
    public class CreateCustomerResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int CustomerId { get; set; }
        public DateTime CreateDate { get; set; }
    }

    // Add new response class
    public class UpdateCustomerResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public int CustomerId { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
