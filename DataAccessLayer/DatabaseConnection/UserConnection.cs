using DataAccessLayer.Infrastructure;
using DTO;
using Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace DataAccessLayer.DatabaseConnection
{
    public class UserConnection : IUserConnection
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public UserConnection(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        public User GetProfile(string email)
        {
            User user = new User();
            using SqlConnection sqlcon = new SqlConnection(_connectionString);
            string cmd = "SELECT * FROM [User] WHERE AccountID = (SELECT AccountID FROM [Account] WHERE Email = @Email)";
            using SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            try
            {
                sqlcon.Open();
                sqlcmd.Parameters.AddWithValue("@Email", email);
                using SqlDataReader reader = sqlcmd.ExecuteReader();
                while (reader.Read())
                {
                    user.UserID = (int)reader["UserID"];
                    user.FirstName = (string)reader["FirstName"];
                    user.LastName = (string)reader["LastName"];
                    user.DOB = (DateTime)reader["DOB"];
                    user.AccountID = (int)reader["AccountID"];
                    user.UpdateID = (int)reader["UpdateID"];
                    user.UpdateDate = (DateTime)reader["UpdateDate"];
                }
                return user;
            }
            catch (Exception)
            {
                throw;
            }
        }



        public bool UpdateProfile(ProfileDTO profile)
        {
            using SqlConnection sqlcon = new SqlConnection(_connectionString);
            string cmd = "UPDATE [User] SET FirstName = @FirstName, LastName = @LastName, DOB = @DOB, UpdateDate = GETDATE() WHERE AccountID = @AccountID";
            using SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            try
            {
                sqlcon.Open();
                sqlcmd.Parameters.AddWithValue("@FirstName", profile.FirstName);
                sqlcmd.Parameters.AddWithValue("@LastName", profile.LastName);
                sqlcmd.Parameters.AddWithValue("@DOB", profile.DOB);
                sqlcmd.Parameters.AddWithValue("@AccountID", profile.AccountID);
                int result = sqlcmd.ExecuteNonQuery();
                return result > 0;
            }
            catch(Exception e)
            {
                throw;
            }
        }
    }
}
