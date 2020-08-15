using DataAccessLayer.Infrastructure;
using DTO;
using Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

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


        public async Task<User> GetProfile(string email)
        {
            User user = new User();
            using SqlConnection sqlcon = new SqlConnection(_connectionString);
            string cmd = "SELECT * FROM [User] WHERE AccountID = (SELECT AccountID FROM [Account] WHERE NormalizedEmail = @NormalizedEmail)";
            using SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            try
            {
                sqlcon.Open();
                sqlcmd.Parameters.AddWithValue("@NormalizedEmail", email.ToUpper());
                using SqlDataReader reader = await sqlcmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    user.UserID = (int)reader["UserID"];
                    _ = reader["FirstName"] != DBNull.Value ? user.FirstName = (string)reader["FirstName"] : user.FirstName = null;
                    _ = reader["LastName"] != DBNull.Value ? user.LastName = (string)reader["LastName"] : user.LastName = null;
                    _ = reader["DOB"] != DBNull.Value ? user.DOB = (DateTime)reader["DOB"] : user.DOB = null;
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



        public async Task<bool> UpdateProfile(ProfileDTO profile)
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
                int result = await sqlcmd.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch(Exception)
            {
                throw;
            }
        }
    }
}
