using System;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using System.Net;
using System.Diagnostics.Eventing.Reader;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Runtime.CompilerServices;
using System.ComponentModel.Design;
using Microsoft.AspNetCore.Mvc.Formatters;
using System.Security.Policy;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using BaseCode.Models.Responses;
using BaseCode.Models.Requests;
using BaseCode.Models.Tables;

using System.Reflection;

namespace BaseCode.Models
{
    public class DBContext
    {
        public string ConnectionString { get; set; }
        public DBContext(string connStr)
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
        public CreateUserResponse CreateUserUsingSqlScript(CreateUserRequest r)
        {
            CreateUserResponse resp = new CreateUserResponse();
            DateTime theDate = DateTime.Now;
            string crtdt = theDate.ToString("yyyy-MM-dd H:mm:ss");
            try
            {
                using (MySqlConnection conn = GetConnection())

                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand("INSERT INTO USER (FIRST_NAME,LAST_NAME,USER_NAME,PASSWORD) " +
                    "VALUES ('" + r.FirstName + "','" + r.LastName + "','" + r.UserName + "','" + r.Password + "');", conn);
                    
                  //  cmd.Parameters.Add(new MySqlParameter("@FIRST_NAME", r.FirstName));
                    cmd.Parameters.Add(new MySqlParameter("@LAST_NAME", r.LastName));
                    cmd.Parameters.Add(new MySqlParameter("@USER_NAME", r.UserName));
                    cmd.Parameters.Add(new MySqlParameter("@PASSWORD", r.Password));

                    cmd.ExecuteNonQuery();

                    conn.Close();
                }
            }
             
            catch (Exception ex)
            {
                resp.Message = "Please try again.";
                resp.isSuccess = false;
                return resp;
            }
            resp.isSuccess = true;
            resp.Message = "Successfully added user profile.";
            return resp;
        }

        public CreateUserResponse UpdateUser(CreateUserRequest r)
        {
            CreateUserResponse resp = new CreateUserResponse();

            DateTime theDate = DateTime.Now;
            string crtdt = theDate.ToString("yyyy-MM-dd H:mm:ss");

            try
            {

             using (MySqlConnection conn = GetConnection())

             {
                conn.Open();


                MySqlCommand cmd = new MySqlCommand("UPDATE USER SET FIRST_NAME = @FIRST_NAME, LAST_NAME = @LAST_NAME, USER_NAME = @USER_NAME, PASSWORD = @PASSWORD " +
                "WHERE USER_ID = @USER_ID;", conn);
                cmd.Parameters.Add(new MySqlParameter("@FIRST_NAME", r.FirstName));
                cmd.Parameters.Add(new MySqlParameter("@LAST_NAME", r.LastName));
                cmd.Parameters.Add(new MySqlParameter("@USER_NAME", r.UserName));
                cmd.Parameters.Add(new MySqlParameter("@USER_ID", r.UserId));
                    cmd.Parameters.Add(new MySqlParameter("@PASSWORD", r.Password));

                    cmd.ExecuteNonQuery();

                conn.Close();
             }   
            }
            catch (Exception ex)
            {
                resp.Message = ex.ToString();
                resp.isSuccess = false;
                return resp;
            }
            resp.isSuccess = true;
            resp.Message = "Successfully updated user profile.";
            return resp;
        }
        public CreateUserResponse DeleteUser(string userId)
        {
            CreateUserResponse resp = new CreateUserResponse();

            DateTime theDate = DateTime.Now;
            string crtdt = theDate.ToString("yyyy-MM-dd H:mm:ss");
            try
            {
                using (MySqlConnection conn = GetConnection())

                {
                    conn.Open();


                    MySqlCommand cmd = new MySqlCommand("UPDATE USER SET STATUS = 'I' " +
                    "WHERE USER_ID = @USER_ID;", conn);
                    cmd.Parameters.Add(new MySqlParameter("@USER_ID", userId));

                    cmd.ExecuteNonQuery();

                    conn.Close();

                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.ToString();
                resp.isSuccess = false;
                return resp;
            }
            resp.isSuccess = true;
            resp.Message = "Successfully deleted user.";
            return resp;
        }
        public GetUserListResponse GetUserList(GetUserListRequest r )
        {
            GetUserListResponse resp = new GetUserListResponse();
            resp.Data = new List<Dictionary<string, string>>();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT * FROM USER WHERE STATUS = 'A'";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        reader.Close();
                        var columns = dt.Columns.Cast<DataColumn>();

                        resp.Data.AddRange(dt.AsEnumerable().Select(dataRow => columns.Select(column =>
                  new { Column = column.ColumnName, Value = dataRow[column] })
                  .ToDictionary(data => data.Column.ToString(), data => data.Value.ToString())).ToList());
                    }
                    resp.isSuccess = true;
                    resp.Message = "List of users:";
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.ToString();
                resp.isSuccess = false;
                return resp;
            }

