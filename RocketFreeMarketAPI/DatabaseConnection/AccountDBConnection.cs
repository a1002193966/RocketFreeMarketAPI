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
                            sqlcmd.Parameters.AddWithValue("@PasswordHash", secret.Cipher);
                            sqlcmd.Parameters.AddWithValue("@AESKey", secret.Key);
                            sqlcmd.Parameters.AddWithValue("@AESIV", secret.IV);

                            int result = sqlcmd.ExecuteNonQuery();
                            return result > 0;
                        }
                        catch(Exception e)
                        {
                            throw;
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
                        throw;
                    }
                    finally
                    {
                        sqlconn.Close();
                    }
                }
            }
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
                                secret.Cipher = (byte[])reader["PasswordHash"];
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
                        throw;
                    }
                    finally
                    {
                        sqlconn.Close();
                    }
                }
            }
        }



        public async Task<List<Account>> GetAllAccountInfo()
        {
            List<Account> AccountList = new List<Account>();
            using (SqlConnection sqlconn = new SqlConnection(_connectionString))
            {
                string cmd = "SELECT * FROM Account";
                using SqlCommand sqlcmd = new SqlCommand(cmd, sqlconn);
                try
                {
                    sqlconn.Open();
                    using (SqlDataReader reader = await sqlcmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Secret secret = new Secret
                            {
                                Cipher = (byte[])reader["PasswordHash"],
                                Key = (byte[])reader["AESKey"],
                                IV = (byte[])reader["AESIV"]
                            };

                            Account account = new Account
                            {
                                AccountID = (int)reader["AccountID"],
                                PhoneNumber = (string)reader["PhoneNumber"],
                                Email = (string)reader["Email"],
                                PasswordHash = CryptoProcess.Decrypt_Aes(secret),
                                AESKey = (byte[])reader["AESKey"],
                                AESIV = (byte[])reader["AESIV"],
                                CreationDate = (DateTime)reader["CreationDate"],
                                UpdateDate = (DateTime)reader["UpdateDate"],
                                LastLoginDate = (DateTime)reader["LastLoginDate"],
                                Status = (int)reader["Status"],
                                AccountType = (string)reader["AccountType"]
                            };
                            AccountList.Add(account);
                        }
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
                finally
                {
                    sqlconn.Close();
                }
            }
            return AccountList;
        }












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
