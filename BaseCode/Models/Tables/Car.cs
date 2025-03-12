using System;

namespace BaseCode.Models.Tables
{
    public class Car
    {
        public int CarId { get; set; }
        public string CarModel { get; set; }
        public string CarBrand { get; set; }
        public string CarHorsepower { get; set; }
        public string CarSeater { get; set; }
        public string CarColor { get; set; }
        public string CarPrice { get; set; }
        public string CarStatus { get; set; }
        public DateTime CarCreateDate { get; set; }
        public DateTime CarUpdateDate { get; set; }
    }
}
