using DataAccessLayer.Infrastructure;
using Entities;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;


namespace DataAccessLayer.DatabaseConnection
{
    public class AccountConnection : IAccountConnection
    {
        private readonly ICryptoProcess _cryptoProcess;
        private readonly IConfiguration _configuration;
    
        public AccountConnection(ICryptoProcess cryptoProcess, IConfiguration configuration)
        {
            _cryptoProcess = cryptoProcess;
            _configuration = configuration;
        }


        private (SqlConnection, SqlConnection) EstablishDBConnection()
        {
            string _defaultConnection = _configuration.GetSection("DBSettings").GetSection("DefaultConnection").Value;
            string _accessConnection = _configuration.GetSection("DBSettings").GetSection("AccessConnection").Value;

            SqlConnection defaultConnection = new SqlConnection(_defaultConnection);
            SqlConnection accessConnection = new SqlConnection(_accessConnection);

            return (defaultConnection, accessConnection);
        }


        public bool Register(RegisterInput registerInput)
        {
            if (!isExist(registerInput.Email))
            {
                //SQL Transaction set to null
                SqlTransaction defaultTransaction = null;
                SqlTransaction accessTransaction = null;

                //Establish DBConnection
                var (defaultConnection, accessConnection) = EstablishDBConnection();

                //Create Secrect Object
                Secret secret = _cryptoProcess.Encrypt_Aes(registerInput.Password);

                //Creating Account
                Account account = Account.CreateAccount(registerInput, secret);



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

                try
                {
                    defaultConnection.Open();
                    defaultTransaction = defaultConnection.BeginTransaction();
                    int accountInsertResult = insertData(defaultConnection, defaultTransaction, account, accountProperty, QueryConst.AccountInsertCMD);
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
                            int accessInsertResult = insertData<Access>(accessConnection, accessTransaction, access, accessProperty, QueryConst.AccessInsertCMD);
                            if (accessInsertResult > 0)
                            {
                                User user = new User()
                                {
                                    AccountID = accountID
                                };
                                int userInsertResult = insertData<User>(defaultConnection, defaultTransaction, user, userProperty, QueryConst.UserInsertCMD);
                                if (userInsertResult > 0)
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



        public bool Login(LoginInput loginInput)
        {
            return verifyLogin(loginInput);
        }



        public Account GetAccountInfo(string email)
        {
            Account account = new Account();
            if (!isExist(email))
            {
                return account;
            }

            //Establish DBConnection
            var (defaultConnection, accessConnection) = EstablishDBConnection();

            SqlCommand defaultcmd = new SqlCommand(QueryConst.GetAccountInfoByEmailCMD, defaultConnection);
            SqlCommand accesscmd = new SqlCommand(QueryConst.GetAccountKeyCMD, accessConnection);

            try
            {
                defaultConnection.Open();
                defaultcmd.Parameters.AddWithValue("@Email", email);
                SqlDataReader defaultReader = defaultcmd.ExecuteReader();
                while (defaultReader.Read())
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
                if (account.Email != null)
                {
                    accessConnection.Open();
                    accesscmd.Parameters.AddWithValue("@AccountID", account.AccountID);
                    SqlDataReader accessReader = accesscmd.ExecuteReader();
                    while (accessReader.Read())
                    {
                        account.AesKey = (byte[])accessReader["AesKey"];
                    }
                    accessReader.Close();
                    return account;
                }
                return null;
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                accesscmd.Dispose();
                defaultcmd.Dispose();
                accessConnection.Close();
                defaultConnection.Close();
            }
        }




        #region Private Help Functions
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
            using (SqlCommand sqlcmd = new SqlCommand(QueryConst.GetAccountIDByEmailCMD, sqlconn, sqltrans))
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

        private bool verifyLogin(LoginInput loginInput)
        {
            Account account = new Account();
            if (!isExist(loginInput.Email))
            {
                return false;
            }

            //Establish DBConnection
            var (defaultConnection, accessConnection) = EstablishDBConnection();

            SqlCommand defaultcmd = new SqlCommand(QueryConst.GetAccountHashCMD, defaultConnection);
            SqlCommand accesscmd = new SqlCommand(QueryConst.GetAccountKeyCMD, accessConnection);

            try
            {
                defaultConnection.Open();
                defaultcmd.Parameters.AddWithValue("@Email", loginInput.Email);
                SqlDataReader defaultReader = defaultcmd.ExecuteReader();
                while (defaultReader.Read())
                {
                    account.AccountID = (int)defaultReader["AccountID"];
                    account.PasswordHash = (byte[])defaultReader["PasswordHash"];
                    account.AesIV = (byte[])defaultReader["AesIV"];
                }
                defaultReader.Close();
                if (account.AesIV != null)
                {
                    accessConnection.Open();
                    accesscmd.Parameters.AddWithValue("@AccountID", account.AccountID);
                    SqlDataReader accessReader = accesscmd.ExecuteReader();
                    while (accessReader.Read())
                    {
                        account.AesKey = (byte[])accessReader["AesKey"];
                    }
                    accessReader.Close();
                }
                return hashCompare(account.PasswordHash, _cryptoProcess.Encrypt_Aes_With_Key_IV(loginInput.Password, account.AesKey, account.AesIV));
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                accesscmd.Dispose();
                defaultcmd.Dispose();
                accessConnection.Close();
                defaultConnection.Close();
            }
        }

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

        private bool isExist(string email)
        {
            //string _defaultConnection = _configuration.GetSection("DBSettings").GetSection("DefaultConnection").Value;
            var (_defaultConnection, _) = EstablishDBConnection();
            using (SqlConnection sqlconn = _defaultConnection)
            {
                using (SqlCommand sqlcmd = new SqlCommand(QueryConst.GetAccountIDByEmailCMD, sqlconn))
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
        #endregion


    }
}