            return resp;
        }

        public GenericGetDataResponse GetData(string query)
        {
            GenericGetDataResponse resp = new GenericGetDataResponse();
            DataTable dt;
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    dt = new DataTable();
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        dt.Load(reader);
                        reader.Close();
                    }
                    conn.Close();
                }
                resp.isSuccess = true;
                resp.Message = "Successfully get data";
                resp.Data = dt;

            }
            catch (Exception ex)
            {
                resp.isSuccess = false;
                resp.Message = ex.Message;
            }
            return resp;
        }

        public CreateUserInfoResponse CreateUserInfoUsingSqlScript(CreateUserInfoRequest r)
        {
            CreateUserInfoResponse resp = new CreateUserInfoResponse();
            DateTime theDate = DateTime.Now;
            string crtdt = theDate.ToString("yyyy-MM-dd H:mm:ss");
            try
            {
                using (MySqlConnection conn = GetConnection())

                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand("INSERT INTO USER_INFO (USER_ID,MOBILE,EMAIL,BIRTHDAY,COUNTRY) " +
                    "VALUES (@USER_ID,@MOBILE,@EMAIL,@BIRTHDAY,@COUNTRY);", conn);

                    cmd.Parameters.Add(new MySqlParameter("@USER_ID", r.UserId));
                    cmd.Parameters.Add(new MySqlParameter("@MOBILE", r.Mobile));
                    cmd.Parameters.Add(new MySqlParameter("@EMAIL", r.Email));
                    cmd.Parameters.Add(new MySqlParameter("@BIRTHDAY", r.Birthday));
                    cmd.Parameters.Add(new MySqlParameter("@COUNTRY", r.Country));

                    cmd.ExecuteNonQuery();

                    conn.Clone();
                }
            }

            catch (Exception ex)
            {
                resp.Message = ex.ToString();
                resp.isSuccess = false;
                return resp;
            }
            resp.isSuccess = true;
            resp.Message = "Successfully added user info profile.";
            return resp;
        }

        public GetUserProfileListResponse GetUserProfileList(GetUserProfileListRequest r)
        {
            GetUserProfileListResponse resp = new GetUserProfileListResponse();
            resp.Data = new List<Dictionary<string, string>>();
            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();
                    string sql = "SELECT U.USER_ID,CONCAT(U.`LAST_NAME`,' ',U.`FIRST_NAME`) AS FULL_NAME,UI.`BIRTHDAY`,UI.`MOBILE`,UI.`EMAIL`,UI.`COUNTRY` FROM USER U " +
                        "LEFT JOIN USER_INFO UI ON UI.USER_ID = U.`USER_ID` WHERE U.STATUS = 'A'";

                    MySqlCommand cmd = new MySqlCommand(sql, conn);
                    var reader = cmd.ExecuteReader();
                    if (reader.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        reader.Close();
                        var columns = dt.Columns.Cast<DataColumn>();

                        resp.Data.AddRange(dt.AsEnumerable().Select(dataRow => columns.Select(column =>
                  new { Column = column.ColumnName, Value = dataRow[column] })
                  .ToDictionary(data => data.Column.ToString(), data => data.Value.ToString())).ToList());
                    }
                    resp.isSuccess = true;
                    resp.Message = "List of users profile:";
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.ToString();
                resp.isSuccess = false;
                return resp;
            }

            return resp;
        }

        public CreateUserResponse GetUserById(int UserId)
        {
            CreateUserResponse resp = new CreateUserResponse();

            try
            {
                using (MySqlConnection conn = GetConnection())
                {
                    conn.Open();

                    MySqlCommand cmd = new MySqlCommand("SELECT * FROM USER WHERE USER_ID = @USER_ID;", conn);
                    cmd.Parameters.Add(new MySqlParameter("@USER_ID", UserId));

                    MySqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        DataTable dt = new DataTable();
                        dt.Load(reader);
                        reader.Close();

                        DataRow row = dt.Rows[0];
                        resp.UserId = UserId;
                        resp.FirstName = row["FIRST_NAME"]?.ToString();
                        resp.LastName = row["LAST_NAME"]?.ToString();
                        resp.UserName = row["USER_NAME"]?.ToString();
                        resp.Status = row["STATUS"] != DBNull.Value ? Convert.ToChar(row["STATUS"]) : 'A';
                        resp.UpdateDate = row["UPDATE_DATE"] != DBNull.Value ? Convert.ToDateTime(row["UPDATE_DATE"]) : DateTime.MinValue;
                        resp.CreateDate = row["CREATE_DATE"] != DBNull.Value ? Convert.ToDateTime(row["CREATE_DATE"]) : DateTime.MinValue;
                        resp.isSuccess = true;
                        resp.Message = "User retrieved successfully.";
                    }
                    else
                    {
                        resp.isSuccess = false;
                        resp.Message = "User not found.";
                    }

                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                resp.Message = ex.ToString();
                resp.isSuccess = false;
            }

            return resp;
        }


    }
}
