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
                    MySqlTransaction myTrans;
                    myTrans = conn.BeginTransaction();
                    MySqlCommand cmd = new MySqlCommand(r.query, conn);
                    cmd.ExecuteNonQuery();

                    resp.Id = r.isInsert ? int.Parse(cmd.LastInsertedId.ToString()) : -1;
                    myTrans.Commit();
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
                    
                    // Check for existing email
                    MySqlCommand checkEmail = new MySqlCommand(
                        "SELECT COUNT(*) FROM CUSTOMERS WHERE EMAIL = @Email", conn);
                    checkEmail.Parameters.AddWithValue("@Email", r.Email);
                    int emailCount = Convert.ToInt32(checkEmail.ExecuteScalar());
                    
                    if (emailCount > 0)
                    {
                        resp.isSuccess = false;
                        resp.Message = "Email already exists";
                        return resp;
                    }

                    MySqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        string hashedPassword = PasswordHasher.HashPassword(r.Password);
                        
                        MySqlCommand customerCmd = new MySqlCommand(
                            "INSERT INTO CUSTOMERS (FIRSTNAME, LASTNAME, EMAIL, PASSWORD, PHONENUMBER, ADDRESS, CREATEDATE) " +
                            "VALUES (@FirstName, @LastName, @Email, @Password, @PhoneNumber, @Address, @CreateDate);", conn);

                        customerCmd.Parameters.AddWithValue("@FirstName", r.FirstName);
                        customerCmd.Parameters.AddWithValue("@LastName", r.LastName);
                        customerCmd.Parameters.AddWithValue("@Email", r.Email);
                        customerCmd.Parameters.AddWithValue("@Password", hashedPassword);
                        customerCmd.Parameters.AddWithValue("@PhoneNumber", r.PhoneNumber ?? (object)DBNull.Value);
                        customerCmd.Parameters.AddWithValue("@Address", r.Address ?? (object)DBNull.Value);
                        customerCmd.Parameters.AddWithValue("@CreateDate", DateTime.Now);

                        customerCmd.ExecuteNonQuery();
                        int customerId = (int)customerCmd.LastInsertedId;

                        transaction.Commit();
                        resp.CustomerId = customerId;
                        resp.CreateDate = DateTime.Now;
                        resp.isSuccess = true;
                        resp.Message = "Customer created successfully.";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        throw ex;
                    }
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
                            string storedHash = reader["PASSWORD"].ToString();
                            bool verified = PasswordHasher.VerifyPassword(r.Password, storedHash);

                            if (verified)
                            {
                                resp.isSuccess = true;
                                resp.CustomerId = Convert.ToInt32(reader["CUSTOMERID"]);
                                resp.Email = reader["EMAIL"].ToString();
                                resp.FirstName = reader["FIRSTNAME"].ToString();
                                resp.LastName = reader["LASTNAME"].ToString();
                                resp.Message = "Login successful";
                            }
                            else
                            {
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
    }
}

//TEST COMMENT
