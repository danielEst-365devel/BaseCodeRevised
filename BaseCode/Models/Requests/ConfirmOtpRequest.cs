namespace BaseCode.Models.Requests
{
    public class ConfirmOtpRequest
    {
        public string CustomerId { get; set; }
        public string OTP { get; set; }
        public string NewPassword { get; set; }
    }
}
