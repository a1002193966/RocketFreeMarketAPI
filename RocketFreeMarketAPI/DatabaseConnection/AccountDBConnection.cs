using Microsoft.Extensions.Configuration;
using RocketFreeMarketAPI.Crypto;
using RocketFreeMarketAPI.Infrastracture;
using RocketFreeMarketAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;


namespace RocketFreeMarketAPI.DatabaseConnection
{
    public class AccountDBConnection : IDBConnection
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public AccountDBConnection(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }



        public bool Register(string email, string password, string phoneNumber)
        {
            if (!isExist(email))
            {
                Secret secret = CryptoProcess.Encrypt_Aes(password);
                using(SqlConnection sqlconn = new SqlConnection(_connectionString))
                {
                    string cmd = "INSERT INTO Account "+
                                 "VALUES(@PhoneNumber, @Email, @PasswordHash, @AESKey, @AESIV, GETDATE(), GETDATE(), GETDATE(), 0, 'Customer')";
                    using (SqlCommand sqlcmd = new SqlCommand(cmd, sqlconn))
                    {
                        try
                        {
                            sqlconn.Open();
                            sqlcmd.Parameters.AddWithValue("@PhoneNumber", phoneNumber);
                            sqlcmd.Parameters.AddWithValue("@Email", email);
                            sqlcmd.Parameters.AddWithValue("@PasswordHash", secret.PasswordHash);
                            sqlcmd.Parameters.AddWithValue("@AESKey", secret.Key);
                            sqlcmd.Parameters.AddWithValue("@AESIV", secret.IV);

                            int result = sqlcmd.ExecuteNonQuery();
                            return result > 0;
                        }
                        catch(Exception e)
                        {

                        }
                        finally
                        {
                            sqlconn.Close();
                        }
                    }
                }
            }
            return false;
        }


        private bool isExist(string email)
        {
            using (SqlConnection sqlconn = new SqlConnection(_connectionString))
            {
                string cmd = "SELECT AccountID FROM Account WHERE Email = @Email";
                using (SqlCommand sqlcmd = new SqlCommand(cmd, sqlconn))
                {
                    try
                    {
                        sqlconn.Open();
                        sqlcmd.Parameters.AddWithValue("@Email", email);
                        using (SqlDataReader reader = sqlcmd.ExecuteReader())
                        {
                            return reader.Read();
                        }
                    }
                    catch(Exception e)
                    {            
                    }
                    finally
                    {
                        sqlconn.Close();
                    }
                }
            }
            return false;
        }



        public Account GetAccountInfo(string email)
        {
            Account acc = new Account();
            if(!isExist(email))
            {
                return acc;
            }

            using (SqlConnection sqlconn = new SqlConnection(_connectionString))
            {
                string cmd = "SELECT * FROM Account WHERE Email = @Email";
                using (SqlCommand sqlcmd = new SqlCommand(cmd, sqlconn))
                {
                    try
                    {
                        sqlconn.Open();
                        sqlcmd.Parameters.AddWithValue("@Email", email);
                        using (SqlDataReader reader = sqlcmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Secret secret = new Secret();
                                secret.PasswordHash = (byte[])reader["PasswordHash"];
                                secret.Key = (byte[])reader["AESKey"];
                                secret.IV = (byte[])reader["AESIV"];

                                acc.AccountID = (int)reader["AccountID"];
                                acc.PhoneNumber = reader["PhoneNumber"].ToString();
                                acc.Email = reader["Email"].ToString();
                                acc.PasswordHash = CryptoProcess.Decrypt_Aes(secret);
                                acc.AESKey = (byte[])reader["AESKey"];
                                acc.AESIV = (byte[])reader["AESIV"];
                                acc.CreationDate = (DateTime)reader["CreationDate"];
                                acc.UpdateDate = (DateTime)reader["UpdateDate"];
                                acc.LastLoginDate = (DateTime)reader["LastLoginDate"];
                                acc.Status = (int)reader["Status"];
                                acc.AccountType = (string)reader["AccountType"];
                            }
                            return acc;
                        }
                    }
                    catch(Exception e)
                    {
                        
                    }
                    finally
                    {
                        sqlconn.Close();
                    }
                }
            }
            return acc;
        }
















        //public async Task<List<Account>> ExcuteCommand(string cmd)
        //{
        //    List<Account> AccountList = new List<Account>();
        //    using (SqlConnection sqlconn = new SqlConnection(_connectionString))
        //    {
        //        using (SqlCommand sqlcmd = new SqlCommand(cmd, sqlconn))
        //        {
        //            try
        //            {
        //                sqlconn.Open();
        //                using (SqlDataReader reader = await sqlcmd.ExecuteReaderAsync())
        //                {                        
        //                    while(await reader.ReadAsync())
        //                    {
        //                        Account Acc = new Account
        //                        {
        //                            AccountID = Convert.ToInt32(reader["AccountID"]),
        //                            PhoneNumber = Convert.ToInt32(reader["PhoneNumber"]),
        //                            Email = reader["Email"].ToString(),
        //                            PasswordHash = reader["PasswordHash"].ToString(),
        //                            PasswordSalt = reader["PasswordSalt"].ToString(),
        //                            CreationDate = Convert.ToDateTime(reader["CreationDate"]),
        //                            UpdateDate = Convert.ToDateTime(reader["UpdateDate"]),
        //                            LastLoginDate = Convert.ToDateTime(reader["LastLoginDate"]),
        //                            Status = Convert.ToInt32(reader["Status"]),
        //                            AccountType = reader["AccountType"].ToString()
        //                        };
        //                        AccountList.Add(Acc);
        //                    }
        //                }
        //            }
        //            catch(Exception e)
        //            { 
        //            }
        //            finally
        //            {
        //                sqlconn.Close();
        //            }
        //        }
        //    }
        //    return AccountList;
        //}






















        //private bool verifyAccount(string email, string password)
        //{
        //    using (SqlConnection sqlconn = new SqlConnection(_connectionString))
        //    {
        //        string cmd = "SELECT PasswordHash, Status FROM Account WHERE Email = @email";
        //        using (SqlCommand sqlcmd = new SqlCommand(cmd, sqlconn))
        //        {
        //            try
        //            {
        //                sqlconn.Open();
        //                using (SqlDataReader reader = sqlcmd.ExecuteReader())
        //                {
                           
                            
        //                }
        //            }
        //            catch (Exception e)
        //            {
        //            }
        //            finally
        //            {
        //                sqlconn.Close();
        //            }
        //        }
        //    }
        //}
      
    }
}
