using System.Collections.Generic;

namespace BaseCode.Models.Responses.forCrudAct
{
    public class UserStatisticsResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public Dictionary<string, int> UserCountsByRole { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> UserCountsByStatus { get; set; } = new Dictionary<string, int>();
        public int UsersCreatedLast30Days { get; set; }
        public int FailedLoginAttempts24Hours { get; set; }
        public int ActiveSessionCount { get; set; }
    }
}
