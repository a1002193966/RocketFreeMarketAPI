using DataAccessLayer.Cryptography;
using DataAccessLayer.Infrastructure;
using DTO;
using Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;

namespace DataAccessLayer.DatabaseConnection
{
    public class AccountConnection : IAccountConnection
    {
        private readonly ICryptoProcess _cryptoProcess;
        private readonly string _connectionString;

        
        //Used for testing purpose.
        public AccountConnection(ICryptoProcess cryptoProcess, string connectionString)
        {
            _cryptoProcess = cryptoProcess;
            _connectionString = connectionString;
        }

        public AccountConnection(ICryptoProcess cryptoProcess, IConfiguration configuration)
        {
            _cryptoProcess = cryptoProcess;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
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
                SqlTransaction sqltrans = null;

                //Establish DBConnection
                var sqlcon = establishSqlConnection();

                //Create Secrect Object
                Secret secret = _cryptoProcess.Encrypt_Aes(registerInput.Password);

                //Creating Account              
                AccountDTO accountDTO = new AccountDTO(registerInput, secret);

                try
                {
                    //open db connection
                    sqlcon.Open();

                    //set up default transaction
                    sqltrans = sqlcon.BeginTransaction();

                    //insert Account to database  
                    int accountInsertResult = insertData(sqlcon, sqltrans, accountDTO, QueryConst.AccountInsertCMD);

                    //if inserted, get the AccountID
                    int accountID = getAccountID(sqlcon, sqltrans, registerInput.Email);

                    UserDTO userDTO = new UserDTO()
                    {
                        AccountID = accountID
                    };

                    //Create Access account class and save the encryption key
                    Access access = new Access()
                    {
                        AccountID = accountID,
                        AesKey = secret.Key
                    };

                    //insert Access Key to database  
                    int accessInsertResult = insertData(sqlcon, sqltrans, access, QueryConst.AccessInsertCMD);

                    //insert User to database
                    int userInsertResult = insertData(sqlcon, sqltrans, userDTO, QueryConst.UserInsertCMD);

                    if (accountID != 0 && accountInsertResult > 0 && accessInsertResult > 0 && userInsertResult > 0)
                    {
                        sqltrans.Commit();
                        //throw new Exception();
                        return true;
                    }
                    else
                    {
                        //if User not inserted, rollback 
                        sqltrans.Rollback();
                        return false;
                    }
                }
                catch (Exception e)
                {
                    if (sqltrans != null)
                        sqltrans.Rollback();
                    return false;
                }
                finally
                {
                    sqlcon.Close();
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
            var sqlcon = establishSqlConnection();
            SqlCommand getAccInfocCmd = new SqlCommand(QueryConst.GetAccountInfoByEmailCMD, sqlcon);
            SqlCommand getAccKeyCmd = new SqlCommand(QueryConst.GetAccountKeyCMD, sqlcon);

            try
            {
                sqlcon.Open();
                getAccInfocCmd.Parameters.AddWithValue("@Email", email);

                //Get Account infromation from database
                using (SqlDataReader reader = getAccInfocCmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        account.AccountID = (int)reader["AccountID"];
                        account.PhoneNumber = (string)reader["PhoneNumber"];
                        account.Email = (string)reader["Email"];
                        account.PasswordHash = (byte[])reader["PasswordHash"];
                        account.AesIV = (byte[])reader["AesIV"];
                        account.CreationDate = (DateTime)reader["CreationDate"];
                        account.UpdateDate = (DateTime)reader["UpdateDate"];
                        account.LastLoginDate = (DateTime)reader["LastLoginDate"];
                        account.EmailVerificationStatus = (bool)reader["EmailVerificationStatus"];
                        account.PhoneVerificationStatus = (bool)reader["PhoneVerificationStatus"];
                        account.AccountStatus = (int)reader["AccountStatus"];
                        account.AccountType = (string)reader["AccountType"];
                    }
                }

                //Assign Aes key from Access database to Account Class
                getAccKeyCmd.Parameters.AddWithValue("@AccountID", account.AccountID);
                using (SqlDataReader accessReader = getAccKeyCmd.ExecuteReader())
                {
                    while (accessReader.Read())
                    {
                        account.AesKey = (byte[])accessReader["AesKey"];
                    }
                }
                return account;
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                getAccInfocCmd.Dispose();
                getAccKeyCmd.Dispose();
                sqlcon.Close();
            }
        }

        
        public bool ActivateAccount(string email, string token)
        {
            int result = 0;
            string emailDecrypted = _cryptoProcess.DecodeHash(email);

            if (verifyToken(emailDecrypted, token))
            {
                using SqlConnection sqlcon = establishSqlConnection();
                using SqlCommand sqlcmd = new SqlCommand(QueryConst.ActivateAccountCMD, sqlcon);
                try
                {
                    sqlcon.Open();
                    sqlcmd.Parameters.AddWithValue("@Email", emailDecrypted);
                    result = sqlcmd.ExecuteNonQuery();
                }
                catch (Exception e)
                {
                    throw;
                }
            }
            return result > 0;
        }


