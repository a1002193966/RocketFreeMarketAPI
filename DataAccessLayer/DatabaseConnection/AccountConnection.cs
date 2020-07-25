using DataAccessLayer.Infrastructure;
using DTO;
using Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Web.Providers.Entities;
using System.Text;


namespace DataAccessLayer.DatabaseConnection
{
    public class AccountConnection : IAccountConnection
    {
        private readonly ICryptoProcess _cryptoProcess;
        private readonly IConfiguration _configuration;

        public AccountConnection(ICryptoProcess cryptoProcess, IConfiguration configuration)
        {
            _cryptoProcess = cryptoProcess;
            _configuration = configuration;
        }

        /*
         * Register new user
         * 
         * 
         * check if email is already existed.
         * If not exist, create new account,encrypt password and save to database, then return true
         * if exist, return false
         */


        public bool Register(RegisterInput registerInput)
        {
            //check if email is already existed.
            if (!isExist(registerInput.Email))
            {

                //SQL Transaction set to null
                SqlTransaction defaultTransaction = null;
                SqlTransaction accessTransaction = null;

                //Establish DBConnection
                var (defaultConnection, accessConnection) = establishDBConnection();

                //Create Secrect Object
                Secret secret = _cryptoProcess.Encrypt_Aes(registerInput.Password);

                //Creating Account              
                AccountDTO accountDTO = new AccountDTO(registerInput, secret);
            
                try
                {
                    //open db connection
                    defaultConnection.Open();
                    //set up transaction
                    defaultTransaction = defaultConnection.BeginTransaction();
                    //insert Account to database  
                    int accountInsertResult = insertData(defaultConnection, defaultTransaction, accountDTO, QueryConst.AccountInsertCMD);                 
                    
                    //check if data inserted to database

                    if (accountInsertResult > 0)
                    {
                        //if inserted, get the AccountID
                        int accountID = getAccountID(defaultConnection, defaultTransaction, registerInput.Email);
                        if (accountID != 0)
                        {
                            //Create Access account clas and save the encryption key
                            Access access = new Access()
                            {
                                AccountID = accountID,
                                AesKey = secret.Key
                            };

                            //Open Access db connection
                            accessConnection.Open();
                            accessTransaction = accessConnection.BeginTransaction();
                            int accessInsertResult = insertData(accessConnection, accessTransaction, access, QueryConst.AccessInsertCMD);
                            if (accessInsertResult > 0)
                            {
                                UserDTO userDTO = new UserDTO()
                                {
                                    AccountID = accountID
                                };
                                int userInsertResult = insertData(defaultConnection, defaultTransaction, userDTO, QueryConst.UserInsertCMD);

                                if (userInsertResult > 0)
                                {
                                    //If no error, commit transaction
                                    accessTransaction.Commit();
                                    defaultTransaction.Commit();
                                    return true;
                                }
                                else
                                {
                                    //if User not inserted, rollback 
                                    accessTransaction.Rollback();
                                    defaultTransaction.Rollback();
                                    return false;
                                }
                            }
                            else
                            {
                                //if Access not inserted, rollback
                                accessTransaction.Rollback();
                                defaultTransaction.Rollback();
                                return false;
                            } // end of if (accessInsertResult > 0)
                        }
                        else
                        {
                            //if AccountID not get, rollback
                            defaultTransaction.Rollback();
                            return false;
                        }// end of if (accountID != 0)
                    }
                    else
                    {
                        //if Account not inserted, rollback
                        defaultTransaction.Rollback();
                        return false;
                    }// end of if (accountInsertResult > 0)
                }
                catch (Exception e)
                {
                    //
                    if (defaultConnection != null)
                        defaultTransaction.Rollback();
                    if (accessTransaction != null)
                        accessTransaction.Rollback();
                    return false;
                }
                finally
                {
                    defaultConnection.Close();
                    accessConnection.Close();
                }
            }
            else
                return false;
        }



        public bool Login(LoginInput loginInput)
        {
            return verifyLogin(loginInput);
        }


        /*
         * Get account infromation
         * Parameter : email
         */
        public Account GetAccountInfo(string email)
        {
            Account account = new Account();

            //check if email exist
            if (!isExist(email))
            {
                return account;
            }

            //Establish DBConnection
            var (defaultConnection, accessConnection) = establishDBConnection();

            SqlCommand defaultcmd = new SqlCommand(QueryConst.GetAccountInfoByEmailCMD, defaultConnection);
            SqlCommand accesscmd = new SqlCommand(QueryConst.GetAccountKeyCMD, accessConnection);

            try
            {
                defaultConnection.Open();
                defaultcmd.Parameters.AddWithValue("@Email", email);

                //Get Account infromation from database
                SqlDataReader defaultReader = defaultcmd.ExecuteReader();

                //Assign value to Account class
                while (defaultReader.Read())
                {
                    account.AccountID = (int)defaultReader["AccountID"];
                    account.PhoneNumber = (string)defaultReader["PhoneNumber"];
                    account.Email = (string)defaultReader["Email"];
                    account.PasswordHash = (byte[])defaultReader["PasswordHash"];
                    account.AesIV = (byte[])defaultReader["AesIV"];
                    account.CreationDate = (DateTime)defaultReader["CreationDate"];
                    account.UpdateDate = (DateTime)defaultReader["UpdateDate"];
                    account.LastLoginDate = (DateTime)defaultReader["LastLoginDate"];
                    account.Status = (int)defaultReader["Status"];
                    account.AccountType = (string)defaultReader["AccountType"];
                }
                defaultReader.Close();

                //Assign Aes key from Access database to Account Class
                if (account.Email != null)
                {
                    accessConnection.Open();
                    accesscmd.Parameters.AddWithValue("@AccountID", account.AccountID);
                    SqlDataReader accessReader = accesscmd.ExecuteReader();
                    while (accessReader.Read())
                    {
                        account.AesKey = (byte[])accessReader["AesKey"];
                    }
                    accessReader.Close();
                    return account;
                }
                return null;
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                accesscmd.Dispose();
                defaultcmd.Dispose();
                accessConnection.Close();
                defaultConnection.Close();
            }
        }




