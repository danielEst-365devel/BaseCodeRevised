using BaseCode.Models;
using BaseCode.Models.Requests.forCrudAct;
using BaseCode.Models.Responses.forCrudAct;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace BaseCode.Controllers
{
    [ApiController]
    [Route("crud")]
    public class BaseCodeCrudAct : Controller
    {
        private DBCrudAct db;
        private readonly IWebHostEnvironment hostingEnvironment;
        private IHttpContextAccessor _IPAccess;
        private readonly IConfiguration _configuration;

        private static readonly string[] Summaries = new[]
       {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public BaseCodeCrudAct(DBCrudAct context, IWebHostEnvironment environment, IHttpContextAccessor accessor, IConfiguration configuration)
        {
            _IPAccess = accessor;
            db = context;
            hostingEnvironment = environment;
            _configuration = configuration;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        // START OF BASIC CRUD CONTROLLERS
        [Authorize(Policy = "CanCreateUsers")]
        [HttpPost("CreateUser")]
        public IActionResult CreateUser([FromBody] CreateUserRequest r)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = db.CreateCustomer(r);
            if (response.isSuccess)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [Authorize(Policy = "CanViewActiveUsers")]
        [HttpGet("ActiveUsers")]
        public IActionResult GetActiveUsers()
        {
            var response = db.GetActiveUsers();
            if (response.isSuccess)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [Authorize(Policy = "CanUpdateUserDetails")]
        [HttpPost("UpdateUserById")]
        public IActionResult UpdateUserById([FromBody] UpdateUserByIdRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = db.UpdateUserById(request);
            if (response.isSuccess)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("DeleteUser")]
        public IActionResult DeleteUser([FromBody] DeleteUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = db.DeleteUser(request);
            if (response.isSuccess)
                return Ok(response);
            else
                return BadRequest(response);
        }

        // END OF BASIC CRUD CONTROLLERS
    
        [HttpPost("LoginWithHeader")]
        public IActionResult LoginWithHeader([FromBody] UserLoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = db.LoginUser(request);
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost("Logout")]
        public IActionResult Logout()
        {
            var jwt = Request.Headers["Authorization"].ToString().Substring("Bearer ".Length).Trim();
            using (var conn = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                using (var cmd = new MySqlCommand("DELETE FROM SESSIONS WHERE SESSION_ID = @SessionId", conn))
                {
                    cmd.Parameters.AddWithValue("@SessionId", jwt);
                    cmd.ExecuteNonQuery();
                }
            }
            return Ok("Logged out.");
        }

        // Add [Authorize] attribute to protected endpoints
        [Authorize]
        [HttpGet("UserProfile")]
        public IActionResult GetUserProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { Message = "Invalid token" });
                }

                int userId = int.Parse(userIdClaim);
                var response = db.GetUserProfile(userId);

                if (response.isSuccess)
                {
                    // Validate that we have the user's roles and permissions
                    if (response.Roles == null || !response.Roles.Any())
                    {
                        response.Message += " (No roles assigned)";
                    }
                    if (response.Permissions == null || !response.Permissions.Any())
                    {
                        response.Message += " (No permissions assigned)";
                    }
                    return Ok(response);
                }
                else
                {
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, Message = "Error retrieving profile: " + ex.Message });
            }
        }

        [Authorize]
        [HttpPut("UpdateProfile")]
        public IActionResult UpdateUserProfile([FromBody] UpdateUserRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { Message = "Invalid token" });
                }

                int userId = int.Parse(userIdClaim);
                var response = db.UpdateUser(userId, request);

                if (response.isSuccess)
                    return Ok(response);
                else
                    return BadRequest(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, Message = "Error updating user profile: " + ex.Message });
            }
        }

        [HttpPost("ForgetPassword")]
        public IActionResult ForgetPassword([FromBody] ForgetPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = db.RequestPasswordReset(request);
            
            if (response.isSuccess)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpPost("ConfirmOTP")]
        public IActionResult ConfirmOtp([FromBody] ConfirmOtpRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = db.ConfirmOtp(request); // Use the instance 'db' instead of 'DBCrudAct'
            return Ok(response);
        }


        [HttpPost("ResetPassword")]
        public IActionResult ResetPassword([FromHeader(Name = "Authorization")] string authorization, [FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
                return Unauthorized(new { message = "Invalid token" });

            string token = authorization.Substring("Bearer ".Length).Trim();
            var response = db.ResetPassword(token, request); // Use the instance 'db' instead of 'DBCrudAct'
            return Ok(response);
        }


     

    }
}