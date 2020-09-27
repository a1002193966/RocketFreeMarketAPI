using DataAccessLayer.Infrastructure;
using DTO;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.DatabaseConnection
{
    public class ProductPostConnection : IProductPostConnection
    {
        private readonly string _connectionString;

        public ProductPostConnection(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }


        public async Task<EStatus> NewProductPost(ProductPost productPost, string email)
        {
            try
            {
                int userId = await getUserId(email);
                using SqlConnection sqlcon = new SqlConnection(_connectionString);
                using SqlCommand sqlcmd = new SqlCommand("SP_NEW_PRODUCT_POST", sqlcon) { CommandType = CommandType.StoredProcedure };
                sqlcmd.Parameters.AddWithValue("@State", productPost.State);
                sqlcmd.Parameters.AddWithValue("@City", productPost.City);
                sqlcmd.Parameters.AddWithValue("@Subject", productPost.Subject);
                sqlcmd.Parameters.AddWithValue("@Category", productPost.Category);
                sqlcmd.Parameters.AddWithValue("@Price", productPost.Price);
                sqlcmd.Parameters.AddWithValue("@Content", productPost.Content);
                sqlcmd.Parameters.AddWithValue("@UserID", userId);
                sqlcmd.Parameters.Add(new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output });
                sqlcon.Open();
                await sqlcmd.ExecuteNonQueryAsync();
                return (EStatus)sqlcmd.Parameters["@ReturnValue"].Value;
            }
            catch (Exception ex) { throw; }
        }



        #region Private Help Function

        private async Task<int> getUserId(string email)
        {
            try
            {
                int userId = 0;
                using SqlConnection sqlcon = new SqlConnection(_connectionString);
                string query = "SELECT [UserID] FROM [User] AS u JOIN [Account] AS a ON a.AccountID = u.AccountID AND Email = @Email";
                using SqlCommand sqlcmd = new SqlCommand(query, sqlcon);
                sqlcmd.Parameters.AddWithValue("@Email", email);
                sqlcon.Open();
                using SqlDataReader reader = await sqlcmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    userId = (int)reader["UserId"];
                }
                return userId;
            }
            catch (Exception ex) { throw; }
        }

        #endregion
    }
}
