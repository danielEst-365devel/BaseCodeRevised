using System.ComponentModel.DataAnnotations;

namespace BaseCode.Models.Requests.forCrudAct
{
    public class InvalidateSessionsRequest
    {
        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }
    }
}
