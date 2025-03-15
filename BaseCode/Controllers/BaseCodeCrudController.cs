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
            var response = new CreateUserResponse();

            if (string.IsNullOrEmpty(r.UserName))
            {
                response.isSuccess = false;
                response.Message = "Please specify Username.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(r.FirstName))
            {
                response.isSuccess = false;
                response.Message = "Please specify First name.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(r.LastName))
            {
                response.isSuccess = false;
                response.Message = "Please specify Last name.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(r.Email))
            {
                response.isSuccess = false;
                response.Message = "Please specify Email.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(r.Password))
            {
                response.isSuccess = false;
                response.Message = "Please specify Password.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(r.PhoneNumber))
            {
                response.isSuccess = false;
                response.Message = "Please specify Phone Number.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(r.CivilStatus))
            {
                response.isSuccess = false;
                response.Message = "Please specify Civil Status.";
                return BadRequest(response);
            }

            if (r.Address != null)
            {
                if (string.IsNullOrEmpty(r.Address.Street))
                {
                    response.isSuccess = false;
                    response.Message = "Please specify Street.";
                    return BadRequest(response);
                }
                if (string.IsNullOrEmpty(r.Address.City))
                {
                    response.isSuccess = false;
                    response.Message = "Please specify City.";
                    return BadRequest(response);
                }
                if (string.IsNullOrEmpty(r.Address.State))
                {
                    response.isSuccess = false;
                    response.Message = "Please specify State.";
                    return BadRequest(response);
                }
                if (string.IsNullOrEmpty(r.Address.ZipCode))
                {
                    response.isSuccess = false;
                    response.Message = "Please specify Zip Code.";
                    return BadRequest(response);
                }
                if (string.IsNullOrEmpty(r.Address.Country))
                {
                    response.isSuccess = false;
                    response.Message = "Please specify Country.";
                    return BadRequest(response);
                }
            }

            response = db.CreateUser(r);
            if (response.isSuccess)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [Authorize(Policy = "CanViewActiveUsers")]
        [HttpPost("ActiveUsers")]
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
            var response = new UpdateUserResponse();

            if (string.IsNullOrEmpty(request.UserId))
            {
                response.isSuccess = false;
                response.Message = "Please specify User ID.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(request.UserName))
            {
                response.isSuccess = false;
                response.Message = "Please specify Username.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(request.FirstName))
            {
                response.isSuccess = false;
                response.Message = "Please specify First name.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(request.LastName))
            {
                response.isSuccess = false;
                response.Message = "Please specify Last name.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(request.Email))
            {
                response.isSuccess = false;
                response.Message = "Please specify Email.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(request.PhoneNumber))
            {
                response.isSuccess = false;
                response.Message = "Please specify Phone Number.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(request.CivilStatus))
            {
                response.isSuccess = false;
                response.Message = "Please specify Civil Status.";
                return BadRequest(response);
            }
            if (string.IsNullOrEmpty(request.AccountStatus))
            {
                response.isSuccess = false;
                response.Message = "Please specify Account Status.";
                return BadRequest(response);
            }
            if (request.AccountStatus != "A" && request.AccountStatus != "I")
            {
                response.isSuccess = false;
                response.Message = "Account Status must be either 'A' (Active) or 'I' (Inactive).";
                return BadRequest(response);
            }

            if (request.Address != null)
            {
                if (string.IsNullOrEmpty(request.Address.Street))
                {
                    response.isSuccess = false;
                    response.Message = "Please specify Street.";
                    return BadRequest(response);
                }
                if (string.IsNullOrEmpty(request.Address.City))
                {
                    response.isSuccess = false;
                    response.Message = "Please specify City.";
                    return BadRequest(response);
                }
                if (string.IsNullOrEmpty(request.Address.State))
                {
                    response.isSuccess = false;
                    response.Message = "Please specify State.";
                    return BadRequest(response);
                }
                if (string.IsNullOrEmpty(request.Address.ZipCode))
                {
                    response.isSuccess = false;
                    response.Message = "Please specify Zip Code.";
                    return BadRequest(response);
                }
                if (string.IsNullOrEmpty(request.Address.Country))
                {
                    response.isSuccess = false;
                    response.Message = "Please specify Country.";
                    return BadRequest(response);
                }
            }

            response = db.UpdateUserById(request);
            if (response.isSuccess)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("DeleteUser")]
        public IActionResult DeleteUser([FromBody] DeleteUserRequest request)
        {
            var response = new DeleteUserResponse();

            if (string.IsNullOrEmpty(request.UserId))
            {
                response.isSuccess = false;
                response.Message = "Please specify User ID.";
                return BadRequest(response);
            }

            response = db.DeleteUser(request);
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
        [HttpPost("UserProfile")]
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
        [HttpPost("UpdateProfile")]
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
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { isSuccess = false, Message = "Please specify Email." });
            }

            var response = db.RequestPasswordReset(request);

            if (response.isSuccess)
                return Ok(response);
            else
                return BadRequest(response);
        }

        [HttpPost("ConfirmOTP")]
        public IActionResult ConfirmOtp([FromBody] ConfirmOtpRequest request)
        {
            if (string.IsNullOrEmpty(request.Email))
            {
                return BadRequest(new { isSuccess = false, Message = "Please specify Email." });
            }
            if (string.IsNullOrEmpty(request.Otp))
            {
                return BadRequest(new { isSuccess = false, Message = "Please specify OTP." });
            }

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
            if (string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest(new { isSuccess = false, Message = "Please specify New Password." });
            }

            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
                return StatusCode(StatusCodes.Status401Unauthorized, new { isSuccess = false, Message = "Invalid token format" });

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
        [HttpPost("GetRoles")]
        public IActionResult GetRoles()
        {
            var response = db.GetRoles();
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("CreateRole")]
        public IActionResult CreateRole([FromBody] CreateRoleRequest request)
        {
            var response = new CreateRoleResponse();

            if (string.IsNullOrEmpty(request.RoleName))
            {
                response.isSuccess = false;
                response.Message = "Please specify Role Name.";
                return BadRequest(response);
            }

            response = db.CreateRole(request);
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("UpdateRolePermissions")]
        public IActionResult UpdateRolePermissions([FromBody] UpdateRolePermissionsRequest request)
        {
            var response = new UpdateRolePermissionsResponse();

            if (request.RoleId <= 0)
            {
                response.isSuccess = false;
                response.Message = "Please specify a valid Role ID.";
                return BadRequest(response);
            }

            if (request.PermissionIds == null || !request.PermissionIds.Any())
            {
                response.isSuccess = false;
                response.Message = "Please specify at least one Permission ID.";
                return BadRequest(response);
            }

            response = db.UpdateRolePermissions(request);
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("CreatePermissions")]
        public IActionResult CreatePermission([FromBody] PermissionCreateRequest request)
        {
            var response = new PermissionResponse();

            if (string.IsNullOrEmpty(request.PermissionName))
            {
                response.isSuccess = false;
                response.Message = "Please specify Permission Name.";
                return BadRequest(response);
            }

            response = db.CreatePermission(request);
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("ViewAllPermissions")]
        public IActionResult ViewAllPermissions()
        {
            var response = db.ViewAllPermissions();
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("EditPermissions")]
        public IActionResult EditPermission([FromBody] PermissionEditRequest request)
        {
            var response = new PermissionResponse();

            if (request.PermissionId <= 0)
            {
                response.isSuccess = false;
                response.Message = "Please specify a valid Permission ID.";
                return BadRequest(response);
            }

            if (string.IsNullOrEmpty(request.PermissionName))
            {
                response.isSuccess = false;
                response.Message = "Please specify Permission Name.";
                return BadRequest(response);
            }

            response = db.EditPermission(request);
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("AssignUserRole")]
        public IActionResult AssignUserRole([FromBody] AssignUserRoleRequest request)
        {
            var response = new AssignUserRoleResponse();

            if (string.IsNullOrEmpty(request.UserId))
            {
            response.isSuccess = false;
            response.Message = "Please specify a valid User ID.";
            return BadRequest(response);
            }

            if (string.IsNullOrEmpty(request.RoleId))
            {
            response.isSuccess = false;
            response.Message = "Please specify a valid Role ID.";
            return BadRequest(response);
            }

            response = db.AssignRoleToUser(request);
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("GetAllCustomers")]
        public IActionResult GetAllCustomers()
        {
            var response = db.GetUsersByRole("Customer");
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("GetAllAdmins")]
        public IActionResult GetAllAdmins()
        {
            var response = db.GetUsersByRole("Admin");
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("SearchUser")]
        public IActionResult SearchUsers([FromQuery] string searchTerm, [FromQuery] string status = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return BadRequest(new { isSuccess = false, Message = "Search term is required" });

            var response = db.SearchUsers(searchTerm, status);
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }
        [Authorize(Roles = "Admin")]
        [HttpPost("RemoveUserRole")]
        public IActionResult RemoveRoleFromUser([FromBody] RemoveRoleRequest request)
        {
            if (request.UserId <= 0)
            {
                return BadRequest(new { isSuccess = false, Message = "Please specify a valid User ID." });
            }

            if (request.RoleId <= 0)
            {
                return BadRequest(new { isSuccess = false, Message = "Please specify a valid Role ID." });
            }

            var response = db.RemoveRoleFromUser(request.UserId, request.RoleId);
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("GetActiveSessions")]
        public IActionResult GetActiveSessions()
        {
            var response = db.GetActiveSessions();
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("InvalidateUserSessions")]
        public IActionResult InvalidateUserSessions([FromBody] InvalidateSessionsRequest request)
        {
            if (request.UserId <= 0)
            {
                return BadRequest(new { isSuccess = false, Message = "Please specify a valid User ID." });
            }

            var response = db.InvalidateUserSessions(request.UserId);
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("GetUserStatistics")]
        public IActionResult GetUserStatistics()
        {
            var response = db.GetUserStatistics();
            return response.isSuccess ? Ok(response) : BadRequest(response);
        }

        [Authorize]
        [HttpPost("ChangePassword")]
        public IActionResult ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (string.IsNullOrEmpty(request.CurrentPassword))
            {
                return BadRequest(new { isSuccess = false, Message = "Please specify your current password." });
            }

            if (string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest(new { isSuccess = false, Message = "Please specify a new password." });
            }

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