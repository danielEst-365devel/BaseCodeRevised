using BaseCode.Models;
using Microsoft.AspNetCore.Http;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;

namespace BaseCode.Services
{
    public class ApiLogService
    {
        private readonly string _connectionString;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiLogService(string connectionString, IHttpContextAccessor httpContextAccessor)
        {
            _connectionString = connectionString;
            _httpContextAccessor = httpContextAccessor;
        }

        public string LogApiCall(string methodName, object parameters, object response = null)
        {
            var logEntry = new ApiLogEntry
            {
                ApiMethodName = methodName,
                ApiParameters = JsonConvert.SerializeObject(parameters, Formatting.None, 
                    new JsonSerializerSettings { 
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    }),
                ApiResponse = response != null ? JsonConvert.SerializeObject(response, Formatting.None, 
                    new JsonSerializerSettings { 
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    }) : null,
                ApiIpAddress = GetClientIpAddress()
            };

            SaveLogEntry(logEntry);
            return logEntry.ApiTraceId;
        }

        public void UpdateApiLog(string traceId, object response)
        {
            if (string.IsNullOrEmpty(traceId))
                return;

            try
            {
                string responseJson = JsonConvert.SerializeObject(response, Formatting.None, 
                    new JsonSerializerSettings { 
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore
                    });

                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand("UPDATE API_LOG SET API_RESPONSE = @Response WHERE API_TRACE_ID = @TraceId", connection))
                    {
                        command.Parameters.AddWithValue("@Response", responseJson);
                        command.Parameters.AddWithValue("@TraceId", traceId);
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating API log: {ex.Message}");
            }
        }

        private void SaveLogEntry(ApiLogEntry logEntry)
        {
            try
            {
                using (var connection = new MySqlConnection(_connectionString))
                {
                    connection.Open();
                    using (var command = new MySqlCommand(@"
                        INSERT INTO API_LOG (API_METHOD_NAME, API_PARAMETERS, API_RESPONSE, API_IP_ADDRESS, API_TRACE_ID) 
                        VALUES (@MethodName, @Parameters, @Response, @IpAddress, @TraceId)", connection))
                    {
                        command.Parameters.AddWithValue("@MethodName", logEntry.ApiMethodName);
                        command.Parameters.AddWithValue("@Parameters", logEntry.ApiParameters ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Response", logEntry.ApiResponse ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@IpAddress", logEntry.ApiIpAddress ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@TraceId", logEntry.ApiTraceId);
                        command.ExecuteNonQuery();
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving API log entry: {ex.Message}");
            }
        }

        private string GetClientIpAddress()
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                if (httpContext == null)
                    return null;

                string ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
                
                // Check for forwarded headers (if behind a proxy)
                if (httpContext.Request.Headers.ContainsKey("X-Forwarded-For"))
                {
                    ipAddress = httpContext.Request.Headers["X-Forwarded-For"];
                    // X-Forwarded-For may contain multiple IPs - get the first one
                    if (ipAddress.Contains(","))
                    {
                        ipAddress = ipAddress.Split(',')[0].Trim();
                    }
                }

                return ipAddress;
            }
            catch
            {
                return null;
            }
        }
    }
}