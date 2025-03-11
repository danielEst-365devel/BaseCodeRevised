using System.ComponentModel.DataAnnotations;

namespace BaseCode.Models.Dealership.Requests
{
    public class GetCarByIdRequest
    {
        [Required]
        public string CarId { get; set; }
    }

    public class GetCarByNameRequest
    {
        [Required]
        public string SearchTerm { get; set; }
    }

    public class CreateCarRequest
    {
        [Required]
        public string CarModel { get; set; }
        
        [Required]
        public string CarBrand { get; set; }
        
        [Required]
        public string CarHorsepower { get; set; }
        
        [Required]
        public string CarSeater { get; set; }
        
        [Required]
        public string CarColor { get; set; }
        
        [Required]
        public string CarPrice { get; set; }
    }

    public class UpdateCarRequest : CreateCarRequest
    {
        [Required]
        public string CarId { get; set; }
    }
}