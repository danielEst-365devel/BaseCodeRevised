using System;
using System.ComponentModel.DataAnnotations;

namespace BaseCode.Models.Requests.forCrudAct
{
    public class UpdateCustomerByIdRequest : UpdateCustomerRequest
    {
        [Required(ErrorMessage = "Customer ID is required")]
        public string CustomerId { get; set; }

        [Required(ErrorMessage = "Account status is required")]
        [RegularExpression("^[AI]$", ErrorMessage = "Account status must be either 'A' or 'I'")]
        [StringLength(1)]
        public string AccountStatus { get; set; }
    }
}
