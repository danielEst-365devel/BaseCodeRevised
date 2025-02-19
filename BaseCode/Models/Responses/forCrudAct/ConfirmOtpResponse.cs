namespace BaseCode.Models.Responses.forCrudAct
{
    public class ConfirmOtpResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public string Token { get; set; }
    }
}
