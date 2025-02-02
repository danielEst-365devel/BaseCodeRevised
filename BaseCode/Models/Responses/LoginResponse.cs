namespace BaseCode.Models.Responses
{
    public class LoginResponse
    {
        public int UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool isSuccess { get; set; }
        public string Message { get; set; }
    }
}
