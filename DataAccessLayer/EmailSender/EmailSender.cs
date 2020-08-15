using DataAccessLayer.Infrastructure;
using DTO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.EmailSender
{
    public class EmailSender : IEmailSender
    {
        private readonly ICryptoProcess _cryptoProcess;
        private readonly string _connectionString;
        public EmailSender(IConfiguration configuration, ICryptoProcess cryptoProcess)
        {
            _cryptoProcess = cryptoProcess;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<bool> ExecuteSender(string email)
        {
            try
            {
                string token = generateToken(email);
                int result = await saveToken(email, token);
                await sendEmailConfirmation(email, token);
                return result > 0;
            }
            catch (Exception)
            {
                throw;
            }
        }


        #region Private Help Functions

        private async Task sendEmailConfirmation(string email, string token)
        {
            using MailMessage mail = new MailMessage();
            using SmtpClient smtp = new SmtpClient();
            SmtpPackage smtpPackage;
            using (StreamReader file = File.OpenText(@"..\DataAccessLayer\EmailSender\SmtpPackage.json"))
            {
                JsonSerializer deserializer = new JsonSerializer();
                smtpPackage = (SmtpPackage)deserializer.Deserialize(file, typeof(SmtpPackage));
            }

            mail.From = new MailAddress(await _cryptoProcess.Decrypt_Aes(smtpPackage.UsernamePackage));
            mail.To.Add(email);
            mail.IsBodyHtml = true;
            mail.Subject = "Rocket Free Market Email Confirmation";
            mail.Body = string.Format(smtpPackage.EmailBody, string.Format(smtpPackage.ConfirmationLink, _cryptoProcess.EncodeText(email), token));

            smtp.Host = smtpPackage.Host;
            smtp.Port = smtpPackage.Port;
            smtp.Credentials = new NetworkCredential(await _cryptoProcess.Decrypt_Aes(smtpPackage.UsernamePackage), await _cryptoProcess.Decrypt_Aes(smtpPackage.PasswordPackage));
            smtp.EnableSsl = true;
            await smtp.SendMailAsync(mail);
        }

 
        private async Task<int> saveToken(string email, string token)
        {
            using SqlConnection sqlcon = new SqlConnection(_connectionString);
            sqlcon.Open();
            string cmd = "SP_UPDATE_TOKEN";
            using SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon)
            {
                CommandType = CommandType.StoredProcedure
            };
            sqlcmd.Parameters.AddWithValue("@Email", email.ToUpper());
            sqlcmd.Parameters.AddWithValue("@Token", token);
            int result = await sqlcmd.ExecuteNonQueryAsync();
            return result;
        }

        private string generateToken(string email)
        {
            string token = null;
            using (SHA512 algorithm = SHA512.Create())
            {
                byte[] hashedBytes = algorithm.ComputeHash(Encoding.UTF32.GetBytes(email.ToUpper() + DateTime.Now.ToString()));
                token = BitConverter.ToString(hashedBytes).Replace("-", "");
            }
            return token;
        }

        #endregion
    }
}
