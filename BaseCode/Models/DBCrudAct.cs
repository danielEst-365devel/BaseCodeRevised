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
                        "SELECT CUSTOMERID, EMAIL, PASSWORD, FIRSTNAME, LASTNAME " +
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

                            // Close reader to allow new commands on same connection.
                            reader.Close();

                            // Check failed login attempts.
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
                                // Clear failed login attempts.
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
                                // Log failed login attempt.
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
    }
}

//TEST COMMENT
