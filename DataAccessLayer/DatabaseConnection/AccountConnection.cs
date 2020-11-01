using DataAccessLayer.Infrastructure;
using DTO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Threading.Tasks;


namespace DataAccessLayer.DatabaseConnection
{
    public class AccountConnection : IAccountConnection
    {
        private readonly ICryptoProcess _cryptoProcess;
        private readonly IEmailSender _emailSender;
        private readonly string _connectionString;


        // Used for testing purpose.
        public AccountConnection(ICryptoProcess cryptoProcess, string connectionString, IEmailSender emailSender)
        {
            _cryptoProcess = cryptoProcess;
            _connectionString = connectionString;
            _emailSender = emailSender;
        }

        public AccountConnection(ICryptoProcess cryptoProcess, IConfiguration configuration, IEmailSender emailSender)
        {
            _cryptoProcess = cryptoProcess;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _emailSender = emailSender;
        }

        
        // <summary>   
        // If email exists => return -1
        // Successfully registered => return 1
        // Database error => 0
        // </summary>
        public async Task<EStatus> Register(RegisterInput registerInput)
        {
            try
            {
                Task<Secret> secret = _cryptoProcess.Encrypt_Aes(registerInput.Password);
                using SqlConnection sqlcon = new SqlConnection(_connectionString);
                using SqlCommand sqlcmd = new SqlCommand("SP_REGISTER", sqlcon) { CommandType = CommandType.StoredProcedure };
                sqlcmd.Parameters.AddWithValue("@Username", registerInput.Username);
                sqlcmd.Parameters.AddWithValue("@FirstName", registerInput.FirstName);
                sqlcmd.Parameters.AddWithValue("@LastName", registerInput.LastName);
                sqlcmd.Parameters.AddWithValue("@PhoneNumber", registerInput.PhoneNumber);
                sqlcmd.Parameters.AddWithValue("@Email", registerInput.Email);
                sqlcmd.Parameters.AddWithValue("@PasswordHash", (await secret).Cipher);
                sqlcmd.Parameters.AddWithValue("@AesIV", (await secret).IV);
                sqlcmd.Parameters.AddWithValue("@AesKey", (await secret).Key);
                sqlcmd.Parameters.Add(new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output });
                sqlcon.Open();
                sqlcmd.ExecuteNonQuery();              
                return (EStatus)sqlcmd.Parameters["@ReturnValue"].Value;
            }
            catch (Exception ex) { throw; }       
        }


