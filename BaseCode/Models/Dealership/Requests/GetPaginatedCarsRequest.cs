using System.ComponentModel.DataAnnotations;
using BaseCode.Models.Enums;

namespace BaseCode.Models.Dealership.Requests
{
    public class GetPaginatedCarsRequest
    {
        [Range(1, int.MaxValue, ErrorMessage = "Page number must be greater than 0")]
        public int PageNumber { get; set; } = 1;
        
        [Range(1, 100, ErrorMessage = "Page size must be between 1 and 100")]
        public int PageSize { get; set; } = 15;

        public SortBy SortBy { get; set; } = SortBy.Id;
        
        public SortDirection SortDirection { get; set; } = SortDirection.Ascending;
    }
}