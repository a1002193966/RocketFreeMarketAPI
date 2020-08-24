using DataAccessLayer.Cryptography;
using DataAccessLayer.Infrastructure;
using DTO;
using Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
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
         * If email exist => return -1
         * Successfully registered => return 1
         * Database error => 0
         */

        public async Task<int> Register(RegisterInput registerInput)
        {
            if (!await isExist(registerInput.Email.ToUpper()))
            {
                string AccountID = _cryptoProcess.AccountIDGenerator(registerInput.Email);
                Secret secret = await _cryptoProcess.Encrypt_Aes(registerInput.Password);
                using SqlConnection sqlcon = new SqlConnection(_connectionString);
                using SqlCommand sqlcmd = new SqlCommand("SP_REGISTER", sqlcon) { CommandType = CommandType.StoredProcedure };
                sqlcmd.Parameters.AddWithValue("@AccountID", AccountID);
                sqlcmd.Parameters.AddWithValue("@PhoneNumber", registerInput.PhoneNumber);
                sqlcmd.Parameters.AddWithValue("@Email", registerInput.Email);
                sqlcmd.Parameters.AddWithValue("@PasswordHash", secret.Cipher);
                sqlcmd.Parameters.AddWithValue("@AesIV", secret.IV);
                sqlcmd.Parameters.AddWithValue("@AesKey", secret.Key);             
                sqlcmd.Parameters.Add(new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output });
                try
                {
                    sqlcon.Open();
                    await sqlcmd.ExecuteNonQueryAsync();
                    return (int)sqlcmd.Parameters["@ReturnValue"].Value;
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            return -1;     
        }


        public async Task<int> Login(LoginInput loginInput)
        {
            try
            {
                if (!await isExist(loginInput.Email.ToUpper()))
                {
                    return -9;
                }
                bool isCredentialMatch = await verifyLogin(loginInput);
                int status = await getAccountStatus(loginInput.Email.ToUpper());
                return isCredentialMatch ? status : -9;              
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        
        public async Task<bool> ActivateAccount(string encryptedEmail, string token)
        {
            bool isTokenExpired = _cryptoProcess.ValidateVerificationToken(token);
            int result = 0;
            if(!isTokenExpired)
            {
                string decryptedEmail = _cryptoProcess.DecodeHash(encryptedEmail).ToUpper();
                bool isTokenMatch = await verifyToken(decryptedEmail, token);
                if (isTokenMatch)
                {
                    using SqlConnection sqlcon = new SqlConnection(_connectionString);
                    using SqlCommand sqlcmd = new SqlCommand(QueryConst.ActivateAccountCMD, sqlcon);
                    sqlcmd.Parameters.AddWithValue("@NormalizedEmail", decryptedEmail);
                    try
                    {
                        sqlcon.Open();
                        result = await sqlcmd.ExecuteNonQueryAsync();
                    }
                    catch (Exception ex)
                    {
                        throw;
                    }
                }
            }          
            return result > 0;
        }


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
                getAccInfocCmd.Parameters.AddWithValue("@NormalizedEmail", email.ToUpper());

                //Get Account infromation from database
                using (SqlDataReader reader = await getAccInfocCmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        account.AccountID = (string)reader["AccountID"];
                        account.PhoneNumber = (string)reader["PhoneNumber"];
                        account.Email = (string)reader["Email"];
                        account.NormalizedEmail = (string)reader["NormalizedEmail"];
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
            catch (Exception ex)
            {
                throw;
            }
        }







        #region Private Help Functions


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
                getHashcmd.Parameters.AddWithValue("@NormalizedEmail", loginInput.Email.ToUpper());

                //get password from database
                using (SqlDataReader reader = await getHashcmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        account.AccountID = (string)reader["AccountID"];
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
            catch (Exception ex)
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
                sqlcmd.Parameters.AddWithValue("@NormalizedEmail", email.ToUpper());
                using SqlDataReader reader = await sqlcmd.ExecuteReaderAsync();
                return await reader.ReadAsync();
            }
            catch (Exception ex)
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
                sqlcmd.Parameters.AddWithValue("@NormalizedEmail", email.ToUpper());
                using (SqlDataReader reader = await sqlcmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        status = (int)reader["AccountStatus"];
                }
                return status;
            }
            catch(Exception ex)
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
                sqlcmd.Parameters.AddWithValue("@Email", email.ToUpper());
                sqlcmd.Parameters.AddWithValue("@Token", token);
                using SqlDataReader reader = await sqlcmd.ExecuteReaderAsync();              
                while (await reader.ReadAsync())
                {
                    return (string)reader["Email"] == email.ToUpper() && (string)reader["Token"] == token;
                }             
                return false;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        #endregion


    }
}
