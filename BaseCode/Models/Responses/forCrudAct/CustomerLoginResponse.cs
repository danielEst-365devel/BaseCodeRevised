namespace BaseCode.Models.Responses.forCrudAct
{
    public class CustomerLoginResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
        public int CustomerId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}
