using System;
using System.Data;

namespace BaseCode.Models.Tables
{
    public class Cars
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
        
        // Factory method to create a Car from DataRow
        public static Cars FromDataRow(DataRow row)
        {
            return new Cars
            {
                CarId = Convert.ToInt32(row["CAR_ID"]),
                CarModel = row["CAR_MODEL"].ToString(),
                CarBrand = row["CAR_BRAND"].ToString(),
                CarHorsepower = row["CAR_HORSEPOWER"].ToString(),
                CarSeater = row["CAR_SEATER"].ToString(),
                CarColor = row["CAR_COLOR"].ToString(),
                CarPrice = row["CAR_PRICE"].ToString(),
                CarStatus = row["CAR_STATUS"].ToString(),
                CarCreateDate = Convert.ToDateTime(row["CAR_CREATE_DATE"]),
                CarUpdateDate = Convert.ToDateTime(row["CAR_UPDATE_DATE"])
            };
        }
    }
}
