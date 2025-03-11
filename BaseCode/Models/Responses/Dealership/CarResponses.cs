using System.Collections.Generic;
using BaseCode.Models.Tables;

namespace BaseCode.Models.Dealership.Responses
{
    public class BasicResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
    }

    public class GetAllCarsResponse : BasicResponse
    {
        public GetAllCarsResponse()
        {
            Cars = new List<Car>();
        }

        public List<Car> Cars { get; set; }
    }

    public class GetCarResponse : BasicResponse
    {
        public Car Car { get; set; }
        public List<Car> Cars { get; set; }
    }
}