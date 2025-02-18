namespace BaseCode.Models.Requests.forCrudAct
{
    public class CreateCustomerRequest
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; } 
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
    }
}
