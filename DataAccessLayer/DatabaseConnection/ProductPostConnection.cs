using DataAccessLayer.Infrastructure;
using DTO;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer.DatabaseConnection
{
    public class ProductPostConnection : IProductPostConnection
    {
        private readonly string _connectionString;
        private readonly Dictionary<string, int> category = new Dictionary<string, int>()
        {
            { "Clothing", 1 }, { "Electronics & Computers", 2 }, { "Health", 3 }, { "Food", 4 }, 
            { "Beauty", 5 }, { "At Home", 6 }, { "Rental", 7 }, { "Sports & Outdoors", 8 }, { "Other", 9 }
        };

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
                string query = @"SELECT PostID, LastUpdateDate, City, State, 
                                 CategoryId, Price, Subject, Content, ViewCount 
                                 FROM [Product_Post] WHERE UserID = @UserID AND PostID = @PostID";
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
                        Category = category.FirstOrDefault(x => x.Value == (int)reader["CategoryId"]).Key,
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


        public async Task<MyPost> GetPostNoAuth(int postID)
        {
            try
            {
                MyPost post = null;
                using SqlConnection sqlcon = new SqlConnection(_connectionString);
                string query = @"SELECT PostID, LastUpdateDate, City, State, CategoryId,
                                 Price, Subject, Content, ViewCount, Username 
                                 FROM [Product_Post] AS P JOIN [User] AS U 
                                 ON P.UserID = U.UserID WHERE PostID = @PostID";
                using SqlCommand sqlcmd = new SqlCommand(query, sqlcon);
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
                        Category = category.FirstOrDefault(x => x.Value == (int)reader["CategoryId"]).Key,
                        Price = (decimal)reader["Price"],
                        Subject = (string)reader["Subject"],
                        Content = (string)reader["Content"],
                        Username = (string)reader["Username"],
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
                string query = @"SELECT PostID, LastUpdateDate, Subject, Content, ViewCount 
                                 FROM [Product_Post] WHERE UserID = @UserID ORDER BY [PostDate] DESC";
                using SqlCommand sqlcmd = new SqlCommand(query, sqlcon);
                sqlcmd.Parameters.AddWithValue("@UserID", await userId);            
                sqlcon.Open();
                using SqlDataReader reader = sqlcmd.ExecuteReader();
                while (await reader.ReadAsync())
                {
                    MyPost post = new MyPost()
                    {
                        PostID = (int)reader["PostID"],
                        LastUpdateDate = (DateTime)reader["LastUpdateDate"],
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


        public async Task<List<MyPost>> GetListing()
        {
            List<MyPost> listing = new List<MyPost>();
            using SqlConnection sqlcon = new SqlConnection(_connectionString);
            string query = @"SELECT TOP(10) PostID, LastUpdateDate,
                             City, State, CategoryId, Price,
                             Subject, Content, ViewCount, Username 
                             FROM [Product_Post] AS P JOIN [User] AS U 
                             ON U.UserID = P.UserID ORDER BY [PostDate] DESC";
            using SqlCommand sqlcmd = new SqlCommand(query, sqlcon);
            try
            {
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
                        Category = category.FirstOrDefault(x => x.Value == (int)reader["CategoryId"]).Key,
                        Price = (decimal)reader["Price"],
                        Subject = (string)reader["Subject"],
                        Content = (string)reader["Content"],
                        Username = (string)reader["Username"],
                        ViewCount = (int)reader["ViewCount"]
                    };
                    listing.Add(post);
                }
                return listing;
            }
            catch (Exception ex) { throw; }
        }


        public async Task<EStatus> NewProductPost(ProductPost productPost, string email)
        {
            try
            {
                if (category.ContainsKey(productPost.Category))
                {
                    Task<int> userId = getUserId(email);
                    using SqlConnection sqlcon = new SqlConnection(_connectionString);
                    using SqlCommand sqlcmd = new SqlCommand("SP_NEW_PRODUCT_POST", sqlcon) { CommandType = CommandType.StoredProcedure };
                    sqlcmd.Parameters.AddWithValue("@State", productPost.State);
                    sqlcmd.Parameters.AddWithValue("@City", productPost.City);
                    sqlcmd.Parameters.AddWithValue("@Subject", productPost.Subject);
                    sqlcmd.Parameters.AddWithValue("@CategoryId", category[productPost.Category]);
                    sqlcmd.Parameters.AddWithValue("@Price", productPost.Price);
                    sqlcmd.Parameters.AddWithValue("@Content", productPost.Content);
                    sqlcmd.Parameters.AddWithValue("@UserID", await userId);
                    sqlcmd.Parameters.Add(new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    sqlcon.Open();
                    await sqlcmd.ExecuteNonQueryAsync();
                    return (EStatus)sqlcmd.Parameters["@ReturnValue"].Value;
                }
                else               
                    throw new Exception("Category undefined");              
            }
            catch (Exception ex) { throw; }
        }


        public async Task<EStatus> UpdatePost(ProductPost productPost, string email, int postId)
        {
            try
            {
                if (category.ContainsKey(productPost.Category))
                {
                    Task<int> userId = getUserId(email);
                    using SqlConnection sqlcon = new SqlConnection(_connectionString);
                    using SqlCommand sqlcmd = new SqlCommand("SP_UPDATE_POST", sqlcon) { CommandType = CommandType.StoredProcedure };
                    sqlcmd.Parameters.AddWithValue("@State", productPost.State);
                    sqlcmd.Parameters.AddWithValue("@City", productPost.City);
                    sqlcmd.Parameters.AddWithValue("@Subject", productPost.Subject);
                    sqlcmd.Parameters.AddWithValue("@CategoryId", category[productPost.Category]);
                    sqlcmd.Parameters.AddWithValue("@Price", productPost.Price);
                    sqlcmd.Parameters.AddWithValue("@Content", productPost.Content);
                    sqlcmd.Parameters.AddWithValue("@PostID", postId);
                    sqlcmd.Parameters.AddWithValue("@UserID", await userId);
                    sqlcmd.Parameters.Add(new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output });
                    sqlcon.Open();
                    int result = await sqlcmd.ExecuteNonQueryAsync();
                    return result > 0 ? EStatus.Succeeded : EStatus.Failed;
                }
                else
                    throw new Exception("Category undefined");
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


        public async Task<EStatus> NewComment(MyComment comment, string email)
        {
            try
            {
                Task<int> userId = getUserId(email);
                string query = @"INSERT INTO [Product_Comment] 
                                 VALUES(@Content, GETDATE(), 0, @PostID, @UserID)";
                using SqlConnection sqlcon = new SqlConnection(_connectionString);
                using SqlCommand sqlcmd = new SqlCommand(query, sqlcon);
                sqlcmd.Parameters.AddWithValue("@Content", comment.Content);
                sqlcmd.Parameters.AddWithValue("@PostID", comment.PostID);
                sqlcmd.Parameters.AddWithValue("@UserID", await userId);
                sqlcon.Open();
                int result = await sqlcmd.ExecuteNonQueryAsync();
                return result > 0 ? EStatus.Succeeded : EStatus.Failed;
            }
            catch (Exception ex) { throw; }
        }


        public async Task<List<CommentDTO>> GetCommentList(int postId)
        {
            List<CommentDTO> commentList = new List<CommentDTO>();
            string query = @"SELECT Content, CommentDate, Username FROM Product_Comment AS P 
                             JOIN [User] AS U ON P.UserID = U.UserID WHERE PostID = @PostID 
                             ORDER BY CommentDate ASC";
            using SqlConnection sqlcon = new SqlConnection(_connectionString);
            using SqlCommand sqlcmd = new SqlCommand(query, sqlcon);
            sqlcmd.Parameters.AddWithValue("@PostID", postId);
            try
            {
                sqlcon.Open();
                using SqlDataReader reader = await sqlcmd.ExecuteReaderAsync();
                while (reader.Read())
                {
                    CommentDTO comment = new CommentDTO()
                    {
                        Username = (string)reader["Username"],
                        Content = (string)reader["Content"],
                        CommentDate = (DateTime)reader["CommentDate"]
                    };
                    commentList.Add(comment);
                }
                return commentList;
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
                string query = @"SELECT [UserID] FROM [User] AS U JOIN [Account] AS A 
                                 ON A.AccountID = U.AccountID AND A.NormalizedEmail = @Email";
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