        #region Private Help Functions

        private int insertData<T>(SqlConnection conn, SqlTransaction transaction, T model, String cmd)

        {
            int result = 0;
            SqlCommand sqlcmd = null;
            try
            {
                sqlcmd = new SqlCommand(cmd, conn, transaction);
                foreach (var x in model.GetType().GetProperties())
                {
                    sqlcmd.Parameters.AddWithValue("@" + x.Name, x.GetValue(model, null));
                }
                result = sqlcmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                sqlcmd.Dispose();
            }
            return result;
        }


        /*
         * Get Account ID
         *
         */
        private int getAccountID(SqlConnection sqlconn, SqlTransaction sqltrans, string email)
        {
            using (SqlCommand sqlcmd = new SqlCommand(QueryConst.GetAccountIDByEmailCMD, sqlconn, sqltrans))
            {
                try
                {
                    sqlcmd.Parameters.AddWithValue("@Email", email);
                    using (SqlDataReader reader = sqlcmd.ExecuteReader())
                    {
                        int ID = 0;
                        while (reader.Read())
                        {
                            ID = (int)reader["AccountID"];
                        }
                        return ID;
                    }
                }
                catch (Exception e)
                {
                    throw;
                }
                finally
                {
                    sqlcmd.Dispose();
                }
            }
        }

        /*
         * verify Login information
         */
        private bool verifyLogin(LoginInput loginInput)
        {
            Account account = new Account();
            //check if email exist
            if (!isExist(loginInput.Email))
            {
                return false;
            }

            //Establish DBConnection
            var (defaultConnection, accessConnection) = establishDBConnection();

            SqlCommand defaultcmd = new SqlCommand(QueryConst.GetAccountHashCMD, defaultConnection);
            SqlCommand accesscmd = new SqlCommand(QueryConst.GetAccountKeyCMD, accessConnection);

            try
            {
                defaultConnection.Open();
                defaultcmd.Parameters.AddWithValue("@Email", loginInput.Email);
                SqlDataReader defaultReader = defaultcmd.ExecuteReader();

                //get password from database
                while (defaultReader.Read())
                {
                    account.AccountID = (int)defaultReader["AccountID"];
                    account.PasswordHash = (byte[])defaultReader["PasswordHash"];
                    account.AesIV = (byte[])defaultReader["AesIV"];
                }
                defaultReader.Close();

                //get Aes key from Access database
                if (account.AesIV != null)
                {
                    accessConnection.Open();
                    accesscmd.Parameters.AddWithValue("@AccountID", account.AccountID);
                    SqlDataReader accessReader = accesscmd.ExecuteReader();

                    //assign Aes Key to Account class
                    while (accessReader.Read())
                    {
                        account.AesKey = (byte[])accessReader["AesKey"];
                    }
                    accessReader.Close();
                }
                return hashCompare(account.PasswordHash, _cryptoProcess.Encrypt_Aes_With_Key_IV(loginInput.Password, account.AesKey, account.AesIV));
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                accesscmd.Dispose();
                defaultcmd.Dispose();
                accessConnection.Close();
                defaultConnection.Close();
            }
        }


        /*
         * compare hasd password.
         * if all character are same, user enter correct password
         * else, wrong password
         */
        private bool hashCompare(byte[] hash1, byte[] hash2)
        {
            if (hash1.Length != hash2.Length)
                return false;
            for (int i = 0; i < hash1.Length; i++)
            {
                if (hash1[i] != hash2[i])
                    return false;
            }
            return true;
        }

        /*
         * check if email exist
         */
        private bool isExist(string email)
        {
            //string _defaultConnection = _configuration.GetSection("DBSettings").GetSection("DefaultConnection").Value;
            var (_defaultConnection, _) = establishDBConnection();
            using (SqlConnection sqlconn = _defaultConnection)
            {
                using (SqlCommand sqlcmd = new SqlCommand(QueryConst.GetAccountIDByEmailCMD, sqlconn))
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
                    catch (Exception e)
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

        private string decodeEmail(string emailHash)
        {
            dynamic dehash = JsonConvert.DeserializeObject<byte[]>(emailHash);
            string email = Encoding.ASCII.GetString(dehash);
            return email;
        }

        private (SqlConnection, SqlConnection) establishDBConnection()
        {
            string _defaultConnection = _configuration.GetConnectionString("DefaultConnection");
            string _accessConnection = _configuration.GetConnectionString("AccessConnection");

            SqlConnection defaultConnection = new SqlConnection(_defaultConnection);
            SqlConnection accessConnection = new SqlConnection(_accessConnection);

            return (defaultConnection, accessConnection);
        }
        #endregion


    }
}