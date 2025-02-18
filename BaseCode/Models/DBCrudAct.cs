using System;
using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using BaseCode.Models.Responses;
using BaseCode.Models.Responses.forCrudAct;
using BaseCode.Models.Requests;
using BaseCode.Models.Requests.forCrudAct;
using BaseCode.Models.Tables;
using System.Reflection;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using BaseCode.Utils;

namespace BaseCode.Models
{
    public class DBCrudAct
    {
        public string ConnectionString { get; set; }
        public DBCrudAct(string connStr)
        {
            this.ConnectionString = connStr;
        }
        private MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        public GenericInsertUpdateResponse InsertUpdateData(GenericInsertUpdateRequest r)
        {
            GenericInsertUpdateResponse resp = new GenericInsertUpdateResponse();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    using (MySqlTransaction myTrans = conn.BeginTransaction())
                    {
                        MySqlCommand cmd = new MySqlCommand(r.query, conn, myTrans);
                        cmd.ExecuteNonQuery();

                        resp.Id = r.isInsert ? int.Parse(cmd.LastInsertedId.ToString()) : -1;
                        myTrans.Commit();
                    }
                    conn.Close();
                    resp.isSuccess = true;
                    resp.Message = r.responseMessage;
                }
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = r.errorMessage + ": " + ex.Message;
            }
            return resp;
        }

        public CreateCustomerResponse CreateCustomer(CreateCustomerRequest r)
        {
            CreateCustomerResponse resp = new CreateCustomerResponse();
            try
            {
                if (r.Age < 12)
                {
                    resp.isSuccess = false;
                    resp.Message = "Age must be at least 12.";
                    return resp;
                }

                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();

                    MySqlCommand checkEmail = new MySqlCommand(
                        "SELECT COUNT(*) FROM CUSTOMERS WHERE EMAIL = @Email", conn);
                    checkEmail.Parameters.AddWithValue("@Email", r.Email);
                    int emailCount = Convert.ToInt32(checkEmail.ExecuteScalar());

                    if (emailCount > 0)
                    {
                        resp.isSuccess = false;
                        resp.Message = "Email already exists";
                        conn.Close();
                        return resp;
                    }

                    using (MySqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            string hashedPassword = PasswordHasher.HashPassword(r.Password);
                            DateTime now = DateTime.Now;

                            MySqlCommand customerCmd = new MySqlCommand(
                                "INSERT INTO CUSTOMERS (FIRSTNAME, LASTNAME, EMAIL, PASSWORD, PHONENUMBER, " +
                                "AGE, BIRTHDAY, CIVIL_STATUS, ADDRESS, CREATEDATE) " +
                                "VALUES (@FirstName, @LastName, @Email, @Password, @PhoneNumber, " +
                                "@Age, @Birthday, @CivilStatus, @Address, @CreateDate);",
                                conn, transaction);

                            customerCmd.Parameters.AddWithValue("@FirstName", r.FirstName);
                            customerCmd.Parameters.AddWithValue("@LastName", r.LastName);
                            customerCmd.Parameters.AddWithValue("@Email", r.Email);
                            customerCmd.Parameters.AddWithValue("@Password", hashedPassword);
                            customerCmd.Parameters.AddWithValue("@PhoneNumber", r.PhoneNumber ?? (object)DBNull.Value);
                            customerCmd.Parameters.AddWithValue("@Age", r.Age);
                            customerCmd.Parameters.AddWithValue("@Birthday", r.Birthday ?? (object)DBNull.Value);
                            customerCmd.Parameters.AddWithValue("@CivilStatus", r.CivilStatus ?? (object)DBNull.Value);
                            customerCmd.Parameters.AddWithValue("@Address", DBNull.Value);
                            customerCmd.Parameters.AddWithValue("@CreateDate", now);

                            customerCmd.ExecuteNonQuery();
                            int customerId = (int)customerCmd.LastInsertedId;

                            if (r.Address != null)
                            {
                                MySqlCommand addressCmd = new MySqlCommand(
                                    "INSERT INTO CUSTOMER_ADDRESSES (CUSTOMERID, STREET, CITY, STATE, ZIPCODE, COUNTRY, CREATEDATE) " +
                                    "VALUES (@CustomerId, @Street, @City, @State, @ZipCode, @Country, @CreateDate);",
                                    conn, transaction);

                                addressCmd.Parameters.AddWithValue("@CustomerId", customerId);
                                addressCmd.Parameters.AddWithValue("@Street", r.Address.Street);
                                addressCmd.Parameters.AddWithValue("@City", r.Address.City);
                                addressCmd.Parameters.AddWithValue("@State", r.Address.State);
                                addressCmd.Parameters.AddWithValue("@ZipCode", r.Address.ZipCode);
                                addressCmd.Parameters.AddWithValue("@Country", r.Address.Country);
                                addressCmd.Parameters.AddWithValue("@CreateDate", now);

                                addressCmd.ExecuteNonQuery();
                            }

                            transaction.Commit();
                            resp.CustomerId = customerId;
                            resp.CreateDate = now;
                            resp.isSuccess = true;
                            resp.Message = "Customer created successfully.";
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
                resp.Message = "Customer creation failed: " + ex.Message;
            }
            return resp;
        }

        public CustomerLoginResponse LoginCustomer(CustomerLoginRequest r)
        {
            CustomerLoginResponse resp = new CustomerLoginResponse();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(
                        "SELECT CUSTOMERID, EMAIL, PASSWORD, FIRSTNAME, LASTNAME, ACCOUNT_STATUS " +
                        "FROM CUSTOMERS WHERE EMAIL = @Email", conn);
                    cmd.Parameters.AddWithValue("@Email", r.Email);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int customerId = Convert.ToInt32(reader["CUSTOMERID"]);
                            string email = reader["EMAIL"].ToString();
                            string storedHash = reader["PASSWORD"].ToString();
                            string firstName = reader["FIRSTNAME"].ToString();
                            string lastName = reader["LASTNAME"].ToString();
                            string accountStatus = reader["ACCOUNT_STATUS"].ToString();

                            reader.Close();

                            if (accountStatus != "A")
                            {
                                resp.isSuccess = false;
                                resp.Message = "Account inactive";
                                conn.Close();
                                return resp;
                            }
                                                       
                            MySqlCommand countCmd = new MySqlCommand(
                            "SELECT COUNT(*) FROM FAILED_LOGINS WHERE CUSTOMERID = @CustomerId", conn);
                            countCmd.Parameters.AddWithValue("@CustomerId", customerId);
                            int failedAttempts = Convert.ToInt32(countCmd.ExecuteScalar());

                            if (failedAttempts >= 5)
                            {
                                resp.isSuccess = false;
                                resp.Message = "Account locked due to too many failed login attempts";
                                conn.Close();
                                return resp;
                            }

                            bool verified = PasswordHasher.VerifyPassword(r.Password, storedHash);

                            if (verified)
                            {
                               
                                MySqlCommand deleteCmd = new MySqlCommand(
                                "DELETE FROM FAILED_LOGINS WHERE CUSTOMERID = @CustomerId", conn);
                                deleteCmd.Parameters.AddWithValue("@CustomerId", customerId);
                                deleteCmd.ExecuteNonQuery();

                                resp.isSuccess = true;
                                resp.CustomerId = customerId;
                                resp.Email = email;
                                resp.FirstName = firstName;
                                resp.LastName = lastName;
                                resp.Message = "Login successful";
                            }
                            else
                            {
                                MySqlCommand insertCmd = new MySqlCommand(
                                "INSERT INTO FAILED_LOGINS (CUSTOMERID) VALUES (@CustomerId)", conn);
                                insertCmd.Parameters.AddWithValue("@CustomerId", customerId);
                                insertCmd.ExecuteNonQuery();

                                resp.isSuccess = false;
                                resp.Message = "Invalid password";
                            }
                        }
                        else
                        {
                            resp.isSuccess = false;
                            resp.Message = "Email not found";
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = "Login failed: " + ex.Message;
            }
            return resp;
        }

        
        public CustomerLoginResponse LoginCustomerWithCookie(CustomerLoginRequest r)
        {
            CustomerLoginResponse resp = new CustomerLoginResponse();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(
                        "SELECT CUSTOMERID, EMAIL, PASSWORD, FIRSTNAME, LASTNAME, ACCOUNT_STATUS " +
                        "FROM CUSTOMERS WHERE EMAIL = @Email", conn);
                    cmd.Parameters.AddWithValue("@Email", r.Email);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            int customerId = Convert.ToInt32(reader["CUSTOMERID"]);
                            string email = reader["EMAIL"].ToString();
                            string storedHash = reader["PASSWORD"].ToString();
                            string firstName = reader["FIRSTNAME"].ToString();
                            string lastName = reader["LASTNAME"].ToString();
                            string accountStatus = reader["ACCOUNT_STATUS"].ToString();

                            reader.Close();

                            if (accountStatus != "A")
                            {
                                resp.isSuccess = false;
                                resp.Message = "Account inactive";
                                return resp;
                            }

                            // Check failed login attempts
                            MySqlCommand countCmd = new MySqlCommand(
                                "SELECT COUNT(*) FROM FAILED_LOGINS WHERE CUSTOMERID = @CustomerId", conn);
                            countCmd.Parameters.AddWithValue("@CustomerId", customerId);
                            int failedAttempts = Convert.ToInt32(countCmd.ExecuteScalar());

                            if (failedAttempts >= 5)
                            {
                                resp.isSuccess = false;
                                resp.Message = "Account locked due to too many failed login attempts";
                                return resp;
                            }

                            bool verified = PasswordHasher.VerifyPassword(r.Password, storedHash);

                            if (verified)
                            {
                                // Clear failed login attempts
                                MySqlCommand deleteCmd = new MySqlCommand(
                                    "DELETE FROM FAILED_LOGINS WHERE CUSTOMERID = @CustomerId", conn);
                                deleteCmd.Parameters.AddWithValue("@CustomerId", customerId);
                                deleteCmd.ExecuteNonQuery();

                                resp.isSuccess = true;
                                resp.CustomerId = customerId;
                                resp.Email = email;
                                resp.FirstName = firstName;
                                resp.LastName = lastName;
                                resp.Message = "Login successful";
                            }
                            else
                            {
                                // Log failed login attempt
                                MySqlCommand insertCmd = new MySqlCommand(
                                    "INSERT INTO FAILED_LOGINS (CUSTOMERID) VALUES (@CustomerId)", conn);
                                insertCmd.Parameters.AddWithValue("@CustomerId", customerId);
                                insertCmd.ExecuteNonQuery();

                                resp.isSuccess = false;
                                resp.Message = "Invalid password";
                            }
                        }
                        else
                        {
                            resp.isSuccess = false;
                            resp.Message = "Email not found";
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

       
        public CustomerProfileResponse GetCustomerProfile(int customerId)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                var response = new CustomerProfileResponse();

                try
                {
                                       string customerQuery = @"SELECT 
                    c.CUSTOMERID, c.FIRSTNAME, c.LASTNAME, c.EMAIL, 
                    c.PHONENUMBER, c.AGE, c.BIRTHDAY, c.CIVIL_STATUS, 
                    c.CREATEDATE,
                    ca.STREET, ca.CITY, ca.STATE, ca.ZIPCODE, ca.COUNTRY
                    FROM CUSTOMERS c
                    LEFT JOIN CUSTOMER_ADDRESSES ca ON c.CUSTOMERID = ca.CUSTOMERID
                    WHERE c.CUSTOMERID = @CustomerId";

                    using (var cmd = new MySqlCommand(customerQuery, conn))
                    {
                        cmd.Parameters.AddWithValue("@CustomerId", customerId);

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                response.isSuccess = true;
                                response.Message = "Customer profile retrieved successfully";
                                response.CustomerId = reader.GetInt32("CUSTOMERID");
                                response.FirstName = reader.GetString("FIRSTNAME");
                                response.LastName = reader.GetString("LASTNAME");
                                response.Email = reader.GetString("EMAIL");
                                response.PhoneNumber = reader.GetString("PHONENUMBER");
                                response.Age = reader.GetInt32("AGE");
                                response.Birthday = reader.IsDBNull(reader.GetOrdinal("BIRTHDAY")) ? null : reader.GetDateTime("BIRTHDAY");
                                response.CivilStatus = reader.GetString("CIVIL_STATUS");
                                response.CreateDate = reader.GetDateTime("CREATEDATE");

                                // Map address if exists
                                if (!reader.IsDBNull(reader.GetOrdinal("STREET")))
                                {
                                    response.Address = new CustomerAddress
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
                                response.Message = "Customer not found";
                            }
                        }
                    }

                    return response;
                }
                catch (Exception ex)
                {
                    response.isSuccess = false;
                    response.Message = $"Error retrieving customer profile: {ex.Message}";
                    return response;
                }
            }
        }

        // Add this new method to the DBCrudAct class
        public UpdateCustomerResponse UpdateCustomer(int customerId, UpdateCustomerRequest r)
        {
            var response = new UpdateCustomerResponse();
            try
            {
                if (r.Age < 12)
                {
                    response.isSuccess = false;
                    response.Message = "Age must be at least 12.";
                    return response;
                }

                using (var conn = GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Update customer details
                            string updateCustomerQuery = @"
                                UPDATE CUSTOMERS 
                                SET FIRSTNAME = @FirstName,
                                    LASTNAME = @LastName,
                                    EMAIL = @Email,
                                    PHONENUMBER = @PhoneNumber,
                                    AGE = @Age,
                                    BIRTHDAY = @Birthday,
                                    CIVIL_STATUS = @CivilStatus,
                                    UPDATEDATE = @UpdateDate
                                WHERE CUSTOMERID = @CustomerId";

                            using (var cmd = new MySqlCommand(updateCustomerQuery, conn, transaction))
                            {
                                DateTime now = DateTime.Now;
                                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                                cmd.Parameters.AddWithValue("@FirstName", r.FirstName);
                                cmd.Parameters.AddWithValue("@LastName", r.LastName);
                                cmd.Parameters.AddWithValue("@Email", r.Email);
                                cmd.Parameters.AddWithValue("@PhoneNumber", r.PhoneNumber ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@Age", r.Age);
                                cmd.Parameters.AddWithValue("@Birthday", r.Birthday ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@CivilStatus", r.CivilStatus ?? (object)DBNull.Value);
                                cmd.Parameters.AddWithValue("@UpdateDate", now);

                                int rowsAffected = cmd.ExecuteNonQuery();
                                if (rowsAffected == 0)
                                {
                                    throw new Exception("Customer not found");
                                }

                                // Update address if provided
                                if (r.Address != null)
                                {
                                    string addressQuery = @"
                                        INSERT INTO CUSTOMER_ADDRESSES 
                                            (CUSTOMERID, STREET, CITY, STATE, ZIPCODE, COUNTRY, CREATEDATE)
                                        VALUES 
                                            (@CustomerId, @Street, @City, @State, @ZipCode, @Country, @CreateDate)
                                        ON DUPLICATE KEY UPDATE
                                            STREET = @Street,
                                            CITY = @City,
                                            STATE = @State,
                                            ZIPCODE = @ZipCode,
                                            COUNTRY = @Country,
                                            UPDATEDATE = @CreateDate";

                                    using (var addrCmd = new MySqlCommand(addressQuery, conn, transaction))
                                    {
                                        addrCmd.Parameters.AddWithValue("@CustomerId", customerId);
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
                                response.Message = "Customer updated successfully";
                                response.CustomerId = customerId;
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
                response.Message = $"Error updating customer: {ex.Message}";
            }
            return response;
        }

    }
}

//TEST COMMENT
