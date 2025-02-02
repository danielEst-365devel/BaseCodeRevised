namespace BaseCode.Models.Requests
{
    public class ResetPasswordRequest
    {
        public string UserId { get; set; }
        public string NewPassword { get; set; }
    }
}
