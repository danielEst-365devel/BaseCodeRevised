using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Twilio.Http;

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
            return AuthenticateResult.Fail("Unauthorized: Missing or invalid Authorization header.");
        }

        var jwt = authHeader.ToString().Substring("Bearer ".Length).Trim();
        if (string.IsNullOrEmpty(jwt))
        {
            return AuthenticateResult.Fail("Unauthorized: No session ID provided.");
        }

        // Validate JWT signature only (minimal validation)
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
                ValidateLifetime = false // Skip JWT expiration check
            }, out var validatedToken);

            // Check SESSIONS table and fetch user data
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
                            int userId = reader.GetInt32("USER_ID");
                            DateTime expiresAt = reader.GetDateTime("EXPIRES_AT");
                            reader.Close();

                            if (expiresAt < DateTime.UtcNow)
                            {
                                await DeleteExpiredSession(conn, jwt);
                                return AuthenticateResult.Fail("Unauthorized: Session expired.");
                            }

                            // Fetch roles from database
                            var roles = new List<string>();
                            using (var roleCmd = new MySqlCommand(
                                "SELECT r.ROLE_NAME FROM USER_ROLES ur " +
                                "JOIN ROLES r ON ur.ROLE_ID = r.ROLE_ID " +
                                "WHERE ur.USER_ID = @UserId", conn))
                            {
                                roleCmd.Parameters.AddWithValue("@UserId", userId);
                                using (var roleReader = await roleCmd.ExecuteReaderAsync())
                                {
                                    while (roleReader.Read())
                                    {
                                        roles.Add(roleReader.GetString("ROLE_NAME"));
                                    }
                                }
                            }

                            // Fetch permissions from database
                            var permissions = new List<string>();
                            using (var permCmd = new MySqlCommand(
                                "SELECT p.PERMISSION_NAME FROM USER_ROLES ur " +
                                "JOIN ROLE_PERMISSIONS rp ON ur.ROLE_ID = rp.ROLE_ID " +
                                "JOIN PERMISSIONS p ON rp.PERMISSION_ID = p.PERMISSION_ID " +
                                "WHERE ur.USER_ID = @UserId", conn))
                            {
                                permCmd.Parameters.AddWithValue("@UserId", userId);
                                using (var permReader = await permCmd.ExecuteReaderAsync())
                                {
                                    while (permReader.Read())
                                    {
                                        permissions.Add(permReader.GetString("PERMISSION_NAME"));
                                    }
                                }
                            }

                            // Build claims from database
                            var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
                            };
                            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
                            claims.AddRange(permissions.Select(perm => new Claim("permission", perm)));

                            var identity = new ClaimsIdentity(claims, Scheme.Name);
                            var ticket = new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name);
                            return AuthenticateResult.Success(ticket);
                        }
                        else
                        {
                            return AuthenticateResult.Fail("Unauthorized: Invalid session.");
                        }
                    }
                }
            }
        }
        catch (SecurityTokenException)
        {
            return AuthenticateResult.Fail("Unauthorized: Invalid session ID signature.");
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

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.ContentType = "text/plain";
        await Response.WriteAsync("401 Unauthorized: Access is denied due to missing or invalid credentials.");
    }

    protected override async Task HandleForbiddenAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        Response.ContentType = "text/plain";
        await Response.WriteAsync("403 Forbidden: You do not have permission to access this resource.");
    }
}