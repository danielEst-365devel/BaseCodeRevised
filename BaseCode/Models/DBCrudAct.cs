﻿using BaseCode.Models.Requests.forCrudAct;
using BaseCode.Models.Responses.forCrudAct;
using BaseCode.Models.Tables;
using BaseCode.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BaseCode.Models
{
    public class DBCrudAct
    {
        public string ConnectionString { get; set; }
        private readonly IConfiguration _configuration;

        public DBCrudAct(string connStr, IConfiguration configuration)
        {
            this.ConnectionString = connStr;
            _configuration = configuration;
        }
        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        /*
        public class DBCrudAct
        {
           private readonly IConfiguration _configuration;
           public string ConnectionString { get; set; }

           public DBCrudAct(string connStr, IConfiguration configuration)
           {
               this.ConnectionString = connStr;
               this._configuration = configuration;
           }
        */


        // START OF CRUD METHODS
        public CreateUserResponse CreateUser(CreateUserRequest r)
        {
            CreateUserResponse resp = new CreateUserResponse();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();

                    MySqlCommand checkEmail = new MySqlCommand(
                        "SELECT COUNT(*) FROM USERS WHERE EMAIL = @Email", conn);
                    checkEmail.Parameters.AddWithValue("@Email", r.Email);
                    int emailCount = Convert.ToInt32(checkEmail.ExecuteScalar());

                    if (emailCount > 0)
                    {
                        resp.isSuccess = false;
                        resp.Message = "Email already exists";
                        conn.Close();
                        return resp;
                    }

                    MySqlCommand checkUserName = new MySqlCommand(
                        "SELECT COUNT(*) FROM USERS WHERE USER_NAME = @UserName", conn);
                    checkUserName.Parameters.AddWithValue("@UserName", r.UserName);
                    int userNameCount = Convert.ToInt32(checkUserName.ExecuteScalar());

                    if (userNameCount > 0)
                    {
                        resp.isSuccess = false;
                        resp.Message = "UserName already exists";
                        conn.Close();
                        return resp;
                    }

                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            string hashedPassword = PasswordHasher.HashPassword(r.Password);
                            DateTime now = DateTime.Now;

                            MySqlCommand userCmd = new MySqlCommand(
                                "INSERT INTO USERS (USER_NAME, FIRST_NAME, LAST_NAME, EMAIL, PASSWORD, PHONE_NUMBER, BIRTHDAY, CIVIL_STATUS, CREATEDATE) " +
                                "VALUES (@UserName, @FirstName, @LastName, @Email, @Password, @PhoneNumber, @Birthday, @CivilStatus, @CreateDate);",
                                conn, transaction);

                            userCmd.Parameters.AddWithValue("@UserName", r.UserName);
                            userCmd.Parameters.AddWithValue("@FirstName", r.FirstName);
                            userCmd.Parameters.AddWithValue("@LastName", r.LastName);
                            userCmd.Parameters.AddWithValue("@Email", r.Email);
                            userCmd.Parameters.AddWithValue("@Password", hashedPassword);
                            userCmd.Parameters.AddWithValue("@PhoneNumber", r.PhoneNumber);
                            userCmd.Parameters.AddWithValue("@Birthday", r.Birthday);
                            userCmd.Parameters.AddWithValue("@CivilStatus", r.CivilStatus.ToUpper());
                            userCmd.Parameters.AddWithValue("@CreateDate", now);

                            userCmd.ExecuteNonQuery();
                            int UserId = (int)userCmd.LastInsertedId;

                            if (r.Address != null)
                            {
                                MySqlCommand addressCmd = new MySqlCommand(
                                    "INSERT INTO USER_ADDRESSES (USER_ID, STREET, CITY, STATE, ZIPCODE, COUNTRY, CREATEDATE) " +
                                    "VALUES (@UserId, @Street, @City, @State, @ZipCode, @Country, @CreateDate);",
                                    conn, transaction);

                                addressCmd.Parameters.AddWithValue("@UserId", UserId);
                                addressCmd.Parameters.AddWithValue("@Street", r.Address.Street);
                                addressCmd.Parameters.AddWithValue("@City", r.Address.City);
                                addressCmd.Parameters.AddWithValue("@State", r.Address.State);
                                addressCmd.Parameters.AddWithValue("@ZipCode", r.Address.ZipCode);
                                addressCmd.Parameters.AddWithValue("@Country", r.Address.Country);
                                addressCmd.Parameters.AddWithValue("@CreateDate", now);

                                addressCmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            resp.UserId = UserId; // You may later refactor this to UserId if needed
                            resp.CreateDate = now;
                            resp.isSuccess = true;
                            resp.Message = "User created successfully.";
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "User creation failed: " + ex.Message;
            }
            return resp;
        }

        public GetActiveUsersResponse GetActiveUsers()
        {
            var response = new GetActiveUsersResponse
            {
                Users = new List<ActiveUsers>()
            };

            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();

                    string sql = @"
                        SELECT 
                            c.USER_NAME, c.USER_ID, c.FIRST_NAME, c.LAST_NAME, c.EMAIL, c.PHONE_NUMBER,
                            c.AGE, c.BIRTHDAY, c.CIVIL_STATUS, c.CREATEDATE,
                            ca.STREET, ca.CITY, ca.STATE, ca.ZIPCODE, ca.COUNTRY
                        FROM USERS c
                        LEFT JOIN USER_ADDRESSES ca ON c.USER_ID = ca.USER_ID
                        WHERE c.ACCOUNT_STATUS = 'A'
                        ORDER BY c.USER_ID";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var user = new ActiveUsers
                                {
                                    UserName = reader.GetString("USER_NAME"),
                                    UserId = reader.GetInt32("USER_ID"),
                                    FirstName = reader.GetString("FIRST_NAME"),
                                    LastName = reader.GetString("LAST_NAME"),
                                    Email = reader.GetString("EMAIL"),
                                    PhoneNumber = reader.IsDBNull("PHONE_NUMBER") ? null : reader.GetString("PHONE_NUMBER"),
                                    Age = reader.GetInt32("AGE"),
                                    Birthday = reader.GetDateTime("BIRTHDAY"),
                                    CivilStatus = reader.GetString("CIVIL_STATUS"),
                                    CreateDate = reader.GetDateTime("CREATEDATE"),
                                    Address = !reader.IsDBNull("STREET") ? new UserAddress
                                    {
                                        Street = reader.GetString("STREET"),
                                        City = reader.GetString("CITY"),
                                        State = reader.GetString("STATE"),
                                        ZipCode = reader.GetString("ZIPCODE"),
                                        Country = reader.GetString("COUNTRY")
                                    } : null
                                };
                                response.Users.Add(user);
                            }
                        }
                    }
                }

                response.isSuccess = true;
                response.Message = "Active users retrieved successfully.";
            }
            catch (Exception ex)
            {
                response.isSuccess = false;
                response.Message = "Error retrieving active users: " + ex.Message;
            }

            return response;
        }

        public UpdateUserResponse UpdateUserById(UpdateUserByIdRequest r)
        {
            var response = new UpdateUserResponse();
            try
            {
                if (!int.TryParse(r.UserId, out int userId))
                {
                    response.isSuccess = false;
                    response.Message = "Invalid User ID format.";
                    return response;
                }

                using (var conn = GetConnection())
                {
                    conn.Open();

                    // Check if user exists
                    using (var checkCmd = new MySqlCommand(
                        "SELECT COUNT(*) FROM USERS WHERE USER_ID = @UserId", conn))
                    {
                        checkCmd.Parameters.AddWithValue("@UserId", userId);
                        int exists = Convert.ToInt32(checkCmd.ExecuteScalar());
                        if (exists == 0)
                        {
                            response.isSuccess = false;
                            response.Message = "User not found.";
                            return response;
                        }
                    }

                    // Check if email is already used by another user
                    using (var emailCmd = new MySqlCommand(
                        "SELECT COUNT(*) FROM USERS WHERE EMAIL = @Email AND USER_ID != @UserId", conn))
                    {
                        emailCmd.Parameters.AddWithValue("@Email", r.Email);
                        emailCmd.Parameters.AddWithValue("@UserId", userId);
                        int emailExists = Convert.ToInt32(emailCmd.ExecuteScalar());
                        if (emailExists > 0)
                        {
                            response.isSuccess = false;
                            response.Message = "Email already exists for another user.";
                            return response;
                        }
                    }

                    // Check if username is already used by another user
                    using (var usernameCmd = new MySqlCommand(
                        "SELECT COUNT(*) FROM USERS WHERE USER_NAME = @UserName AND USER_ID != @UserId", conn))
                    {
                        usernameCmd.Parameters.AddWithValue("@UserName", r.UserName);
                        usernameCmd.Parameters.AddWithValue("@UserId", userId);
                        int usernameExists = Convert.ToInt32(usernameCmd.ExecuteScalar());
                        if (usernameExists > 0)
                        {
                            response.isSuccess = false;
                            response.Message = "Username already exists for another user.";
                            return response;
                        }
                    }

                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            DateTime now = DateTime.Now;

                            // Update user details including account status and username
                            string updateUserQuery = @"
                            UPDATE USERS 
                            SET USER_NAME = @UserName,
                                FIRST_NAME = @FirstName,
                                LAST_NAME = @LastName,
                                EMAIL = @Email,
                                PHONE_NUMBER = @PhoneNumber,
                                BIRTHDAY = @Birthday,
                                CIVIL_STATUS = @CivilStatus,
                                ACCOUNT_STATUS = @AccountStatus,
                                UPDATEDATE = @UpdateDate
                            WHERE USER_ID = @UserId";

                            using (var cmd = new MySqlCommand(updateUserQuery, conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@UserId", userId);
                                cmd.Parameters.AddWithValue("@UserName", r.UserName);
                                cmd.Parameters.AddWithValue("@FirstName", r.FirstName);
                                cmd.Parameters.AddWithValue("@LastName", r.LastName);
                                cmd.Parameters.AddWithValue("@Email", r.Email);
                                cmd.Parameters.AddWithValue("@PhoneNumber", r.PhoneNumber); // nullable in schema
                                cmd.Parameters.AddWithValue("@Birthday", r.Birthday);
                                cmd.Parameters.AddWithValue("@CivilStatus", r.CivilStatus.ToUpper()); // Ensure ENUM compatibility
                                cmd.Parameters.AddWithValue("@AccountStatus", r.AccountStatus.ToUpper()); // Ensure ENUM compatibility
                                cmd.Parameters.AddWithValue("@UpdateDate", now);

                                cmd.ExecuteNonQuery();
                            }

                            // Handle address update (assuming one address per user)
                            if (r.Address != null)
                            {
                                // First, check if an address exists for this user
                                string upsertAddressQuery = @"
                            INSERT INTO USER_ADDRESSES 
                                (USER_ID, STREET, CITY, STATE, ZIPCODE, COUNTRY, CREATEDATE)
                            VALUES 
                                (@UserId, @Street, @City, @State, @ZipCode, @Country, @CreateDate)
                            ON DUPLICATE KEY UPDATE
                                STREET = @Street,
                                CITY = @City,
                                STATE = @State,
                                ZIPCODE = @ZipCode,
                                COUNTRY = @Country,
                                UPDATEDATE = @CreateDate";

                                // Note: This requires a UNIQUE KEY on USER_ID in USER_ADDRESSES (see schema change below)
                                using (var addrCmd = new MySqlCommand(upsertAddressQuery, conn, transaction))
                                {
                                    addrCmd.Parameters.AddWithValue("@UserId", userId);
                                    addrCmd.Parameters.AddWithValue("@Street", r.Address.Street);
                                    addrCmd.Parameters.AddWithValue("@City", r.Address.City);
                                    addrCmd.Parameters.AddWithValue("@State", r.Address.State);
                                    addrCmd.Parameters.AddWithValue("@ZipCode", r.Address.ZipCode);
                                    addrCmd.Parameters.AddWithValue("@Country", r.Address.Country);
                                    addrCmd.Parameters.AddWithValue("@CreateDate", now);

                                    addrCmd.ExecuteNonQuery();
                                }
                            }

                            transaction.Commit();
                            response.UserId = userId;
                            response.UpdateDate = now;
                            response.isSuccess = true;
                            response.Message = "User updated successfully.";
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            response.isSuccess = false;
                            response.Message = $"Error during update: {ex.Message}";
                            return response;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.isSuccess = false;
                response.Message = $"Error updating user: {ex.Message}";
            }
            return response;
        }

        public DeleteUserResponse DeleteUser(DeleteUserRequest request)
        {
            var response = new DeleteUserResponse();
            try
            {
                if (!int.TryParse(request.UserId, out int userId))
                {
                    response.isSuccess = false;
                    response.Message = "Invalid User ID format.";
                    return response;
                }

                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            var checkCmd = new MySqlCommand(
                                "SELECT COUNT(*) FROM USERS WHERE USER_ID = @UserId AND ACCOUNT_STATUS = 'A'",
                                conn, transaction);
                            checkCmd.Parameters.AddWithValue("@UserId", userId);
                            int exists = Convert.ToInt32(checkCmd.ExecuteScalar());

                            if (exists == 0)
                            {
                                response.isSuccess = false;
                                response.Message = "User not found or already inactive.";
                                return response;
                            }

                            // Update account status to 'I'
                            DateTime now = DateTime.Now;
                            var updateCmd = new MySqlCommand(
                                "UPDATE USERS SET ACCOUNT_STATUS = 'I', UPDATEDATE = @UpdateDate " +
                                "WHERE USER_ID = @UserId",
                                conn, transaction);
                            updateCmd.Parameters.AddWithValue("@UserId", userId);
                            updateCmd.Parameters.AddWithValue("@UpdateDate", now);

                            updateCmd.ExecuteNonQuery();

                            transaction.Commit();
                            response.UserId = userId;
                            response.UpdateDate = now;
                            response.isSuccess = true;
                            response.Message = "User deleted successfully.";
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            response.isSuccess = false;
                            response.Message = $"Error during deletion: {ex.Message}";
                            return response;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.isSuccess = false;
                response.Message = $"Error deleting user: {ex.Message}";
            }
            return response;
        }

        // END OF CRUD METHODS


        public UserLoginResponse LoginUser(UserLoginRequest r)
        {
            var resp = new UserLoginResponse();
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var cmd = new MySqlCommand(
                        "SELECT USER_ID, EMAIL, PASSWORD, FIRST_NAME, LAST_NAME, ACCOUNT_STATUS " +
                        "FROM USERS WHERE EMAIL = @Email", conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", r.Email);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int userId = Convert.ToInt32(reader["USER_ID"]);
                                string email = reader["EMAIL"].ToString();
                                string storedHash = reader["PASSWORD"].ToString();
                                string firstName = reader["FIRST_NAME"].ToString();
                                string lastName = reader["LAST_NAME"].ToString();
                                string accountStatus = reader["ACCOUNT_STATUS"].ToString();

                                reader.Close();

                                if (accountStatus != "A")
                                {
                                    resp.isSuccess = false;
                                    resp.Message = "403 Forbidden: Account inactive";
                                    return resp;
                                }

                                using (var countCmd = new MySqlCommand(
                                    "SELECT COUNT(*) FROM FAILED_LOGINS WHERE USER_ID = @UserId", conn))
                                {
                                    countCmd.Parameters.AddWithValue("@UserId", userId);
                                    int failedAttempts = Convert.ToInt32(countCmd.ExecuteScalar());

                                    if (failedAttempts >= 5)
                                    {
                                        resp.isSuccess = false;
                                        resp.Message = "403 Forbidden: Account locked due to too many failed login attempts";
                                        return resp;
                                    }
                                }

                                bool verified = PasswordHasher.VerifyPassword(r.Password, storedHash);

                                if (verified)
                                {
                                    using (var deleteCmd = new MySqlCommand(
                                        "DELETE FROM FAILED_LOGINS WHERE USER_ID = @UserId", conn))
                                    {
                                        deleteCmd.Parameters.AddWithValue("@UserId", userId);
                                        deleteCmd.ExecuteNonQuery();
                                    }

                                    // Generate minimal JWT (no roles, permissions, or expiration in claims)
                                    var jwtSettings = GetJwtSettings();
                                    var key = Encoding.ASCII.GetBytes(jwtSettings["SecretKey"]);
                                    var tokenHandler = new JwtSecurityTokenHandler();
                                    var claims = new List<Claim>
                            {
                                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // Unique ID
                            };

                                    var tokenDescriptor = new SecurityTokenDescriptor
                                    {
                                        Subject = new ClaimsIdentity(claims),
                                        // No Expires here; expiration managed by SESSIONS table
                                        Issuer = jwtSettings["Issuer"],
                                        Audience = jwtSettings["Audience"],
                                        SigningCredentials = new SigningCredentials(
                                            new SymmetricSecurityKey(key),
                                            SecurityAlgorithms.HmacSha256Signature)
                                    };

                                    var token = tokenHandler.CreateToken(tokenDescriptor);
                                    string jwt = tokenHandler.WriteToken(token);

                                    // Store JWT in SESSIONS with expiration
                                    DateTime expiresAt = r.RememberMe ? DateTime.UtcNow.AddDays(14) : DateTime.UtcNow.AddDays(1);
                                    using (var sessionCmd = new MySqlCommand(
                                        "INSERT INTO SESSIONS (SESSION_ID, USER_ID, EXPIRES_AT) " +
                                        "VALUES (@SessionId, @UserId, @ExpiresAt)", conn))
                                    {
                                        sessionCmd.Parameters.AddWithValue("@SessionId", jwt);
                                        sessionCmd.Parameters.AddWithValue("@UserId", userId);
                                        sessionCmd.Parameters.AddWithValue("@ExpiresAt", expiresAt);
                                        sessionCmd.ExecuteNonQuery();
                                    }

                                    resp.isSuccess = true;
                                    resp.UserId = userId;
                                    resp.Email = email;
                                    resp.FirstName = firstName;
                                    resp.LastName = lastName;
                                    resp.SessionId = jwt;
                                    resp.Message = "Login successful";
                                }
                                else
                                {
                                    using (var insertCmd = new MySqlCommand(
                                        "INSERT INTO FAILED_LOGINS (USER_ID) VALUES (@UserId)", conn))
                                    {
                                        insertCmd.Parameters.AddWithValue("@UserId", userId);
                                        insertCmd.ExecuteNonQuery();
                                    }

                                    resp.isSuccess = false;
                                    resp.Message = "401 Unauthorized: Invalid password";
                                }
                            }
                            else
                            {
                                resp.isSuccess = false;
                                resp.Message = "401 Unauthorized: Email not found";
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "Login failed: " + ex.Message;
            }
            return resp;
        }

        // Placeholder for JWT settings retrieval (adjust based on your setup)
        private IConfigurationSection GetJwtSettings()
        {
            return _configuration.GetSection("JwtSettings");
        }

        public UserProfileResponse GetUserProfile(int userId)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                var response = new UserProfileResponse();

                try
                {
                    // First get the user basic info and address
                    string userQuery = @"SELECT 
                        u.USER_ID, u.USER_NAME, u.FIRST_NAME, u.LAST_NAME, u.EMAIL, 
                        u.PHONE_NUMBER, u.AGE, u.BIRTHDAY, u.CIVIL_STATUS, 
                        u.CREATEDATE, u.ACCOUNT_STATUS,
                        ua.STREET, ua.CITY, ua.STATE, ua.ZIPCODE, ua.COUNTRY
                        FROM USERS u
                        LEFT JOIN USER_ADDRESSES ua ON u.USER_ID = ua.USER_ID
                        WHERE u.USER_ID = @UserId";

                    using (var cmd = new MySqlCommand(userQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                response.isSuccess = true;
                                response.Message = "User profile retrieved successfully";
                                response.UserId = reader.GetInt32("USER_ID");
                                response.FirstName = reader.GetString("FIRST_NAME");
                                response.LastName = reader.GetString("LAST_NAME");
                                response.Email = reader.GetString("EMAIL");
                                response.PhoneNumber = reader.IsDBNull(reader.GetOrdinal("PHONE_NUMBER")) ? null : reader.GetString("PHONE_NUMBER");
                                response.Age = reader.GetInt32("AGE");
                                response.Birthday = reader.GetDateTime("BIRTHDAY");
                                response.CivilStatus = reader.GetString("CIVIL_STATUS");
                                response.CreateDate = reader.GetDateTime("CREATEDATE");

                                // Map address if exists
                                if (!reader.IsDBNull(reader.GetOrdinal("STREET")))
                                {
                                    response.Address = new UserAddress
                                    {
                                        Street = reader.GetString("STREET"),
                                        City = reader.GetString("CITY"),
                                        State = reader.GetString("STATE"),
                                        ZipCode = reader.GetString("ZIPCODE"),
                                        Country = reader.GetString("COUNTRY")
                                    };
                                }
                            }
                            else
                            {
                                response.isSuccess = false;
                                response.Message = "User not found";
                                return response;
                            }
                        }
                    }

                    // Now get roles
                    string rolesQuery = @"
                        SELECT DISTINCT r.ROLE_NAME 
                        FROM USER_ROLES ur
                        JOIN ROLES r ON ur.ROLE_ID = r.ROLE_ID
                        WHERE ur.USER_ID = @UserId";

                    using (var cmd = new MySqlCommand(rolesQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                response.Roles.Add(reader.GetString("ROLE_NAME"));
                            }
                        }
                    }

                    // Finally get permissions
                    string permissionsQuery = @"
                        SELECT DISTINCT p.PERMISSION_NAME
                        FROM USER_ROLES ur
                        JOIN ROLE_PERMISSIONS rp ON ur.ROLE_ID = rp.ROLE_ID
                        JOIN PERMISSIONS p ON rp.PERMISSION_ID = p.PERMISSION_ID
                        WHERE ur.USER_ID = @UserId";

                    using (var cmd = new MySqlCommand(permissionsQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                response.Permissions.Add(reader.GetString("PERMISSION_NAME"));
                            }
                        }
                    }

                    return response;
                }
                catch (Exception ex)
                {
                    response.isSuccess = false;
                    response.Message = $"Error retrieving user profile: {ex.Message}";
                    return response;
                }
            }
        }

        public UpdateUserResponse UpdateUser(int userId, UpdateUserRequest r)
        {
            var response = new UpdateUserResponse();
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Check if username is already taken by another user
                            using (var checkCmd = new MySqlCommand(
                                "SELECT COUNT(*) FROM USERS WHERE USER_NAME = @UserName AND USER_ID != @UserId",
                                conn, transaction))
                            {
                                checkCmd.Parameters.AddWithValue("@UserName", r.UserName);
                                checkCmd.Parameters.AddWithValue("@UserId", userId);
                                int usernameExists = Convert.ToInt32(checkCmd.ExecuteScalar());
                                if (usernameExists > 0)
                                {
                                    throw new Exception("Username is already taken");
                                }
                            }

                            string updateUserQuery = @"
                                UPDATE USERS
                                SET USER_NAME = @UserName,
                                    FIRST_NAME = @FirstName,
                                    LAST_NAME = @LastName,
                                    EMAIL = @Email,
                                    PHONE_NUMBER = @PhoneNumber,
                                    BIRTHDAY = @Birthday,
                                    CIVIL_STATUS = @CivilStatus,
                                    UPDATEDATE = @UpdateDate
                                WHERE USER_ID = @UserId";

                            using (var cmd = new MySqlCommand(updateUserQuery, conn, transaction))
                            {
                                DateTime now = DateTime.Now;
                                cmd.Parameters.AddWithValue("@UserId", userId);
                                cmd.Parameters.AddWithValue("@UserName", r.UserName);
                                cmd.Parameters.AddWithValue("@FirstName", r.FirstName);
                                cmd.Parameters.AddWithValue("@LastName", r.LastName);
                                cmd.Parameters.AddWithValue("@Email", r.Email);
                                cmd.Parameters.AddWithValue("@PhoneNumber", r.PhoneNumber);
                                cmd.Parameters.AddWithValue("@Birthday", r.Birthday);
                                cmd.Parameters.AddWithValue("@CivilStatus", r.CivilStatus);
                                cmd.Parameters.AddWithValue("@UpdateDate", now);

                                int rowsAffected = cmd.ExecuteNonQuery();
                                if (rowsAffected == 0)
                                    throw new Exception("User not found");

                                // Update address if provided
                                if (r.Address != null)
                                {
                                    string addressQuery = @"
                                        INSERT INTO USER_ADDRESSES 
                                            (USER_ID, STREET, CITY, STATE, ZIPCODE, COUNTRY, CREATEDATE)
                                        VALUES 
                                            (@UserId, @Street, @City, @State, @ZipCode, @Country, @CreateDate)
                                        ON DUPLICATE KEY UPDATE
                                            STREET = @Street,
                                            CITY = @City,
                                            STATE = @State,
                                            ZIPCODE = @ZipCode,
                                            COUNTRY = @Country,
                                            UPDATEDATE = @CreateDate";

                                    using (var addrCmd = new MySqlCommand(addressQuery, conn, transaction))
                                    {
                                        addrCmd.Parameters.AddWithValue("@UserId", userId);
                                        addrCmd.Parameters.AddWithValue("@Street", r.Address.Street);
                                        addrCmd.Parameters.AddWithValue("@City", r.Address.City);
                                        addrCmd.Parameters.AddWithValue("@State", r.Address.State);
                                        addrCmd.Parameters.AddWithValue("@ZipCode", r.Address.ZipCode);
                                        addrCmd.Parameters.AddWithValue("@Country", r.Address.Country);
                                        addrCmd.Parameters.AddWithValue("@CreateDate", now);

                                        addrCmd.ExecuteNonQuery();
                                    }
                                }

                                transaction.Commit();
                                response.isSuccess = true;
                                response.Message = "User updated successfully";
                                response.UserId = userId;
                                response.UpdateDate = now;
                            }
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.isSuccess = false;
                response.Message = $"Error updating user: {ex.Message}";
            }
            return response;
        }


        public ForgetPasswordResponse RequestPasswordReset(ForgetPasswordRequest request)
        {
            var response = new ForgetPasswordResponse();
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Check if email exists and get user details
                            var checkCmd = new MySqlCommand(
                                "SELECT USER_ID, PHONE_NUMBER, ACCOUNT_STATUS FROM USERS WHERE EMAIL = @Email",
                                conn, transaction);
                            checkCmd.Parameters.AddWithValue("@Email", request.Email);

                            int userId;
                            string phoneNumber;
                            string accountStatus;
                            using (var reader = checkCmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    response.isSuccess = false;
                                    response.Message = "Email not found";
                                    return response;
                                }
                                userId = reader.GetInt32("USER_ID");
                                phoneNumber = reader.GetString("PHONE_NUMBER");
                                accountStatus = reader.GetString("ACCOUNT_STATUS");
                            }

                            if (accountStatus != "A")
                            {
                                response.isSuccess = false;
                                response.Message = "Account is inactive";
                                return response;
                            }

                            if (string.IsNullOrEmpty(phoneNumber))
                            {
                                response.isSuccess = false;
                                response.Message = "No phone number associated with this account";
                                return response;
                            }

                            // Expire OTPs that are older than 1 minute
                            var expireOldOtpsCmd = new MySqlCommand(@"
                                UPDATE USERS_OTP 
                                SET STATUS = 'E' 
                                WHERE USER_ID = @UserId 
                                  AND STATUS = 'A'
                                  AND TIMESTAMPDIFF(MINUTE, CREATED_AT, NOW()) >= 1",
                                conn, transaction);
                            expireOldOtpsCmd.Parameters.AddWithValue("@UserId", userId);
                            expireOldOtpsCmd.ExecuteNonQuery();

                            // Generate OTP and set expiry date (e.g., 5 minutes from now)
                            string otp = GenerateOtp();
                            DateTime now = DateTime.Now;
                            DateTime expiryDate = now.AddMinutes(5);

                            // Expire any remaining active OTPs
                            var expireCmd = new MySqlCommand(
                                "UPDATE USERS_OTP SET STATUS = 'E' WHERE USER_ID = @UserId AND STATUS = 'A'",
                                conn, transaction);
                            expireCmd.Parameters.AddWithValue("@UserId", userId);
                            expireCmd.ExecuteNonQuery();

                            // Insert new OTP record
                            var insertCmd = new MySqlCommand(
                                "INSERT INTO USERS_OTP (USER_ID, OTP, STATUS, CREATED_AT, EXPIRY_DATE) " +
                                "VALUES (@UserId, @Otp, 'A', @CreatedAt, @ExpiryDate)",
                                conn, transaction);
                            insertCmd.Parameters.AddWithValue("@UserId", userId);
                            insertCmd.Parameters.AddWithValue("@Otp", otp);
                            insertCmd.Parameters.AddWithValue("@CreatedAt", now);
                            insertCmd.Parameters.AddWithValue("@ExpiryDate", expiryDate);
                            insertCmd.ExecuteNonQuery();

                            // Send OTP via Twilio
                            bool smsSent = TwilioService.SendSms(phoneNumber, $"Your password reset OTP is: {otp}. Valid for 1 minute.");
                            if (!smsSent)
                            {
                                throw new Exception("Failed to send SMS");
                            }

                            transaction.Commit();
                            response.isSuccess = true;
                            response.Message = "OTP sent successfully";
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            response.isSuccess = false;
                            response.Message = $"Password reset request failed: {ex.Message}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.isSuccess = false;
                response.Message = $"Password reset request failed: {ex.Message}";
            }
            return response;
        }

        private string GenerateOtp()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 8)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public ConfirmOtpResponse ConfirmOtp(ConfirmOtpRequest request)
        {
            var response = new ConfirmOtpResponse();
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            var checkCmd = new MySqlCommand(@"
                                SELECT u.USER_ID, u.EMAIL, o.OTP_ID 
                                FROM USERS u
                                JOIN USERS_OTP o ON u.USER_ID = o.USER_ID
                                WHERE u.EMAIL = @Email 
                                  AND o.OTP = @Otp
                                  AND o.STATUS = 'A'
                                  AND TIMESTAMPDIFF(MINUTE, o.CREATED_AT, NOW()) < 1",
                                conn, transaction);

                            checkCmd.Parameters.AddWithValue("@Email", request.Email);
                            checkCmd.Parameters.AddWithValue("@Otp", request.Otp);

                            using (var reader = checkCmd.ExecuteReader())
                            {
                                if (!reader.Read())
                                {
                                    response.isSuccess = false;
                                    response.Message = "Invalid or expired OTP";
                                    return response;
                                }

                                int userId = reader.GetInt32("USER_ID");
                                int otpId = reader.GetInt32("OTP_ID");
                                string email = reader.GetString("EMAIL");
                                reader.Close();

                                // Mark OTP as used
                                var updateCmd = new MySqlCommand(
                                    "UPDATE USERS_OTP SET STATUS = 'U' WHERE OTP_ID = @OtpId",
                                    conn, transaction);
                                updateCmd.Parameters.AddWithValue("@OtpId", otpId);
                                updateCmd.ExecuteNonQuery();

                                // Retrieve the JWT secret key and check for null/empty
                                var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
                                if (string.IsNullOrEmpty(jwtSecret))
                                    throw new Exception("JWT_SECRET_KEY environment variable is not set.");

                                var tokenHandler = new JwtSecurityTokenHandler();
                                var key = Encoding.ASCII.GetBytes(jwtSecret);
                                var tokenDescriptor = new SecurityTokenDescriptor
                                {
                                    Subject = new ClaimsIdentity(new[]
                                    {
                                        new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                                        new Claim(ClaimTypes.Email, email),
                                        new Claim("purpose", "password_reset")
                                    }),
                                    Expires = DateTime.UtcNow.AddMinutes(15),
                                    SigningCredentials = new SigningCredentials(
                                        new SymmetricSecurityKey(key),
                                        SecurityAlgorithms.HmacSha256Signature)
                                };

                                var token = tokenHandler.CreateToken(tokenDescriptor);
                                response.Token = tokenHandler.WriteToken(token);

                                transaction.Commit();
                                response.isSuccess = true;
                                response.Message = "OTP confirmed successfully";
                            }
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.isSuccess = false;
                response.Message = $"OTP confirmation failed: {ex.Message}";
            }
            return response;
        }

        public ResetPasswordResponse ResetPassword(string token, ResetPasswordRequest request)
        {
            var response = new ResetPasswordResponse();
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("JWT_SECRET_KEY"));

                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out validatedToken);

                var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
                var purpose = principal.FindFirst("purpose")?.Value;

                if (purpose != "password_reset")
                {
                    throw new Exception("Invalid token purpose");
                }

                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            string hashedPassword = PasswordHasher.HashPassword(request.NewPassword);

                            // Update password in USERS table
                            var updateCmd = new MySqlCommand(
                                "UPDATE USERS SET PASSWORD = @Password WHERE USER_ID = @UserId",
                                conn, transaction);
                            updateCmd.Parameters.AddWithValue("@Password", hashedPassword);
                            updateCmd.Parameters.AddWithValue("@UserId", userId);

                            int rowsAffected = updateCmd.ExecuteNonQuery();
                            if (rowsAffected == 0)
                            {
                                throw new Exception("User not found");
                            }
                                                     
                            var clearFailedLoginsCmd = new MySqlCommand(
                                "DELETE FROM FAILED_LOGINS WHERE USER_ID = @UserId",
                                conn, transaction);
                            clearFailedLoginsCmd.Parameters.AddWithValue("@UserId", userId);
                            clearFailedLoginsCmd.ExecuteNonQuery();

                            transaction.Commit();
                            response.isSuccess = true;
                            response.Message = "Password reset successfully and account unlocked";
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.isSuccess = false;
                response.Message = $"Password reset failed: {ex.Message}";
            }
            return response;
        }

        public GetRolesResponse GetRoles()
        {
            var response = new GetRolesResponse();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    string sql = @"
                        SELECT 
                            r.ROLE_ID, r.ROLE_NAME, r.DESCRIPTION,
                            p.PERMISSION_ID, p.PERMISSION_NAME, p.DESCRIPTION as PERMISSION_DESCRIPTION
                        FROM ROLES r
                        LEFT JOIN ROLE_PERMISSIONS rp ON r.ROLE_ID = rp.ROLE_ID
                        LEFT JOIN PERMISSIONS p ON rp.PERMISSION_ID = p.PERMISSION_ID
                        ORDER BY r.ROLE_ID";

                    var roleDict = new Dictionary<int, Role>();

                    using (var cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int roleId = reader.GetInt32("ROLE_ID");
                            
                            if (!roleDict.ContainsKey(roleId))
                            {
                                roleDict[roleId] = new Role
                                {
                                    RoleId = roleId,
                                    RoleName = reader.GetString("ROLE_NAME"),
                                    Description = reader.IsDBNull(reader.GetOrdinal("DESCRIPTION")) 
                                        ? null 
                                        : reader.GetString("DESCRIPTION")
                                };
                            }

                            if (!reader.IsDBNull(reader.GetOrdinal("PERMISSION_ID")))
                            {
                                roleDict[roleId].Permissions.Add(new RolePermission
                                {
                                    PermissionId = reader.GetInt32("PERMISSION_ID"),
                                    PermissionName = reader.GetString("PERMISSION_NAME"),
                                    Description = reader.IsDBNull(reader.GetOrdinal("PERMISSION_DESCRIPTION"))
                                        ? null
                                        : reader.GetString("PERMISSION_DESCRIPTION")
                                });
                            }
                        }
                    }

                    response.Roles = roleDict.Values.ToList();
                    response.isSuccess = true;
                    response.Message = "Roles retrieved successfully";
                }
            }
            catch (Exception ex)
            {
                response.isSuccess = false;
                response.Message = $"Error retrieving roles: {ex.Message}";
            }
            return response;
        }

        public CreateRoleResponse CreateRole(CreateRoleRequest request)
        {
            var response = new CreateRoleResponse();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Check if role name already exists
                            using (var checkCmd = new MySqlCommand(
                                "SELECT COUNT(*) FROM ROLES WHERE ROLE_NAME = @RoleName",
                                conn, transaction))
                            {
                                checkCmd.Parameters.AddWithValue("@RoleName", request.RoleName);
                                int exists = Convert.ToInt32(checkCmd.ExecuteScalar());
                                if (exists > 0)
                                {
                                    throw new Exception("Role name already exists");
                                }
                            }

                            // Insert new role
                            int roleId;
                            using (var cmd = new MySqlCommand(
                                "INSERT INTO ROLES (ROLE_NAME, DESCRIPTION) VALUES (@RoleName, @Description)",
                                conn, transaction))
                            {
                                cmd.Parameters.AddWithValue("@RoleName", request.RoleName);
                                cmd.Parameters.AddWithValue("@Description", 
                                    string.IsNullOrEmpty(request.Description) ? DBNull.Value : request.Description);
                                
                                cmd.ExecuteNonQuery();
                                roleId = (int)cmd.LastInsertedId;
                            }

                            // Add role permissions if any
                            if (request.PermissionIds?.Any() == true)
                            {
                                using (var cmd = new MySqlCommand(
                                    "INSERT INTO ROLE_PERMISSIONS (ROLE_ID, PERMISSION_ID) VALUES (@RoleId, @PermissionId)",
                                    conn, transaction))
                                {
                                    foreach (int permissionId in request.PermissionIds)
                                    {
                                        cmd.Parameters.Clear();
                                        cmd.Parameters.AddWithValue("@RoleId", roleId);
                                        cmd.Parameters.AddWithValue("@PermissionId", permissionId);
                                        cmd.ExecuteNonQuery();
                                    }
                                }
                            }

                            transaction.Commit();
                            response.isSuccess = true;
                            response.Message = "Role created successfully";
                            response.RoleId = roleId;
                            response.RoleName = request.RoleName;
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.isSuccess = false;
                response.Message = $"Error creating role: {ex.Message}";
            }
            return response;
        }

        public UpdateRolePermissionsResponse UpdateRolePermissions(UpdateRolePermissionsRequest request)
        {
            var response = new UpdateRolePermissionsResponse();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Verify role exists
                            using (var checkCmd = new MySqlCommand(
                                "SELECT COUNT(*) FROM ROLES WHERE ROLE_ID = @RoleId",
                                conn, transaction))
                            {
                                checkCmd.Parameters.AddWithValue("@RoleId", request.RoleId);
                                int exists = Convert.ToInt32(checkCmd.ExecuteScalar());
                                if (exists == 0)
                                {
                                    throw new Exception("Role not found");
                                }
                            }

                            // Remove existing permissions
                            using (var deleteCmd = new MySqlCommand(
                                "DELETE FROM ROLE_PERMISSIONS WHERE ROLE_ID = @RoleId",
                                conn, transaction))
                            {
                                deleteCmd.Parameters.AddWithValue("@RoleId", request.RoleId);
                                deleteCmd.ExecuteNonQuery();
                            }

                            // Add new permissions
                            using (var insertCmd = new MySqlCommand(
                                "INSERT INTO ROLE_PERMISSIONS (ROLE_ID, PERMISSION_ID) VALUES (@RoleId, @PermissionId)",
                                conn, transaction))
                            {
                                foreach (int permissionId in request.PermissionIds)
                                {
                                    insertCmd.Parameters.Clear();
                                    insertCmd.Parameters.AddWithValue("@RoleId", request.RoleId);
                                    insertCmd.Parameters.AddWithValue("@PermissionId", permissionId);
                                    insertCmd.ExecuteNonQuery();
                                }
                            }

                            // Get updated permissions for response
                            using (var getCmd = new MySqlCommand(@"
                                SELECT p.PERMISSION_ID, p.PERMISSION_NAME, p.DESCRIPTION
                                FROM PERMISSIONS p
                                JOIN ROLE_PERMISSIONS rp ON p.PERMISSION_ID = rp.PERMISSION_ID
                                WHERE rp.ROLE_ID = @RoleId",
                                conn, transaction))
                            {
                                getCmd.Parameters.AddWithValue("@RoleId", request.RoleId);
                                using (var reader = getCmd.ExecuteReader())
                                {
                                    while (reader.Read())
                                    {
                                        response.UpdatedPermissions.Add(new RolePermission
                                        {
                                            PermissionId = reader.GetInt32("PERMISSION_ID"),
                                            PermissionName = reader.GetString("PERMISSION_NAME"),
                                            Description = reader.IsDBNull(reader.GetOrdinal("DESCRIPTION"))
                                                ? null
                                                : reader.GetString("DESCRIPTION")
                                        });
                                    }
                                }
                            }

                            transaction.Commit();
                            response.isSuccess = true;
                            response.Message = "Role permissions updated successfully";
                            response.RoleId = request.RoleId;
                        }
                        catch (Exception)
                        {
                            transaction.Rollback();
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                response.isSuccess = false;
                response.Message = $"Error updating role permissions: {ex.Message}";
            }
            return response;
        }

    }
}

//TEST COMMENT
