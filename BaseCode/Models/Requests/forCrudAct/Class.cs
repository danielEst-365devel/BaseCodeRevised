using System;
using System.ComponentModel.DataAnnotations;

namespace BaseCode.Models.Requests.forCrudAct
{
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username must be 50 characters or fewer")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name must be 100 characters or fewer")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name must be 100 characters or fewer")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [StringLength(200, ErrorMessage = "Email must be 200 characters or fewer")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(255, ErrorMessage = "Password must be 255 characters or fewer")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        [StringLength(15, ErrorMessage = "Phone number must be 15 characters or fewer")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Birthday is required")]
        public DateTime Birthday { get; set; }  // Changed to non-nullable

        [Required(ErrorMessage = "Civil status is required")]
        [RegularExpression("^(?i)(SINGLE|MARRIED|DIVORCED|WIDOWED)$", ErrorMessage = "Civil status must be SINGLE, MARRIED, DIVORCED, or WIDOWED")]
        public string CivilStatus { get; set; }

        public UserAddress Address { get; set; }
    }

    public class UpdateUserRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, ErrorMessage = "Username must be 50 characters or fewer")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, ErrorMessage = "First name must be 100 characters or fewer")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, ErrorMessage = "Last name must be 100 characters or fewer")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [StringLength(200, ErrorMessage = "Email must be 200 characters or fewer")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phone Number is required")]
        [StringLength(15, ErrorMessage = "Phone number must be 15 characters or fewer")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Birthday is required")]
        public DateTime Birthday { get; set; }  // Changed to non-nullable

        [Required(ErrorMessage = "Civil status is required")]
        [RegularExpression("^(?i)(SINGLE|MARRIED|DIVORCED|WIDOWED)$", ErrorMessage = "Civil status must be SINGLE, MARRIED, DIVORCED, or WIDOWED")]
        public string CivilStatus { get; set; }

        public UserAddress Address { get; set; }
    }
   
    public class UserAddress
    {
        [Required(ErrorMessage = "Street is required")]
        [StringLength(200, ErrorMessage = "Street must be 200 characters or fewer")]
        public string Street { get; set; }

        [Required(ErrorMessage = "City is required")]
        [StringLength(100, ErrorMessage = "City must be 100 characters or fewer")]
        public string City { get; set; }

        [Required(ErrorMessage = "State is required")]
        [StringLength(100, ErrorMessage = "State must be 100 characters or fewer")]
        public string State { get; set; }

        [Required(ErrorMessage = "Zip code is required")]
        [StringLength(20, ErrorMessage = "Zip code must be 20 characters or fewer")]
        public string ZipCode { get; set; }

        [Required(ErrorMessage = "Country is required")]
        [StringLength(100, ErrorMessage = "Country must be 100 characters or fewer")]
        public string Country { get; set; }
    }

}
