using Microsoft.Extensions.Configuration;
using RocketFreeMarketAPI.Infrastracture;
using RocketFreeMarketAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
namespace RocketFreeMarketAPI.DatabaseConnection
{
    public class DBConnection : IDBConnection
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        public DBConnection(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<Account>> ExcuteCommand(string cmd)
        {
            List<Account> AccountList = new List<Account>();
            using (SqlConnection sqlconn = new SqlConnection(_connectionString))
            {
                using (SqlCommand sqlcmd = new SqlCommand(cmd, sqlconn))
                {
                    try
                    {
                        sqlconn.Open();
                        using (SqlDataReader reader = await sqlcmd.ExecuteReaderAsync())
                        {                        
                            while(await reader.ReadAsync())
                            {
                                Account Acc = new Account
                                {
                                    AccountID = Convert.ToInt32(reader["AccountID"]),
                                    PhoneNumber = Convert.ToInt32(reader["PhoneNumber"]),
                                    Email = reader["Email"].ToString(),
                                    PasswordHash = reader["PasswordHash"].ToString(),
                                    PasswordSalt = reader["PasswordSalt"].ToString(),
                                    CreationDate = Convert.ToDateTime(reader["CreationDate"]),
                                    UpdateDate = Convert.ToDateTime(reader["UpdateDate"]),
                                    LastLoginDate = Convert.ToDateTime(reader["LastLoginDate"]),
                                    Status = Convert.ToInt32(reader["Status"]),
                                    AccountType = reader["AccountType"].ToString()
                                };
                                AccountList.Add(Acc);
                            }
                        }
                    }
                    catch(Exception e)
                    { 
                    }
                    finally
                    {
                        sqlconn.Close();
                    }
                }
            }
            return AccountList;
        }
      
    }
}
