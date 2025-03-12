namespace BaseCode.Models.Responses.forCrudAct
{
    public class UserLoginResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public string SessionId { get; set; } // Changed from Token, holds the JWT
        public int UserId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}