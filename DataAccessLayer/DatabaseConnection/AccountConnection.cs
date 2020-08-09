using DataAccessLayer.Cryptography;
using DataAccessLayer.Infrastructure;
using DTO;
using Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

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

        public async Task<bool> Register(RegisterInput registerInput)
        {
            //check if email is already existed.
            if (!await isExist(registerInput.Email))
            {
                //SQL Transaction set to null
                SqlTransaction sqltrans = null;

                //Establish DBConnection
                using SqlConnection sqlcon = new SqlConnection(_connectionString);

                //Create Secrect Object
                Secret secret = await _cryptoProcess.Encrypt_Aes(registerInput.Password);

                //Creating Account              
                AccountDTO accountDTO = new AccountDTO(registerInput, secret);

                try
                {
                    //open db connection
                    sqlcon.Open();

                    //set up default transaction
                    sqltrans = sqlcon.BeginTransaction();

                    //insert Account to database  
                    int accountInsertResult = await insertData(sqlcon, sqltrans, accountDTO, QueryConst.AccountInsertCMD);

                    //if inserted, get the AccountID
                    int accountID = await getAccountID(sqlcon, sqltrans, registerInput.Email);

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
                    int accessInsertResult = await insertData(sqlcon, sqltrans, access, QueryConst.AccessInsertCMD);

                    //insert User to database
                    int userInsertResult = await insertData(sqlcon, sqltrans, userDTO, QueryConst.UserInsertCMD);

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
            }
            else
                return false;
        }


        public async Task<int> Login(LoginInput loginInput)
        {
            try
            {
                if (!await isExist(loginInput.Email))
                {
                    return -9;
                }
                bool isCredentialMatch = await verifyLogin(loginInput);
                int status = await getAccountStatus(loginInput.Email);
                return isCredentialMatch ? status : -9;
            }
            catch (Exception)
            {
                throw;
            }
        }


        /*
         * Get account infromation
         * Parameter : email
         */
        public async Task<Account> GetAccountInfo(string email)
        {
            Account account = new Account();

            //Establish DBConnection
            using SqlConnection sqlcon = new SqlConnection(_connectionString);
            using SqlCommand getAccInfocCmd = new SqlCommand(QueryConst.GetAccountInfoByEmailCMD, sqlcon);
            using SqlCommand getAccKeyCmd = new SqlCommand(QueryConst.GetAccountKeyCMD, sqlcon);
            try
            {
                sqlcon.Open();
                getAccInfocCmd.Parameters.AddWithValue("@Email", email);

                //Get Account infromation from database
                using (SqlDataReader reader = await getAccInfocCmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
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
                using (SqlDataReader accessReader = await getAccKeyCmd.ExecuteReaderAsync())
                {
                    while (await accessReader.ReadAsync())
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
        }

        
        public async Task<bool> ActivateAccount(string email, string token)
        {
            int result = 0;
            string emailDecrypted = _cryptoProcess.DecodeHash(email);
            bool isMatch = await verifyToken(emailDecrypted, token);
            if (isMatch)
            {
                using SqlConnection sqlcon = new SqlConnection(_connectionString);
                using SqlCommand sqlcmd = new SqlCommand(QueryConst.ActivateAccountCMD, sqlcon);
                try
                {
                    sqlcon.Open();
                    sqlcmd.Parameters.AddWithValue("@Email", emailDecrypted);
                    result = await sqlcmd.ExecuteNonQueryAsync();
                }
                catch (Exception e)
                {
                    throw;
                }
            }
            return result > 0;
        }


        #region Private Help Functions


        private async Task<int> insertData<T>(SqlConnection sqlcon, SqlTransaction sqltrans, T model, String query)
        {
            int result;
            using SqlCommand sqlcmd = new SqlCommand(query, sqlcon, sqltrans);
            try
            {            
                foreach (var x in model.GetType().GetProperties())
                {
                    sqlcmd.Parameters.AddWithValue("@" + x.Name, x.GetValue(model, null));
                }
                result = await sqlcmd.ExecuteNonQueryAsync();
            }
            catch (Exception e)
            {
                throw;
            }
            return result;
        }


        /*
         * Get Account ID
         *
         */
        private async Task<int> getAccountID(SqlConnection sqlcon, SqlTransaction sqltrans, string email)
        {
            using SqlCommand sqlcmd = new SqlCommand(QueryConst.GetAccountIDByEmailCMD, sqlcon, sqltrans);
            try
            {
                sqlcmd.Parameters.AddWithValue("@Email", email);
                using SqlDataReader reader = await sqlcmd.ExecuteReaderAsync();
                int id = 0;
                while (await reader.ReadAsync())
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
        private async Task<bool> verifyLogin(LoginInput loginInput)
        {
            Account account = new Account();

            //Establish DBConnection
            using SqlConnection sqlcon = new SqlConnection(_connectionString);
            using SqlCommand getHashcmd = new SqlCommand(QueryConst.GetAccountHashCMD, sqlcon);
            using SqlCommand getKeycmd = new SqlCommand(QueryConst.GetAccountKeyCMD, sqlcon);

            try
            {
                sqlcon.Open();
                getHashcmd.Parameters.AddWithValue("@Email", loginInput.Email);

                //get password from database
                using (SqlDataReader reader = await getHashcmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        account.AccountID = (int)reader["AccountID"];
                        account.PasswordHash = (byte[])reader["PasswordHash"];
                        account.AesIV = (byte[])reader["AesIV"];
                    }
                }

                //get Aes key from Access database     
                getKeycmd.Parameters.AddWithValue("@AccountID", account.AccountID);
                using (SqlDataReader accessReader = await getKeycmd.ExecuteReaderAsync())
                {
                    while (await accessReader.ReadAsync())
                    {
                        account.AesKey = (byte[])accessReader["AesKey"];
                    }
                }
                return hashCompare(account.PasswordHash, await _cryptoProcess.Encrypt_Aes_With_Key_IV(loginInput.Password, account.AesKey, account.AesIV));
            }
            catch (Exception e)
            {
                throw;
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
        private async Task<bool> isExist(string email)
        {
            using SqlConnection sqlcon = new SqlConnection(_connectionString);
            using SqlCommand sqlcmd = new SqlCommand(QueryConst.GetAccountIDByEmailCMD, sqlcon);
            try
            {
                sqlcon.Open();
                sqlcmd.Parameters.AddWithValue("@Email", email);
                using SqlDataReader reader = await sqlcmd.ExecuteReaderAsync();
                return await reader.ReadAsync();
            }
            catch (Exception e)
            {
                throw;
            }
        }

        private async Task<int> getAccountStatus(string email)
        {
            int status = 0;
            using SqlConnection sqlcon = new SqlConnection(_connectionString);
            using SqlCommand sqlcmd = new SqlCommand(QueryConst.GetAccountStatusByEmailCMD, sqlcon);
            try
            {
                sqlcon.Open();
                sqlcmd.Parameters.AddWithValue("@Email", email);
                using (SqlDataReader reader = await sqlcmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        status = (int)reader["AccountStatus"];
                }
                return status;
            }
            catch(Exception e)
            {
                throw;
            }
        }


        private async Task<bool> verifyToken(string email, string token)
        {
            using SqlConnection sqlcon = new SqlConnection(_connectionString);
            using SqlCommand sqlcmd = new SqlCommand(QueryConst.VerifyTokenCMD, sqlcon);
            try
            {
                sqlcon.Open();              
                sqlcmd.Parameters.AddWithValue("@Email", email);
                sqlcmd.Parameters.AddWithValue("@Token", token);
                using SqlDataReader reader = await sqlcmd.ExecuteReaderAsync();              
                while (await reader.ReadAsync())
                {
                    return (string)reader["Email"] == email && (string)reader["Token"] == token;
                }             
                return false;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        #endregion


    }
}
