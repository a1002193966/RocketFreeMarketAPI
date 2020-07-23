using DataAccessLayer.Infrastructure;
using DTO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace DataAccessLayer.EmailSender
{
    public class EmailSender : IEmailSender
    {
        private readonly ICryptoProcess _cryptoProcess;
        private readonly string _connectionString;
        public EmailSender(IConfiguration configuration, ICryptoProcess cryptoProcess)
        {
            _cryptoProcess = cryptoProcess;
            _connectionString = configuration.GetConnectionString("AccessConnection");
        }

        public void ExecuteSender(string email)
        {
            try 
            {
                string token = generateToken(email);
                saveToken(email, token);
                sendEmailConfirmation(email, token);
            }
            catch(Exception e)
            {
                throw;
            }           
        }

        private void sendEmailConfirmation(string email, string token)
        {      
            using (MailMessage mail = new MailMessage())
            {
                using (SmtpClient smtp = new SmtpClient())
                {
                    SmtpPackage smtpPackage = null;
                    using (StreamReader file = File.OpenText(@"D:\SmtpPackage.json"))
                    {
                        JsonSerializer deserializer = new JsonSerializer();
                        smtpPackage = (SmtpPackage)deserializer.Deserialize(file, typeof(SmtpPackage));
                    }
                 
                    mail.From = new MailAddress(_cryptoProcess.Decrypt_Aes(smtpPackage.UsernamePackage));
                    mail.To.Add(email);
                    mail.IsBodyHtml = true;
                    mail.Subject = "Rocket Free Market Email Confirmation";
                    mail.Body = string.Format(smtpPackage.EmailBody, string.Format(smtpPackage.ConfirmationLink, email, token));

                    smtp.Host = smtpPackage.Host;
                    smtp.Port = smtpPackage.Port;
                    smtp.Credentials = new NetworkCredential(_cryptoProcess.Decrypt_Aes(smtpPackage.UsernamePackage), _cryptoProcess.Decrypt_Aes(smtpPackage.PasswordPackage));
                    smtp.EnableSsl = true;
                    smtp.Send(mail);
                }
            }                  
        }

        private void saveToken(string email, string token)
        {
            using (SqlConnection sqlcon = new SqlConnection(_connectionString))
            {
                sqlcon.Open();
                string cmd = "INSERT INTO [ConfirmationToken](Email, Token, TokenType) VALUES(@Email, @Token, @TokenType)";
                using (SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon))
                {
                    sqlcmd.Parameters.AddWithValue("@Email", email);
                    sqlcmd.Parameters.AddWithValue("@Token", token);
                    sqlcmd.Parameters.AddWithValue("@TokenType", "Email");
                    sqlcmd.ExecuteNonQuery();
                }
            }
        }

        private string generateToken(string email)
        {
            string token = null;
            using (SHA512 algorithm = SHA512.Create())
            {
                byte[] hashedBytes = algorithm.ComputeHash(Encoding.UTF32.GetBytes(email + DateTime.Now.ToString()));
                token = BitConverter.ToString(hashedBytes).Replace("-", "");
            }
            return token;
        }

    }
}   
            
           
  
       
            
        
    

