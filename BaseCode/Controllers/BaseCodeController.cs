using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System.Data;
using System.Text;
using BaseCode.Models.Requests;
using BaseCode.Models.Responses;
using BaseCode.Models;

namespace BaseCode.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class BaseCodeController : Controller
    {
        private DBContext db;
        private readonly IWebHostEnvironment hostingEnvironment;
        private IHttpContextAccessor _IPAccess;

        private static readonly string[] Summaries = new[]
       {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public BaseCodeController(DBContext context, IWebHostEnvironment environment, IHttpContextAccessor accessor)
        {
            _IPAccess = accessor;
            db = context;
            hostingEnvironment = environment;
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
            CreateUserResponse resp = new CreateUserResponse();

            if (string.IsNullOrEmpty(r.FirstName))
            {
                resp.Message = "Please specify Firstname.";
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.LastName))
            {
                resp.Message = "Please specify lastname.";
                return BadRequest(resp);
            }
            resp = db.CreateUserUsingSqlScript(r);
            // resp = db.CreateUserUsingSqlScript(r);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("UpdateUser")]
        public IActionResult UpdateUser([FromBody] CreateUserRequest r)
        {
            CreateUserResponse resp = new CreateUserResponse();

            if (string.IsNullOrEmpty(r.UserId.ToString()))
            {
                resp.Message = "Please specify UserId.";
                return BadRequest(resp);
            }
            resp.UserId = r.UserId;
            resp = db.UpdateUser(r);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("DeleteUser")]
        public IActionResult DeleteUser([FromBody] CreateUserRequest r)
        {
            CreateUserResponse resp = new CreateUserResponse();

            if (string.IsNullOrEmpty(r.UserId.ToString()))
            {
                resp.Message = "Please specify UserId.";
                return BadRequest(resp);
            }

            resp = db.DeleteUser(r.UserId.ToString());

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }
        [HttpPost("GetUserList")]
        public IActionResult GetUserList([FromBody] GetUserListRequest r)
        {
            GetUserListResponse resp = new GetUserListResponse();

            resp = db.GetUserList(r);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("CreateUserInfo")]
        public IActionResult CreateUserInfo([FromBody] CreateUserInfoRequest r)
        {
            CreateUserInfoResponse resp = new CreateUserInfoResponse();

            if (string.IsNullOrEmpty(r.Mobile))
            {
                resp.Message = "Please specify Mobile.";
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.Email))
            {
                resp.Message = "Please specify Email.";
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.Birthday))
            {
                resp.Message = "Please specify Birthday.";
                return BadRequest(resp);
            }
            if (string.IsNullOrEmpty(r.Country))
            {
                resp.Message = "Please specify Country.";
                return BadRequest(resp);
            }
            resp = db.CreateUserInfoUsingSqlScript(r);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("GetUserProfileList")]
        public IActionResult GetUserProfileList([FromBody] GetUserProfileListRequest r)
        {
            GetUserProfileListResponse resp = new GetUserProfileListResponse();

            resp = db.GetUserProfileList(r);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpGet("GetUserById")]
        public IActionResult GetUserById(int UserId)
        {
            if (UserId <= 0)
            {
                return BadRequest(new { Message = "Please specify a valid UserId." });
            }

            var resp = db.GetUserById(UserId);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return NotFound(resp);
        }

        [HttpPost("RegisterUser")]
        public IActionResult RegisterUser([FromBody] RegisterUserRequest r)
        {
            if (string.IsNullOrEmpty(r.FirstName))
                return BadRequest(new { Message = "First name is required." });

            if (string.IsNullOrEmpty(r.LastName))
                return BadRequest(new { Message = "Last name is required." });

            if (string.IsNullOrEmpty(r.UserName))
                return BadRequest(new { Message = "Username is required." });

            if (string.IsNullOrEmpty(r.Password))
                return BadRequest(new { Message = "Password is required." });

            var resp = db.RegisterUser(r);

            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("Login")]
        public IActionResult Login([FromBody] BaseCode.Models.Requests.LoginRequest req)
        {
            if (string.IsNullOrEmpty(req.UserName) || string.IsNullOrEmpty(req.Password))
                return BadRequest(new { Message = "Username and Password are required." });

            var resp = db.LoginUser(req);
            if (resp.isSuccess)
                return Ok(resp);
            else
                return Unauthorized(resp);
        }

        [HttpPost("ResetPassword")]
        public IActionResult ResetPassword([FromBody] BaseCode.Models.Requests.ResetPasswordRequest req)
        {
            if (string.IsNullOrEmpty(req.UserId) || string.IsNullOrEmpty(req.NewPassword))
                return BadRequest(new { Message = "Valid UserId and NewPassword are required." });

            var resp = db.ResetPassword(req);
            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("UpdateUserDetails")]
        public IActionResult UpdateUserDetails([FromBody] BaseCode.Models.Requests.UpdateUserDetailsRequest req)
        {
            if (string.IsNullOrEmpty(req.UserId) || 
                string.IsNullOrEmpty(req.FirstName) || 
                string.IsNullOrEmpty(req.LastName) || 
                string.IsNullOrEmpty(req.UserName))
                return BadRequest(new { Message = "UserId, FirstName, LastName and UserName are required." });
            
            var resp = db.UpdateUserDetails(req);
            if (resp.isSuccess)
                return Ok(resp);
            else
                return BadRequest(resp);
        }

        [HttpPost("GetUserByUserId")]
        public IActionResult GetUserByUserId([FromBody] BaseCode.Models.Requests.GetUserByUserIdRequest req)
        {
            if (string.IsNullOrEmpty(req.UserId))
                return BadRequest(new { Message = "UserId is required." });
            
            var resp = db.GetUserByUserId(req);
            if (resp.isSuccess)
                return Ok(resp);
            else
                return NotFound(resp);
        }
    }
}
