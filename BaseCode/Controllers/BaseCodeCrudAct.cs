using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Text;
using BaseCode.Models.Requests.forCrudAct;
using BaseCode.Models.Responses.forCrudAct;
using BaseCode.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

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

        [HttpGet("ActiveUsers")]
        public IActionResult GetActiveUsers()
        {
            var response = db.GetActiveUsers();
            if (response.isSuccess)
                return Ok(response);
            else
                return BadRequest(response);
        }

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

        [HttpPost("DeleteUser")]
        public IActionResult DeleteUser([FromBody] DeleteUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var response = db.DeleteUser(request);
                if (response.isSuccess)
                    return Ok(response);
                else
                    return BadRequest(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, Message = "Error deleting user: " + ex.Message });
            }
        }



        [HttpPost("Login")]
        public IActionResult Login([FromBody] CustomerLoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = db.LoginCustomerWithCookie(request);

            if (response.isSuccess)
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);
            
                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, response.CustomerId.ToString()),
                        new Claim(ClaimTypes.Email, response.Email),
                        new Claim(ClaimTypes.GivenName, response.FirstName),
                        new Claim(ClaimTypes.Surname, response.LastName)
                    }),
                    Expires = request.RememberMe ? 
                        DateTime.UtcNow.AddDays(14) : // 2 weeks for remember me
                        DateTime.UtcNow.AddDays(1),   // 1 day default
                    Issuer = jwtSettings["Issuer"],
                    Audience = jwtSettings["Audience"],
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                response.Token = tokenHandler.WriteToken(token);

                // Set authentication cookie
                var cookieOptions = new CookieOptions
                {
                    HttpOnly = true,
                    Secure = false, // Requires HTTPS
                    SameSite = SameSiteMode.Strict,
                    Expires = tokenDescriptor.Expires,
                };

                Response.Cookies.Append("AuthToken", response.Token, cookieOptions);
            }

            if (response.isSuccess)
                return Ok(response);
            else
                return BadRequest(response);
        }


        [HttpPost("LoginWithHeader")]
        public IActionResult LoginWithHeader([FromBody] CustomerLoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = db.LoginCustomer(request);

            if (response.isSuccess)
            {
                var jwtSettings = _configuration.GetSection("JwtSettings");
                var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);

                var tokenHandler = new JwtSecurityTokenHandler();
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, response.CustomerId.ToString()),
                        new Claim(ClaimTypes.Email, response.Email),
                        new Claim(ClaimTypes.GivenName, response.FirstName),
                        new Claim(ClaimTypes.Surname, response.LastName)
                    }),
                    Expires = request.RememberMe ?
                        DateTime.UtcNow.AddDays(14) : // 2 weeks for remember me
                        DateTime.UtcNow.AddDays(1),   // 1 day default
                    Issuer = jwtSettings["Issuer"],
                    Audience = jwtSettings["Audience"],
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                response.Token = tokenHandler.WriteToken(token);

                // Token will be sent in response body
                // Client should store it and send in Authorization: Bearer <token> header
            }

            if (response.isSuccess)
                return Ok(response);
            else
                return BadRequest(response);
        }

        // Add [Authorize] attribute to protected endpoints
        [Authorize]
        [HttpGet("customer-profile")]
        public IActionResult GetCustomerProfile()
        {
            try
            {
                var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(customerIdClaim))
                {
                    return Unauthorized(new { Message = "Invalid token" });
                }

                int customerId = int.Parse(customerIdClaim);
                var response = db.GetCustomerProfile(customerId);

                if (response.isSuccess)
                    return Ok(response);
                else
                    return BadRequest(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, Message = "Error retrieving profile: " + ex.Message });
            }
        }

        //[Authorize]
        //[HttpPut("update-profile")]
        //public IActionResult UpdateCustomerProfile([FromBody] UpdateUserRequest request)
        //{
        //    try
        //    {
        //        var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //        if (string.IsNullOrEmpty(customerIdClaim))
        //        {
        //            return Unauthorized(new { Message = "Invalid token" });
        //        }

        //        int customerId = int.Parse(customerIdClaim);
        //        var response = db.UpdateCustomer(customerId, request);

        //        if (response.isSuccess)
        //            return Ok(response);
        //        else
        //            return BadRequest(response);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { isSuccess = false, Message = "Error updating profile: " + ex.Message });
        //    }
        //}

        [HttpPost("forget-password")]
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

        [HttpPost("confirm-otp")]
        public IActionResult ConfirmOtp([FromBody] ConfirmOtpRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = db.ConfirmOtp(request); // Use the instance 'db' instead of 'DBCrudAct'
            return Ok(response);
        }


        [HttpPost("reset-password")]
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