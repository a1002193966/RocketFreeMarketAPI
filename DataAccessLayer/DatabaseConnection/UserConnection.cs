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
        private readonly string _connectionString;

        public UserConnection(IConfiguration configuration)
        {
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
                    user.Username = (string)reader["Username"];
                    user.FirstName = (string)reader["FirstName"];
                    user.LastName = (string)reader["LastName"];
                    user.DOB = reader["DOB"] != DBNull.Value ? (DateTime)reader["DOB"] : (DateTime?)null;
                    user.AccountID = (string)reader["AccountID"];
                    user.UpdateID = (int)reader["UpdateID"];
                    user.UpdateDate = (DateTime)reader["UpdateDate"];
                }
                return user;
            }
            catch (Exception ex) { throw; }
        }


        public async Task<bool> UpdateProfile(ProfileDTO profile)
        {
            using SqlConnection sqlcon = new SqlConnection(_connectionString);
            string cmd = "UPDATE [User] SET Username = @Username, FirstName = @FirstName, LastName = @LastName, DOB = @DOB, UpdateDate = GETDATE() WHERE AccountID = @AccountID";
            using SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            try
            {
                sqlcon.Open();
                sqlcmd.Parameters.AddWithValue("@Username", profile.Username);
                sqlcmd.Parameters.AddWithValue("@FirstName", profile.FirstName);
                sqlcmd.Parameters.AddWithValue("@LastName", profile.LastName);
                sqlcmd.Parameters.AddWithValue("@DOB", profile.DOB);
                sqlcmd.Parameters.AddWithValue("@AccountID", profile.AccountID);
                int result = await sqlcmd.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch(Exception ex) { throw; }
        }
    }
}
