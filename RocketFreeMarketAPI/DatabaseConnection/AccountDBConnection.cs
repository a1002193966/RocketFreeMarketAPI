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
                SqlConnection defaultConnection = new SqlConnection(_defaultConnection);
                SqlConnection accessConnection = new SqlConnection(_accessConnection);
                SqlTransaction defaultTransaction = null;
                SqlTransaction accessTransaction = null;
                Account account = new Account()
                {
                    Email = registerInput.Email,
                    PasswordHash = secret.Cipher,
                    PhoneNumber = registerInput.PhoneNumber,
                    AesIV = secret.IV,
                    AccountType = "Customer"
                };
                List<string> accountProperty = new List<string>()
                {
                    "PhoneNumber",
                    "Email",
                    "PasswordHash",
                    "AesIV",
                    "AccountType"
                };
                List<string> accessProperty = new List<string>()
                {
                    "AccountID",
                    "AesKey"
                };
                List<string> userProperty = new List<string>()
                {
                    "AccountID"
                };
                string accountInsertCMD = "INSERT INTO Account(PhoneNumber, Email, PasswordHash, AesIV, AccountType) VALUES(@PhoneNumber, @Email, @PasswordHash, @AesIV, @AccountType)";
                string accessInsertCMD = "INSERT INTO Access(AccountID, AesKey) VALUES(@AccountID, @AesKey)";
                string userInsertCMD = "INSERT INTO [User](AccountID) VALUES(@AccountID)";

                try
                {
                    defaultConnection.Open();
                    defaultTransaction = defaultConnection.BeginTransaction();
                    int accountInsertResult = insertData<Account>(defaultConnection, defaultTransaction, account, accountProperty, accountInsertCMD);
                    if (accountInsertResult > 0)
                    {
                        int accountID = getAccountID(defaultConnection, defaultTransaction, registerInput.Email);
                        if (accountID != 0)
                        {
                            Access access = new Access()
                            {
                                AccountID = accountID,
                                AesKey = secret.Key
                            };
                            accessConnection.Open();
                            accessTransaction = accessConnection.BeginTransaction();
                            int accessInsertResult = insertData<Access>(accessConnection, accessTransaction, access, accessProperty, accessInsertCMD);
                            if (accessInsertResult > 0)
                            {
                                User user = new User()
                                {
                                    AccountID = accountID
                                };
                                int userInsertResult = insertData<User>(defaultConnection, defaultTransaction, user, userProperty, userInsertCMD);
                                if(userInsertResult > 0)
                                {
                                    accessTransaction.Commit();
                                    defaultTransaction.Commit();
                                    return true;
                                }
                                else
                                {
                                    accessTransaction.Rollback();
                                    defaultTransaction.Rollback();
                                    return false;
                                }
                            }
                            else
                            {
                                accessTransaction.Rollback();
                                defaultTransaction.Rollback();
                                return false;
                            }
                        }
                        else
                        {
                            defaultTransaction.Rollback();
                            return false;
                        }                       
                    }
                    else
                    {
                        defaultTransaction.Rollback();
                        return false;
                    }            
                }
                catch (Exception e)
                {
                    if (defaultConnection != null)
                        defaultTransaction.Rollback();
                    if (accessTransaction != null)
                        accessTransaction.Rollback();
                    return false;
                }
                finally
                {              
                    defaultConnection.Close();
                    accessConnection.Close();
                }
            }
            else
                return false;
        }


        private int insertData<T>(SqlConnection conn, SqlTransaction transaction, T model, List<String> property, String cmd)
        {
            int result = 0;
            SqlCommand sqlcmd = null;
            try
            {
                sqlcmd = new SqlCommand(cmd, conn, transaction);
                foreach (string x in property)
                {
                    sqlcmd.Parameters.AddWithValue("@" + x, model.GetType().GetProperty(x).GetValue(model, null));
                }
                result = sqlcmd.ExecuteNonQuery();
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                sqlcmd.Dispose();
            }
            return result;
        }


        private int getAccountID(SqlConnection sqlconn, SqlTransaction sqltrans, string email)
        {
            string cmd = "SELECT AccountID FROM Account WHERE Email = @Email";
            using (SqlCommand sqlcmd = new SqlCommand(cmd, sqlconn, sqltrans))
            {
                try
                {
                    sqlcmd.Parameters.AddWithValue("@Email", email);
                    using (SqlDataReader reader = sqlcmd.ExecuteReader())
                    {
                        int ID = 0;
                        while (reader.Read())
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
                    sqlcmd.Dispose();
                }
            }
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
                    accesscmd.Parameters.AddWithValue("@AccountID", account.AccountID);
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

    }
}
