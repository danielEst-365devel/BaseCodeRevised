using System;

namespace BaseCode.Models
{
    public class ApiLogEntry
    {
        public long ApiId { get; set; }
        public string ApiMethodName { get; set; }
        public string ApiParameters { get; set; }
        public string ApiResponse { get; set; }
        public string ApiIpAddress { get; set; }
        public DateTime CreateDate { get; set; }
        public string ApiTraceId { get; set; }

        public ApiLogEntry()
        {
            CreateDate = DateTime.Now;
            ApiTraceId = Guid.NewGuid().ToString();
        }
    }
}