        // <summary>
        // Incorrect email or password => return -9
        // Successfully logged in => return 1
        // Require email verification => return 0
        // Account Locked => return -1
        // Account disabled => return -7
        // </summary>
        public async Task<ELoginStatus> Login(LoginInput loginInput)
        {
            try
            {
                byte[] passwordHash = await getPasswordHash(loginInput.Email, loginInput.Password);
                using SqlConnection sqlcon = new SqlConnection(_connectionString);
                using SqlCommand sqlcmd = new SqlCommand("SP_LOGIN", sqlcon) { CommandType = CommandType.StoredProcedure };
                sqlcmd.Parameters.AddWithValue("@Email", loginInput.Email.ToUpper());
                sqlcmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                sqlcmd.Parameters.Add(new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output });
                sqlcon.Open();
                await sqlcmd.ExecuteNonQueryAsync();
                return (ELoginStatus)sqlcmd.Parameters["@ReturnValue"].Value;
            }
            catch (Exception ex) { throw; }
        }


        // <summary>
        // Activate account by verifying email address.
        // </summary>
        public async Task<EStatus> ActivateAccount(string encryptedEmail, string token)
        {
            try 
            {      
                string decryptedEmail = _cryptoProcess.DecodeHash(encryptedEmail).ToUpper();              
                using SqlConnection sqlcon = new SqlConnection(_connectionString);
                using SqlCommand sqlcmd = new SqlCommand("SP_CONFIRM_EMAIL", sqlcon) { CommandType = CommandType.StoredProcedure };
                sqlcmd.Parameters.AddWithValue("@Email", decryptedEmail);
                sqlcmd.Parameters.AddWithValue("@Token", token);
                sqlcmd.Parameters.Add(new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output });          
                sqlcon.Open();
                await sqlcmd.ExecuteNonQueryAsync();
                return (EStatus)sqlcmd.Parameters["@ReturnValue"].Value;                           
            }
            catch (Exception ex) { throw; }
        }



        public async Task<EStatus> ChangePassword(ChangePasswordInput changePasswordInput)
        {
            try
            {
                Task<byte[]> oldPasswordHash = getPasswordHash(changePasswordInput.Email, changePasswordInput.OldPassword);
                Task<Secret> newPasswordSecret = _cryptoProcess.Encrypt_Aes(changePasswordInput.NewPassword);
                using SqlConnection sqlcon = new SqlConnection(_connectionString);
                using SqlCommand sqlcmd = new SqlCommand("SP_CHANGE_PASSWORD", sqlcon) { CommandType = CommandType.StoredProcedure };
                sqlcmd.Parameters.AddWithValue("@Email", changePasswordInput.Email.ToUpper());
                sqlcmd.Parameters.AddWithValue("@OldPasswordHash", await oldPasswordHash);
                sqlcmd.Parameters.AddWithValue("@NewPasswordHash", (await newPasswordSecret).Cipher);
                sqlcmd.Parameters.AddWithValue("@NewKey", (await newPasswordSecret).Key);
                sqlcmd.Parameters.AddWithValue("@NewIV", (await newPasswordSecret).IV);
                sqlcmd.Parameters.Add(new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output });
                sqlcon.Open();
                await sqlcmd.ExecuteNonQueryAsync();
                return (EStatus)sqlcmd.Parameters["@ReturnValue"].Value;
            }
            catch (Exception ex) { throw; }
        }



        public async Task SendResetLink(string email)
        {
            try
            {
                if (await isExist(email.ToUpper()))               
                    await _emailSender.ExecuteSender(email, "Reset");
            }
            catch (Exception ex) { throw; }
        }



        public async Task<EStatus> ResetPassword(ResetPasswordInput resetPasswordInput)
        {
            try
            {              
                bool isTokenExpired = _cryptoProcess.ValidateVerificationToken(resetPasswordInput.Token);
                if (isTokenExpired) return EStatus.TokenExpired;
                string decryptedEmail = _cryptoProcess.DecodeHash(resetPasswordInput.EncryptedEmail).ToUpper();
                Task<Secret> newPasswordSecret = _cryptoProcess.Encrypt_Aes(resetPasswordInput.Password);
                using SqlConnection sqlcon = new SqlConnection(_connectionString);
                using SqlCommand sqlcmd = new SqlCommand("SP_RESET_PASSWORD", sqlcon) { CommandType = CommandType.StoredProcedure };
                sqlcmd.Parameters.AddWithValue("@Email", decryptedEmail);
                sqlcmd.Parameters.AddWithValue("@Token", resetPasswordInput.Token);
                sqlcmd.Parameters.AddWithValue("@NewPasswordHash", (await newPasswordSecret).Cipher);
                sqlcmd.Parameters.AddWithValue("@NewKey", (await newPasswordSecret).Key);
                sqlcmd.Parameters.AddWithValue("@NewIV", (await newPasswordSecret).IV);
                sqlcmd.Parameters.Add(new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output });
                sqlcon.Open();
                await sqlcmd.ExecuteNonQueryAsync();
                return (EStatus)sqlcmd.Parameters["@ReturnValue"].Value;
            }
            catch (Exception ex) { throw; }
        }






        #region Private Help Functions

        private async Task<bool> reCaptchaVerify(string token)
        {
            string key = "6LfYEd0ZAAAAAIzgqOZWKQMJkCX3VvK7JBrRRWIC";
            try
            {
                using HttpClient http = new HttpClient();
                using HttpRequestMessage request = new HttpRequestMessage()
                {
                    Method = HttpMethod.Get,
                    RequestUri = new Uri($"https://www.google.com/recaptcha/api/siteverify?secret={key}&response={token}")
                };
                using HttpResponseMessage response = await http.SendAsync(request);
                string data = await response.Content.ReadAsStringAsync();
                dynamic obj = JsonConvert.DeserializeObject(data);
                return obj.success;
            }
            catch (Exception ex) { throw; }
        }


        private async Task<byte[]> getPasswordHash(string email, string password)
        {
            byte[] IV = null;
            byte[] Key = null;
            using SqlConnection sqlcon = new SqlConnection(_connectionString);
            string query = "SELECT A.AesIV, B.AesKey FROM [Account] AS A JOIN [Access] AS B "+
                "ON A.AccountID = B.AccountID AND NormalizedEmail = @Email";
            using SqlCommand sqlcmd = new SqlCommand(query, sqlcon);
            sqlcmd.Parameters.AddWithValue("@Email", email.ToUpper());
            try
            {
                sqlcon.Open();
                using SqlDataReader reader = sqlcmd.ExecuteReader();
                while(reader.Read())
                {
                    IV = (byte[])reader["AesIV"];
                    Key = (byte[])reader["AesKey"];
                }
                return await _cryptoProcess.Encrypt_Aes_With_Key_IV(password, Key, IV);
            }
            catch (Exception ex) { throw; }

        }


        private async Task<bool> isExist(string email)
        {
            using SqlConnection sqlcon = new SqlConnection(_connectionString);
            string query = "SELECT AccountID FROM [Account] WHERE NormalizedEmail = @NormalizedEmail";
            using SqlCommand sqlcmd = new SqlCommand(query, sqlcon);
            try
            {
                sqlcon.Open();
                sqlcmd.Parameters.AddWithValue("@NormalizedEmail", email.ToUpper());
                using SqlDataReader reader = await sqlcmd.ExecuteReaderAsync();
                return reader.Read();
            }
            catch (Exception ex) { throw; }
        }

        
        #endregion
    }
}
