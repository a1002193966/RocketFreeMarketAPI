﻿using DataAccessLayer.Infrastructure;
using DTO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace DataAccessLayer.EmailSender
{
    public class EmailSender : IEmailSender
    {
        private readonly ICryptoProcess _cryptoProcess;
        private readonly IConfiguration _configuration;
        private readonly IOptions<SmtpPackageSerialized> _smtpSerialized;

        public EmailSender(IConfiguration configuration, ICryptoProcess cryptoProcess, IOptions<SmtpPackageSerialized> smtpSerialized)
        {
            _cryptoProcess = cryptoProcess;
            _configuration = configuration;
            _smtpSerialized = smtpSerialized;
        }

        public async Task<bool> ExecuteSender(string email, string tokenType)
        {
            try
            {
                string token = generateToken(email);
                int result = await saveToken(email, token, tokenType);
                await sendEmailConfirmation(email, token, tokenType);
                return result > 0;
            }
            catch (Exception ex) { throw; }
        }


        #region Private Help Functions

        private async Task sendEmailConfirmation(string email, string token, string tokenType)
        {
            SmtpPackageSerialized smtpSe = _smtpSerialized.Value;
            string data = JsonConvert.SerializeObject(smtpSe);
            SmtpPackage smtpPackage = JsonConvert.DeserializeObject<SmtpPackage>(data);

            using MailMessage mail = new MailMessage();
            using SmtpClient smtp = new SmtpClient();             
            mail.From = new MailAddress(await _cryptoProcess.Decrypt_Aes(smtpPackage.UsernamePackage));
            mail.To.Add(email);
            mail.IsBodyHtml = true;
            mail.Subject = "Rocket Free Market Email Confirmation";
            mail.Body = tokenType == "Email" ?
                string.Format(_configuration["ConfirmEmail:EmailBody"], string.Format(_configuration["ConfirmEmail:ConfirmationLink"], _cryptoProcess.EncodeText(email), token)) :
                string.Format(_configuration["ResetPasswordConfirmEmail:EmailBody"], string.Format(_configuration["ResetPasswordConfirmEmail:ConfirmationLink"], _cryptoProcess.EncodeText(email), token));

            smtp.Host = smtpPackage.Host;
            smtp.Port = smtpPackage.Port;
            smtp.Credentials = new NetworkCredential(await _cryptoProcess.Decrypt_Aes(smtpPackage.UsernamePackage), await _cryptoProcess.Decrypt_Aes(smtpPackage.PasswordPackage));
            smtp.EnableSsl = true;
            await smtp.SendMailAsync(mail);
        }

 
        private async Task<int> saveToken(string email, string token, string tokenType)
        {
            using SqlConnection sqlcon = new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            try
            {
                sqlcon.Open();
                string cmd = "SP_UPDATE_TOKEN";
                using SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon)
                {
                    CommandType = CommandType.StoredProcedure
                };
                sqlcmd.Parameters.AddWithValue("@Email", email.ToUpper());
                sqlcmd.Parameters.AddWithValue("@Token", token);
                sqlcmd.Parameters.AddWithValue("@TokenType", tokenType);
                int result = await sqlcmd.ExecuteNonQueryAsync();
                return result;
            }
            catch (Exception ex) { throw; }
        }


        private string generateToken(string email)
        {
            using SHA512 algorithm = SHA512.Create();          
            byte[] bytes = algorithm.ComputeHash(Encoding.UTF32.GetBytes(email));
            string byteString = BitConverter.ToString(bytes).Replace("-", "") + " " + DateTime.Now.AddMinutes(15).ToString();
            byte[] byteHash = Encoding.UTF7.GetBytes(byteString);
            string token = JsonConvert.SerializeObject(byteHash).Replace("\"", "");
            return token;
        }

        #endregion
    }
}
