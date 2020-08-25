using DataAccessLayer.Infrastructure;
using DTO;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;


namespace DataAccessLayer.DatabaseConnection
{
    public class AccountConnection : IAccountConnection
    {
        private readonly ICryptoProcess _cryptoProcess;
        private readonly string _connectionString;


        // Used for testing purpose.
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

        
        // <summary>   
        // If email exists => return -1
        // Successfully registered => return 1
        // Database error => 0
        // </summary>
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
                catch (Exception ex) { throw; }
            }
            return -1;     
        }


        // <summary>
        // Incorrect email or password => return -9
        // Successfully logged in => return 1
        // Require email verification => return 0
        // Account Locked => return -1
        // Account disabled => return -7
        // </summary>
        public async Task<int> Login(LoginInput loginInput)
        {
            try
            {
                if (!await isExist(loginInput.Email.ToUpper()))
                    return -9;
                byte[] passwordHash = await getPasswordHash(loginInput.Email, loginInput.Password);
                using SqlConnection sqlcon = new SqlConnection(_connectionString);
                using SqlCommand sqlcmd = new SqlCommand("SP_LOGIN", sqlcon) { CommandType = CommandType.StoredProcedure };
                sqlcmd.Parameters.AddWithValue("@Email", loginInput.Email.ToUpper());
                sqlcmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                sqlcmd.Parameters.Add(new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output });
                sqlcon.Open();
                await sqlcmd.ExecuteNonQueryAsync();
                return (int)sqlcmd.Parameters["@ReturnValue"].Value;
            }
            catch (Exception ex) { throw; }
        }


        // <summary>
        // Activate account by verifying email address.
        // </summary>
        public async Task<int> ActivateAccount(string encryptedEmail, string token)
        {
            bool isTokenExpired = _cryptoProcess.ValidateVerificationToken(token);
            if (isTokenExpired) return -1;
            
            string decryptedEmail = _cryptoProcess.DecodeHash(encryptedEmail).ToUpper();              
            using SqlConnection sqlcon = new SqlConnection(_connectionString);
            using SqlCommand sqlcmd = new SqlCommand("SP_CONFIRM_EMAIL", sqlcon) { CommandType = CommandType.StoredProcedure };
            sqlcmd.Parameters.AddWithValue("@Email", decryptedEmail);
            sqlcmd.Parameters.AddWithValue("@Token", token);
            sqlcmd.Parameters.Add(new SqlParameter("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.Output });
            try
            {
                sqlcon.Open();
                await sqlcmd.ExecuteNonQueryAsync();
                return (int)sqlcmd.Parameters["@ReturnValue"].Value;
            }
            catch (Exception ex) { throw; }
        }



        #region Private Help Functions

        private async Task<byte[]> getPasswordHash(string email, string password)
        {
            byte[] IV = null;
            byte[] Key = null;
            using SqlConnection sqlcon = new SqlConnection(_connectionString);
            string query = "SELECT AesIV, AesKey FROM [Account] JOIN [Access] ON NormalizedEmail = @Email";
            using SqlCommand sqlcmd = new SqlCommand(query, sqlcon);
            sqlcmd.Parameters.AddWithValue("@Email", email.ToUpper());
            try
            {
                sqlcon.Open();
                using SqlDataReader reader = await sqlcmd.ExecuteReaderAsync();
                while(await reader.ReadAsync())
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
                return await reader.ReadAsync();
            }
            catch (Exception ex) { throw; }
        }
     
        #endregion
    }
}
