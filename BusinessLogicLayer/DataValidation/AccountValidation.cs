using BusinessLogicLayer.Infrastructure;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DTO;
using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using DataAccessLayer.Infrastructure;

namespace BusinessLogicLayer.DataValidation
{
    public class AccountValidation : IAccountValidation
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly IAccountConnection _conn;
        private readonly ICryptoProcess _cryptoProcess;


        public AccountValidation(IConfiguration configuration, IAccountConnection conn, ICryptoProcess cryptoProcess)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _conn = conn;
            _cryptoProcess = cryptoProcess;
        }


        public async Task<EStatus> RegisterValidation(RegisterInput registerInput)
        {
            try
            {
                bool isReCapchaVerified = await reCaptchaVerify(registerInput.ReCaptchaToken);
                if (!isReCapchaVerified) return EStatus.ReCaptchaFailed;
                bool isEmailExist = await isExist(registerInput.Email);
                if (isEmailExist) return EStatus.EmailExists;
                return await _conn.Register(registerInput);
            }
            catch (Exception ex) { throw; }
        }


        public async Task<ELoginStatus> LoginValidation(LoginInput loginInput)
        {
            try
            {
                bool isReCapchaVerified = await reCaptchaVerify(loginInput.ReCaptchaToken);
                if (!isReCapchaVerified) return ELoginStatus.ReCaptchaFailed;
                bool isEmailExist = await isExist(loginInput.Email);
                if (!isEmailExist) return ELoginStatus.IncorrectCredential;
                return await _conn.Login(loginInput);
            }
            catch (Exception ex) { throw; }
        }


        public async Task<EStatus> ActivateAccountValidation(string e, string t)
        {
            try
            {
                if (e == null || t == null) return EStatus.InvalidLink;
                bool isTokenExpired = _cryptoProcess.ValidateVerificationToken(t);
                if (isTokenExpired) return EStatus.TokenExpired;
                return await _conn.ActivateAccount(e, t);
            }
            catch (Exception ex) { throw; }
        }





        #region Private Help Functions

        private async Task<bool> reCaptchaVerify(string token)
        {
            string key = _configuration.GetSection("ReCaptchaKey").Value;
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
