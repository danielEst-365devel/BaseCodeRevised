using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Text;
using BaseCode.Models.Requests;
using BaseCode.Models.Requests.forCrudAct;
using BaseCode.Models.Responses;
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

        [HttpPost("CreateCustomer")]
        public IActionResult CreateCustomer([FromBody] CreateCustomerRequest r)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = db.CreateCustomer(r);
            if (response.isSuccess)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] CustomerLoginRequest request)
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
                    Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpirationInMinutes"])),
                    Issuer = jwtSettings["Issuer"],
                    Audience = jwtSettings["Audience"],
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                response.Token = tokenHandler.WriteToken(token);
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
            var customerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            // Implement get customer profile logic here
            return Ok(new { CustomerId = customerId });
        }
    }
}