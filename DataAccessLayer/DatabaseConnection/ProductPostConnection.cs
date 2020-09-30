using DataAccessLayer.Infrastructure;
using DTO;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
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


        public async Task<MyPost> GetPost(string email, int postID)
        {
            try
            {
                Task<int> userId = getUserId(email);
                MyPost post = null;
                using SqlConnection sqlcon = new SqlConnection(_connectionString);
                string query = "SELECT PostID, LastUpdateDate, City, State, Category, Price, " +
                    "Subject, Content, ViewCount FROM [Product_Post] WHERE UserID = @UserID AND PostID = @PostID";
                using SqlCommand sqlcmd = new SqlCommand(query, sqlcon);
                sqlcmd.Parameters.AddWithValue("@UserID", await userId);
                sqlcmd.Parameters.AddWithValue("@PostID", postID);
                sqlcon.Open();
                using SqlDataReader reader = await sqlcmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    post = new MyPost()
                    {
                        PostID = (int)reader["PostID"],
                        LastUpdateDate = (DateTime)reader["LastUpdateDate"],
                        City = (string)reader["City"],
                        State = (string)reader["State"],
                        Category = (string)reader["Category"],
                        Price = (decimal)reader["Price"],
                        Subject = (string)reader["Subject"],
                        Content = (string)reader["Content"],
                        ViewCount = (int)reader["ViewCount"]
                    };
                }
                return post;
            }
            catch (Exception ex) { throw; }
        }


        public async Task<List<MyPost>> GetMyListing(string email)
        {
            try
            {
                Task<int> userId = getUserId(email);
                List<MyPost> myListing = new List<MyPost>();
                using SqlConnection sqlcon = new SqlConnection(_connectionString);
                string query = "SELECT PostID, LastUpdateDate, City, State, Category, Price, "+
                    "Subject, Content, ViewCount FROM [Product_Post] WHERE UserID = @UserID";
                using SqlCommand sqlcmd = new SqlCommand(query, sqlcon);
                sqlcmd.Parameters.AddWithValue("@UserID", await userId);            
                sqlcon.Open();
                using SqlDataReader reader = await sqlcmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    MyPost post = new MyPost()
                    {
                        PostID = (int)reader["PostID"],
                        LastUpdateDate = (DateTime)reader["LastUpdateDate"],
                        City = (string)reader["City"],
                        State = (string)reader["State"],
                        Category = (string)reader["Category"],
                        Price = (decimal)reader["Price"],
                        Subject = (string)reader["Subject"],
                        Content = (string)reader["Content"],
                        ViewCount = (int)reader["ViewCount"]
                    };
                    myListing.Add(post);
                }
                return myListing;
            }
            catch (Exception ex) { throw; }
        }


        public async Task<EStatus> NewProductPost(ProductPost productPost, string email)
        {
            try
            {
                Task<int> userId = getUserId(email);
                using SqlConnection sqlcon = new SqlConnection(_connectionString);
                using SqlCommand sqlcmd = new SqlCommand("SP_NEW_PRODUCT_POST", sqlcon) { CommandType = CommandType.StoredProcedure };
                sqlcmd.Parameters.AddWithValue("@State", productPost.State);
                sqlcmd.Parameters.AddWithValue("@City", productPost.City);
                sqlcmd.Parameters.AddWithValue("@Subject", productPost.Subject);
                sqlcmd.Parameters.AddWithValue("@Category", productPost.Category);
                sqlcmd.Parameters.AddWithValue("@Price", productPost.Price);
                sqlcmd.Parameters.AddWithValue("@Content", productPost.Content);
                sqlcmd.Parameters.AddWithValue("@UserID", await userId);
                sqlcmd.Parameters.Add(new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output });
                sqlcon.Open();
                await sqlcmd.ExecuteNonQueryAsync();
                return (EStatus)sqlcmd.Parameters["@ReturnValue"].Value;
            }
            catch (Exception ex) { throw; }
        }


        public async Task<EStatus> UpdatePost(ProductPost productPost, string email, int postId)
        {
            try
            {
                Task<int> userId = getUserId(email);
                using SqlConnection sqlcon = new SqlConnection(_connectionString);
                using SqlCommand sqlcmd = new SqlCommand("SP_UPDATE_POST", sqlcon) { CommandType = CommandType.StoredProcedure };
                sqlcmd.Parameters.AddWithValue("@State", productPost.State);
                sqlcmd.Parameters.AddWithValue("@City", productPost.City);
                sqlcmd.Parameters.AddWithValue("@Subject", productPost.Subject);
                sqlcmd.Parameters.AddWithValue("@Category", productPost.Category);
                sqlcmd.Parameters.AddWithValue("@Price", productPost.Price);
                sqlcmd.Parameters.AddWithValue("@Content", productPost.Content);
                sqlcmd.Parameters.AddWithValue("@PostID", postId);
                sqlcmd.Parameters.AddWithValue("@UserID", await userId);
                sqlcmd.Parameters.Add(new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output });
                sqlcon.Open();
                await sqlcmd.ExecuteNonQueryAsync();
                return (EStatus)sqlcmd.Parameters["@ReturnValue"].Value;
            }
            catch (Exception ex) { throw; }
        }


        public async Task<EStatus> DeletePost(string email, int postId)
        {
            try
            {
                Task<int> userId = getUserId(email);            
                using SqlConnection sqlcon = new SqlConnection(_connectionString);
                using SqlCommand sqlcmd = new SqlCommand("SP_DELETE_POST", sqlcon) { CommandType = CommandType.StoredProcedure };         
                sqlcmd.Parameters.AddWithValue("@PostID", postId);
                sqlcmd.Parameters.AddWithValue("@UserID", await userId);
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
                string query = "SELECT [UserID] FROM [User] AS U JOIN [Account] AS A ON A.AccountID = U.AccountID AND A.NormalizedEmail = @Email";
                using SqlCommand sqlcmd = new SqlCommand(query, sqlcon);
                sqlcmd.Parameters.AddWithValue("@Email", email);
                sqlcon.Open();
                using SqlDataReader reader = await sqlcmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    userId = (int)reader["UserId"];
                    break;
                }
                return userId;
            }
            catch (Exception ex) { throw; }
        }

        #endregion
    }
}
