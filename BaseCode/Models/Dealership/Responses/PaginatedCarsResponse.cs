using BaseCode.Models.Tables;
using System.Collections.Generic;

namespace BaseCode.Models.Dealership.Responses
{
    public class PaginatedCarsResponse
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; }
        public List<Cars> Cars { get; set; } = new List<Cars>();
        public int TotalRecords { get; set; }
        public int TotalPages { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;
    }
}