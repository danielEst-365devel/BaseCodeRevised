using System.ComponentModel.DataAnnotations;

namespace BaseCode.Models.Requests.forCrudAct
{
    public class DeleteUserRequest
    {
        [Required(ErrorMessage = "Customer ID is required")]
        public string CustomerId { get; set; }
    }
}
