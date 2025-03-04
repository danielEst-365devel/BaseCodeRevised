using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Data.Common;

public class SessionExpirationService : IHostedService, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _checkInterval;
    private Timer _timer;
    private readonly bool _useOnlineDb;

    public SessionExpirationService(IConfiguration configuration)
    {
        _configuration = configuration;
        var intervalMinutes = _configuration.GetValue<int>("SessionSettings:ExpirationCheckIntervalMinutes", 30);
        _checkInterval = TimeSpan.FromMinutes(intervalMinutes);

        // Check if we're using online database (Azure SQL)
        _useOnlineDb = bool.Parse(Environment.GetEnvironmentVariable("USE_ONLINE_DB") ?? "false");

        Console.WriteLine($"Session expiration service initialized with {(_useOnlineDb ? "ONLINE" : "LOCAL")} database.");
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(ExpireSessions, null, TimeSpan.Zero, _checkInterval); // Start immediately, then every interval
        return Task.CompletedTask;
    }

    private void ExpireSessions(object state)
    {
        try
        {
            if (_useOnlineDb)
            {
                // Use SQL Server for Azure SQL Database
                using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        "DELETE FROM SESSIONS WHERE EXPIRES_AT < @Now", conn))
                    {
                        cmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        Console.WriteLine($"Expired {rowsAffected} sessions at {DateTime.UtcNow} (SQL Server).");
                    }
                }
            }
            else
            {
                // Use MySQL for local database
                using (var conn = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(
                        "DELETE FROM SESSIONS WHERE EXPIRES_AT < @Now", conn))
                    {
                        cmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        Console.WriteLine($"Expired {rowsAffected} sessions at {DateTime.UtcNow} (MySQL).");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log the exception
            Console.WriteLine($"Error expiring sessions: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0); // Stop the timer
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
