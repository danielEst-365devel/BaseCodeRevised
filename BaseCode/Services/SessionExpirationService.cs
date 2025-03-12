using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using MySql.Data.MySqlClient;
using System;
using System.Threading;
using System.Threading.Tasks;

public class SessionExpirationService : IHostedService, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly TimeSpan _checkInterval;
    private Timer _timer;

    public SessionExpirationService(IConfiguration configuration)
    {
        _configuration = configuration;
        var intervalMinutes = _configuration.GetValue<int>("SessionSettings:ExpirationCheckIntervalMinutes", 30);
        _checkInterval = TimeSpan.FromMinutes(intervalMinutes);
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
            using (var conn = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                using (var cmd = new MySqlCommand(
                    "DELETE FROM SESSIONS WHERE EXPIRES_AT < @Now", conn))
                {
                    cmd.Parameters.AddWithValue("@Now", DateTime.UtcNow);
                    int rowsAffected = cmd.ExecuteNonQuery();
                    // Optional: Log rowsAffected for monitoring (e.g., ILogger if added)
                    Console.WriteLine($"Expired {rowsAffected} sessions at {DateTime.UtcNow}.");
                }
            }
        }
        catch (Exception ex)
        {
            // Optional: Log the exception (e.g., ILogger if added)
            Console.WriteLine($"Error expiring sessions: {ex.Message}");
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