        #region Private Help Functions
        private SqlConnection establishSqlConnection()
        {
            SqlConnection sqlcon = new SqlConnection(_connectionString);
            return sqlcon;
        }

        private int insertData<T>(SqlConnection sqlcon, SqlTransaction sqltrans, T model, String query)
        {
            int result = 0;
            SqlCommand sqlcmd = null;
            try
            {
                sqlcmd = new SqlCommand(query, sqlcon, sqltrans);
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
        private int getAccountID(SqlConnection sqlcon, SqlTransaction sqltrans, string email)
        {
            using SqlCommand sqlcmd = new SqlCommand(QueryConst.GetAccountIDByEmailCMD, sqlcon, sqltrans);
            try
            {
                sqlcmd.Parameters.AddWithValue("@Email", email);
                using SqlDataReader reader = sqlcmd.ExecuteReader();
                int id = 0;
                while (reader.Read())
                {
                    id = (int)reader["AccountID"];
                }
                return id;
            }
            catch (Exception e)
            {
                throw;
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
            var sqlcon = establishSqlConnection();

            SqlCommand getHashcmd = new SqlCommand(QueryConst.GetAccountHashCMD, sqlcon);
            SqlCommand getKeycmd = new SqlCommand(QueryConst.GetAccountKeyCMD, sqlcon);

            try
            {
                sqlcon.Open();
                getHashcmd.Parameters.AddWithValue("@Email", loginInput.Email);

                //get password from database
                using (SqlDataReader reader = getHashcmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        account.AccountID = (int)reader["AccountID"];
                        account.PasswordHash = (byte[])reader["PasswordHash"];
                        account.AesIV = (byte[])reader["AesIV"];
                    }
                }

                //get Aes key from Access database     
                getKeycmd.Parameters.AddWithValue("@AccountID", account.AccountID);
                using (SqlDataReader accessReader = getKeycmd.ExecuteReader())
                {
                    while (accessReader.Read())
                    {
                        account.AesKey = (byte[])accessReader["AesKey"];
                    }
                }
                return hashCompare(account.PasswordHash, _cryptoProcess.Encrypt_Aes_With_Key_IV(loginInput.Password, account.AesKey, account.AesIV));
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                getHashcmd.Dispose();
                getKeycmd.Dispose();
                sqlcon.Close();
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
            using SqlConnection sqlcon = establishSqlConnection();
            using SqlCommand sqlcmd = new SqlCommand(QueryConst.GetAccountIDByEmailCMD, sqlcon);
            try
            {
                sqlcon.Open();
                sqlcmd.Parameters.AddWithValue("@Email", email);
                using SqlDataReader reader = sqlcmd.ExecuteReader();
                return reader.Read();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private int getAccountStatus(string email)
        {
            int status = 0;
            using SqlConnection sqlcon = establishSqlConnection();
            using SqlCommand sqlcmd = new SqlCommand(QueryConst.GetAccountStatusByEmailCMD, sqlcon);
            try
            {
                sqlcon.Open();
                sqlcmd.Parameters.AddWithValue("@Email", email);
                using (SqlDataReader reader = sqlcmd.ExecuteReader())
                {
                    while (reader.Read())
                        status = (int)reader["AccountStatus"];
                }
                return status;
            }
            catch(Exception e)
            {
                throw;
            }
        }


        private bool verifyToken(string email, string token)
        {

            var sqlcon = establishSqlConnection();
            SqlCommand sqlcmd = null;

            try
            {
                sqlcon.Open();
                sqlcmd = new SqlCommand(QueryConst.VerifyTokenCMD, sqlcon);
                sqlcmd.Parameters.AddWithValue("@Email", email);
                sqlcmd.Parameters.AddWithValue("@Token", token);

                using (SqlDataReader reader = sqlcmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        return (string)reader["Email"] == email && (string)reader["Token"] == token;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                sqlcmd.Dispose();
                sqlcon.Close();
            }
        }

        #endregion


    }
}
