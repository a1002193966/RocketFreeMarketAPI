using DataAccessLayer.Infrastructure;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace DataAccessLayer.EmailSender
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration.GetSection("SMTP");
            _connectionString = configuration.GetConnectionString("AccessConnection");
        }

        public void SendEmailConfirmation(string email)
        {
            MailMessage mail = new MailMessage();
            SmtpClient smtp = new SmtpClient();
            SqlConnection sqlcon = null;
            SqlCommand sqlcmd = null;

            try
            {


                string token;
                using (var algorithm = SHA512.Create())
                {
                    var hashedBytes = algorithm.ComputeHash(Encoding.UTF8.GetBytes(email + DateTime.Now.ToString()));
                    token = BitConverter.ToString(hashedBytes).Replace("-", "");
                }

                //SAVE TOKEN TO DATABASE
                
               
                sqlcon = new SqlConnection(_connectionString);
                string cmd = "INSERT INTO [ConfirmationToken](Email, Token, TokenType) VALUES(@Email, @Token, @TokenType)";
                sqlcmd = new SqlCommand(cmd, sqlcon);
                sqlcmd.Parameters.AddWithValue("@Email", email);
                sqlcmd.Parameters.AddWithValue("@Token", token);
                sqlcmd.Parameters.AddWithValue("@TokenType", "Email");
                sqlcon.Open();
                sqlcmd.ExecuteNonQuery();
                





                //SEND CONFIRMATION EMAIL
           
         
                mail.From = new MailAddress(_configuration["Username"]);
                mail.To.Add(email);
                mail.IsBodyHtml = true;
                mail.Subject = "Rocket Free Market Email Confirmation";
                string confirmationLink = "https://localhost:44300/accounts/ConfirmEmail?email=" + email + "&token=" + token;
                string emailBody = "<!DOCTYPE html>" +
                                   "<html>" +
                                   "<head>" +
                                       "<meta charset='utf-8'>" +
                                       "<title>Email Confirmation</title>" +
                                   "</head>" +
                                   "<body>" +
                                       "<h2 style='text-align: center;'>Email Confirmation</h2>" +
                                       "<p style='text-align: center;'>Please click the following link to complete the confirmation process: <br>" +
                                       "<a href='" + confirmationLink + "'>CLICK HERE !</a> <br>" +
                                       "This link will expire in 15 minutes." +
                                       "</p>" +
                                   "</body>" +
                                   "</html>";
                mail.Body = emailBody;

                smtp.Host = _configuration["Host"];
                smtp.Port = Convert.ToInt32(_configuration["Port"]);
                smtp.Credentials = new NetworkCredential(_configuration["Username"], _configuration["Password"]);
                smtp.EnableSsl = true;
                smtp.Send(mail);
          


            }
            catch(Exception e)
            {
                throw;
            }
            finally
            {
                sqlcmd.Dispose();
                sqlcon.Close();
                mail.Dispose();
                smtp.Dispose();
            }
        }
    }
}
