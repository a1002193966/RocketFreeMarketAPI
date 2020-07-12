using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using RocketFreeMarketAPI.Crypto;
using RocketFreeMarketAPI.Infrastracture;
using RocketFreeMarketAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;


namespace RocketFreeMarketAPI.DatabaseConnection
{
    public class AccountDBConnection : IDBConnection       
    {
        private readonly IConfiguration _configuration;
        private readonly ICryptoProcess _cryptoProcess;
        private readonly string _defaultConnection;
        private readonly string _accessConnection;
        public AccountDBConnection(IConfiguration configuration, ICryptoProcess cryptoProcess)
        {
            _configuration = configuration;
            _cryptoProcess = cryptoProcess;
            _defaultConnection = _configuration.GetConnectionString("DefaultConnection");
            _accessConnection = _configuration.GetConnectionString("AccessConnection");
        }

        public bool Register(RegisterInput registerInput)
        {
            if (!isExist(registerInput.Email))
            {
                Secret secret = _cryptoProcess.Encrypt_Aes(registerInput.Password);
                string defCmd = "INSERT INTO Account " +
                                "VALUES(@PhoneNumber, @Email, @PasswordHash, @AesIV, GETDATE(), GETDATE(), GETDATE(), 0, 'Customer')";
                string accCmd = "INSERT INTO Access " +
                                "VALUES(@AccountID, @AesKey)";

                SqlConnection defConn = new SqlConnection(_defaultConnection);
                SqlConnection accConn = new SqlConnection(_accessConnection);          
                SqlCommand defaultcmd = new SqlCommand(defCmd, defConn);
                SqlCommand accesscmd = new SqlCommand(accCmd, accConn);

                try
                {
                    defConn.Open(); 
                    defaultcmd.Parameters.AddWithValue("@PhoneNumber", registerInput.PhoneNumber);
                    defaultcmd.Parameters.AddWithValue("@Email", registerInput.Email);
                    defaultcmd.Parameters.AddWithValue("@PasswordHash", secret.Cipher);
                    defaultcmd.Parameters.AddWithValue("@AesIV", secret.IV);
                    int defaultResult = defaultcmd.ExecuteNonQuery();
                    if(defaultResult > 0)
                    {
                        accConn.Open();
                        accesscmd.Parameters.AddWithValue("@AccountID", getAccountID(registerInput.Email));
                        accesscmd.Parameters.AddWithValue("@AesKey", secret.Key);
                        int accessResult = accesscmd.ExecuteNonQuery();
                        return accessResult > 0;
                    }
                    return false;                                 
                }
                catch(Exception e)
                {
                    throw;
                }
                finally
                {
                    defaultcmd.Dispose();
                    accesscmd.Dispose();
                    defConn.Close();
                    accConn.Close();
                }        
            }
            return false;
        }


        public Account GetAccountInfo(string email)
        {
            Account account = new Account();
            if (!isExist(email))
            {
                return account;
            }

            string defCmd = "SELECT * FROM Account WHERE Email = @Email";
            string accCmd = "SELECT AesKey FROM Access WHERE AccountID = @AccountID";
     
            SqlConnection defConn = new SqlConnection(_defaultConnection);
            SqlConnection accConn = new SqlConnection(_accessConnection);
            SqlCommand defaultcmd = new SqlCommand(defCmd, defConn);
            SqlCommand accesscmd = new SqlCommand(accCmd, accConn);

            try
            {
                defConn.Open();
                defaultcmd.Parameters.AddWithValue("@Email", email);
                SqlDataReader defaultReader = defaultcmd.ExecuteReader();
                while(defaultReader.Read())
                {
                    account.AccountID = (int)defaultReader["AccountID"];
                    account.PhoneNumber = (string)defaultReader["PhoneNumber"];
                    account.Email = (string)defaultReader["Email"];
                    account.PasswordHash = (byte[])defaultReader["PasswordHash"];
                    account.AesIV = (byte[])defaultReader["AesIV"];
                    account.CreationDate = (DateTime)defaultReader["CreationDate"];
                    account.UpdateDate = (DateTime)defaultReader["UpdateDate"];
                    account.LastLoginDate = (DateTime)defaultReader["LastLoginDate"];
                    account.Status = (int)defaultReader["Status"];
                    account.AccountType = (string)defaultReader["AccountType"];
                }
                defaultReader.Close();
                if(account != null)
                {
                    accConn.Open();
                    accesscmd.Parameters.AddWithValue("@AccountID", getAccountID(email));
                    SqlDataReader accessReader = accesscmd.ExecuteReader();
                    while(accessReader.Read())
                    {
                        account.AesKey = (byte[])accessReader["AesKey"];
                    }
                    accessReader.Close();
                    return account;
                }
                return null;
            }
            catch(Exception e)
            {
                throw;
            }
            finally
            {
                accesscmd.Dispose();
                defaultcmd.Dispose();
                accConn.Close();
                defConn.Close();
            }
        }

        private bool isExist(string email)
        {
            using (SqlConnection sqlconn = new SqlConnection(_defaultConnection))
            {
                string cmd = "SELECT AccountID FROM Account WHERE Email = @Email";
                using (SqlCommand sqlcmd = new SqlCommand(cmd, sqlconn))
                {
                    try
                    {
                        sqlconn.Open();
                        sqlcmd.Parameters.AddWithValue("@Email", email);
                        using (SqlDataReader reader = sqlcmd.ExecuteReader())
                        {
                            return reader.Read();
                        }
                    }
                    catch(Exception e)
                    {
                        throw;
                    }
                    finally
                    {
                        sqlconn.Close();
                    }
                }
            }
        }

        private int getAccountID(string email)
        {
            using (SqlConnection sqlconn = new SqlConnection(_defaultConnection))
            {
                string cmd = "SELECT AccountID FROM Account WHERE Email = @Email";
                using (SqlCommand sqlcmd = new SqlCommand(cmd, sqlconn))
                {
                    try
                    {
                        sqlconn.Open();
                        sqlcmd.Parameters.AddWithValue("@Email", email);
                        using (SqlDataReader reader = sqlcmd.ExecuteReader())
                        {
                            int ID = 0;
                            while(reader.Read())
                            {
                                ID = (int)reader["AccountID"];
                            }
                            return ID;
                        }
                    }
                    catch (Exception e)
                    {
                        throw;
                    }
                    finally
                    {
                        sqlconn.Close();
                    }
                }
            }
        }

       


    }
}
