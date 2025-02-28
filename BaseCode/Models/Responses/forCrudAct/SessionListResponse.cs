using System;
using System.Collections.Generic;

namespace BaseCode.Models.Responses.forCrudAct
{
    public class SessionListResponse
    {
        public bool isSuccess { get; set; }
        public string Message { get; set; }
        public List<SessionInfo> Sessions { get; set; } = new List<SessionInfo>();
    }

    public class SessionInfo
    {
        public string SessionId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
