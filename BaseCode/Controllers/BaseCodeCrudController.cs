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
using System.Threading.Tasks;
using BaseCode.Models.Dealership.Requests;
using BaseCode.Models.Dealership.Responses;
using BaseCode.Services;

namespace BaseCode.Controllers
{
    [ApiController]
    [Route("crud")]
    public class BaseCodeCrudController : Controller
    {
        private DBCrudAct db;
        private readonly IWebHostEnvironment hostingEnvironment;
        private IHttpContextAccessor _IPAccess;
        private readonly IConfiguration _configuration;
        private readonly CarService _carService;

        private static readonly string[] Summaries = new[]
       {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public BaseCodeCrudController(DBCrudAct context, IWebHostEnvironment environment, IHttpContextAccessor accessor, IConfiguration configuration, CarService carService)
        {
            _IPAccess = accessor;
            db = context;
            hostingEnvironment = environment;
            _configuration = configuration;
            _carService = carService;
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

        #region BASIC CRUD OPERATIONS
        // START OF BASIC CRUD CONTROLLERS
        [Authorize(Policy = "CanCreateUsers")]
        [HttpPost("CreateUser")]
        public IActionResult CreateUser([FromBody] CreateUserRequest r)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = db.CreateUser(r);
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
        #endregion


        #region CAR CRUD OPERATIONS
        // CAR CRUD OPERATIONS
        [HttpPost("GetCars")]
        public IActionResult GetAllCars()
        {
            var response = _carService.GetAllCars();
            return response.IsSuccess ? Ok(response) : BadRequest(response);
        }

        [HttpPost("GetCarById")]
        public IActionResult GetCarById([FromBody] GetCarByIdRequest request)
        {
            var response = new GetCarResponse();

            if (string.IsNullOrEmpty(request.CarId))
            {
                response.IsSuccess = false;
                response.Message = "Please specify CarId.";
                return BadRequest(response);
            }

            var serviceResponse = _carService.GetCarById(request);
            return serviceResponse.IsSuccess ? Ok(serviceResponse) : BadRequest(serviceResponse);
        }

        [HttpPost("GetCarByName")]
        public IActionResult GetCarByName([FromBody] GetCarByNameRequest request)
        {
            var response = new GetCarResponse();

            if (string.IsNullOrEmpty(request.SearchTerm))
            {
                response.IsSuccess = false;
                response.Message = "Please specify SearchTerm.";
                return BadRequest(response);
            }

            var serviceResponse = _carService.GetCarByName(request);
            return serviceResponse.IsSuccess ? Ok(serviceResponse) : BadRequest(serviceResponse);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("CreateCar")]
        public IActionResult CreateCar([FromBody] CreateCarRequest request)
        {
            var response = new GetCarResponse();

            if (string.IsNullOrEmpty(request.CarModel))
            {
                response.IsSuccess = false;
                response.Message = "Please specify CarModel.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(request.CarBrand))
            {
                response.IsSuccess = false;
                response.Message = "Please specify CarBrand.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(request.CarHorsepower))
            {
                response.IsSuccess = false;
                response.Message = "Please specify CarHorsepower.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(request.CarSeater))
            {
                response.IsSuccess = false;
                response.Message = "Please specify CarSeater.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(request.CarColor))
            {
                response.IsSuccess = false;
                response.Message = "Please specify CarColor.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(request.CarPrice))
            {
                response.IsSuccess = false;
                response.Message = "Please specify CarPrice.";
                return BadRequest(response);
            }

            var serviceResponse = _carService.CreateCar(request);
            return serviceResponse.IsSuccess ? Ok(serviceResponse) : BadRequest(serviceResponse);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("UpdateCarDetails")]
        public IActionResult UpdateCar([FromBody] UpdateCarRequest request)
        {
            var response = new GetCarResponse();

            if (string.IsNullOrEmpty(request.CarId))
            {
                response.IsSuccess = false;
                response.Message = "Please specify CarId.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(request.CarModel))
            {
                response.IsSuccess = false;
                response.Message = "Please specify CarModel.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(request.CarBrand))
            {
                response.IsSuccess = false;
                response.Message = "Please specify CarBrand.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(request.CarHorsepower))
            {
                response.IsSuccess = false;
                response.Message = "Please specify CarHorsepower.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(request.CarSeater))
            {
                response.IsSuccess = false;
                response.Message = "Please specify CarSeater.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(request.CarColor))
            {
                response.IsSuccess = false;
                response.Message = "Please specify CarColor.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(request.CarPrice))
            {
                response.IsSuccess = false;
                response.Message = "Please specify CarPrice.";
                return BadRequest(response);
            }

            var serviceResponse = _carService.UpdateCar(request);
            return serviceResponse.IsSuccess ? Ok(serviceResponse) : BadRequest(serviceResponse);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("DeleteCar")]
        public IActionResult DeleteCar([FromBody] GetCarByIdRequest request)
        {
            var response = new GetCarResponse();

            if (string.IsNullOrEmpty(request.CarId))
            {
                response.IsSuccess = false;
                response.Message = "Please specify CarId.";
                return BadRequest(response);
            }

            if (!int.TryParse(request.CarId, out int carId))
            {
                response.IsSuccess = false;
                response.Message = "Invalid CarId format. Please provide a valid numeric Id.";
                return BadRequest(response);
            }

            var serviceResponse = _carService.DeleteCar(carId);
            return serviceResponse.IsSuccess ? Ok(serviceResponse) : BadRequest(serviceResponse);
        }

        #endregion


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
            try
            {
                var authHeader = Request.Headers["Authorization"].ToString();
                if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                {
                    return BadRequest(new { isSuccess = false, Message = "Invalid authorization header" });
                }

                var jwt = authHeader.Substring("Bearer ".Length).Trim();

                using (var conn = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand("DELETE FROM SESSIONS WHERE SESSION_ID = @SessionId", conn))
                    {
                        cmd.Parameters.AddWithValue("@SessionId", jwt);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        
                        // Even if no rows were affected (session might already be expired), we consider it a successful logout
                        return Ok(new { isSuccess = true, Message = "Successfully logged out" });
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception (consider using a proper logging framework)
                Console.WriteLine($"Error during logout: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, 
                    new { isSuccess = false, Message = "An error occurred during logout", Error = ex.Message });
            }
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

            var response = db.ConfirmOtp(request);

            if (!response.isSuccess)
            {
                if (response.Message.Contains("Invalid or expired OTP"))
                    return BadRequest(response);
                else
                    return StatusCode(StatusCodes.Status500InternalServerError, response);
            }

            return Ok(response);
        }


        [HttpPost("ResetPassword")]
        public IActionResult ResetPassword([FromHeader(Name = "Authorization")] string authorization, [FromBody] ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
                return StatusCode(StatusCodes.Status401Unauthorized, new { message = "Invalid token format" });

            try
            {
                string token = authorization.Substring("Bearer ".Length).Trim();
                var response = db.ResetPassword(token, request);

                if (!response.isSuccess)
                {
                    if (response.Message.Contains("Invalid token"))
                        return StatusCode(StatusCodes.Status401Unauthorized, response);
                    else if (response.Message.Contains("User not found"))
                        return StatusCode(StatusCodes.Status404NotFound, response);
                    else
                        return StatusCode(StatusCodes.Status400BadRequest, response);
                }

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { isSuccess = false, Message = "Internal server error during password reset" });
            }
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Roles")]
        public IActionResult GetRoles()
        {
            var response = db.GetRoles();
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Roles")]
        public IActionResult CreateRole([FromBody] CreateRoleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = db.CreateRole(request);
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Roles/Permissions")]  
        public IActionResult UpdateRolePermissions([FromBody] UpdateRolePermissionsRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = db.UpdateRolePermissions(request);
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("CreatePermissions")]
        public IActionResult CreatePermission([FromBody] PermissionCreateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = db.CreatePermission(request);
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("ViewAllPermissions")]
        public IActionResult ViewAllPermissions()
        {
            var response = db.ViewAllPermissions();
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("EditPermissions")]
        public IActionResult EditPermission([FromBody] PermissionEditRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = db.EditPermission(request);
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("AssignUserRole")]
        public ObjectResult AssignUserRole([FromBody] AssignUserRoleRequest request)
        {
            if (!ModelState.IsValid)
                return new BadRequestObjectResult(ModelState);

            var response = db.AssignRoleToUser(request);
            return response.isSuccess
                ? new OkObjectResult(response)
                : new BadRequestObjectResult(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Customers")]
        public IActionResult GetAllCustomers()
        {
            var response = db.GetUsersByRole("Customer");
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Admins")]
        public IActionResult GetAllAdmins()
        {
            var response = db.GetUsersByRole("Admin");
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Users/Search")]
        public IActionResult SearchUsers([FromQuery] string searchTerm, [FromQuery] string status = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest(new { isSuccess = false, Message = "Search term is required" });

            var response = db.SearchUsers(searchTerm, status);
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Users/RemoveRole")]
        public IActionResult RemoveRoleFromUser([FromBody] RemoveRoleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = db.RemoveRoleFromUser(request.UserId, request.RoleId);
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Sessions")]
        public IActionResult GetActiveSessions()
        {
            var response = db.GetActiveSessions();
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("Users/InvalidateSessions")]
        public IActionResult InvalidateUserSessions([FromBody] InvalidateSessionsRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var response = db.InvalidateUserSessions(request.UserId);
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("Statistics")]
        public IActionResult GetUserStatistics()
        {
            var response = db.GetUserStatistics();
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize]
        [HttpPost("ChangePassword")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                    return Unauthorized(new { Message = "Invalid token" });

                int userId = int.Parse(userIdClaim);
                var response = db.ChangePassword(userId, request);

                if (response.isSuccess)
                {
                    // Optionally logout after password change by invalidating all sessions
                    db.InvalidateUserSessions(userId);
                    return Ok(response);
                }
                else
                    return BadRequest(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { isSuccess = false, Message = $"Error changing password: {ex.Message}" });
            }
        }
    }
}