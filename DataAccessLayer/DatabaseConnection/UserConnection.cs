using DataAccessLayer.Infrastructure;
using DTO;
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
        private readonly string connectionString;
        public UserConnection(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public bool UpdateProfile(ProfileDTO profile)
        {
            using SqlConnection sqlcon = new SqlConnection(connectionString);
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
