using Microsoft.Extensions.Configuration;
using RocketFreeMarketAPI.Infrastracture;
using RocketFreeMarketAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using RocketFreeMarketAPI.Encryption;


namespace RocketFreeMarketAPI.DatabaseConnection
{
    public class AccountDBConnection : IDBConnection
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly AESEncryption _aes = new AESEncryption();
        public AccountDBConnection(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }



        public bool Register(string email, string password, int phoneNumber)
        {
            if (!isExist(email))
            {
                var (AESKey, AESIV) = _aes.CreateAES(password);

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
                            sqlcmd.Parameters.AddWithValue("@PasswordHash", Convert.ToInt32(password));
                            sqlcmd.Parameters.AddWithValue("@AESKey", AESKey);
                            sqlcmd.Parameters.AddWithValue("@AESIV", AESIV);

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


        public async Task<List<Account>> ExcuteCommand(string cmd)
        {
            List<Account> AccountList = new List<Account>();
            using (SqlConnection sqlconn = new SqlConnection(_connectionString))
            {
                using (SqlCommand sqlcmd = new SqlCommand(cmd, sqlconn))
                {
                    try
                    {
                        sqlconn.Open();
                        using (SqlDataReader reader = await sqlcmd.ExecuteReaderAsync())
                        {                        
                            while(await reader.ReadAsync())
                            {
                                Account Acc = new Account
                                {
                                    AccountID = Convert.ToInt32(reader["AccountID"]),
                                    PhoneNumber = Convert.ToInt32(reader["PhoneNumber"]),
                                    Email = reader["Email"].ToString(),
                                    PasswordHash = reader["PasswordHash"].ToString(),
                                    AESKey = (byte[])reader["AESKey"],
                                    AESIV = (byte[])reader["AESIV"],
                                    CreationDate = Convert.ToDateTime(reader["CreationDate"]),
                                    UpdateDate = Convert.ToDateTime(reader["UpdateDate"]),
                                    LastLoginDate = Convert.ToDateTime(reader["LastLoginDate"]),
                                    Status = Convert.ToInt32(reader["Status"]),
                                    AccountType = reader["AccountType"].ToString()
                                };
                                AccountList.Add(Acc);
                            }
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
