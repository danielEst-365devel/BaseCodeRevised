using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

public class SessionAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;

    public SessionAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader) ||
            !authHeader.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var jwt = authHeader.ToString().Substring("Bearer ".Length).Trim();
        if (string.IsNullOrEmpty(jwt))
        {
            return AuthenticateResult.Fail("No JWT provided.");
        }

        // Validate JWT signature
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);

        try
        {
            var principal = tokenHandler.ValidateToken(jwt, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidateAudience = true,
                ValidAudience = jwtSettings["Audience"],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero // Enforce exact expiration
            }, out var validatedToken);

            // Check SESSIONS table
            using (var conn = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await conn.OpenAsync();
                using (var cmd = new MySqlCommand(
                    "SELECT USER_ID, EXPIRES_AT FROM SESSIONS WHERE SESSION_ID = @SessionId", conn))
                {
                    cmd.Parameters.AddWithValue("@SessionId", jwt);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            DateTime expiresAt = reader.GetDateTime("EXPIRES_AT");
                            if (expiresAt < DateTime.UtcNow)
                            {
                                reader.Close();
                                await DeleteExpiredSession(conn, jwt);
                                return AuthenticateResult.Fail("Session expired.");
                            }

                            var identity = new ClaimsIdentity(principal.Claims, Scheme.Name);
                            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
                            return AuthenticateResult.Success(ticket);
                        }
                        else
                        {
                            return AuthenticateResult.Fail("Invalid session.");
                        }
                    }
                }
            }
        }
        catch (SecurityTokenException)
        {
            return AuthenticateResult.Fail("Invalid JWT.");
        }
    }

    private async Task DeleteExpiredSession(MySqlConnection conn, string sessionId)
    {
        using (var cmd = new MySqlCommand("DELETE FROM SESSIONS WHERE SESSION_ID = @SessionId", conn))
        {
            cmd.Parameters.AddWithValue("@SessionId", sessionId);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
