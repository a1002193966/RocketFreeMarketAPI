using DataAccessLayer.Cryptography;
using DataAccessLayer.DatabaseConnection;
using DTO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace DataAccessLayerUnitTest.DatabaseConnection.UnitTest
{
    [TestClass]
    public class AccountConnectionTest
    {
        private const string _connectionString = "Server=.\\SQLEXPRESS; Database=TestRocketFreeMarket; Trusted_Connection=True;";

        private async Task<bool> changeStatus(int status, string email)
        {
            int result;
            using SqlConnection sqlcon = new SqlConnection(_connectionString);
            string cmd = "UPDATE [Account] SET AccountStatus = @Status WHERE Email = @Email";
            using SqlCommand sqlcmd = new SqlCommand(cmd, sqlcon);
            try
            {
                sqlcon.Open();
                sqlcmd.Parameters.AddWithValue("@Status", status);
                sqlcmd.Parameters.AddWithValue("@Email", email);
                result = await sqlcmd.ExecuteNonQueryAsync();
                return result > 0;
            }
            catch (Exception e)
            {
                throw;
            }
        }


        [TestInitialize()]
        public async Task Initialize()
        {
            using SqlConnection sqlcon = new SqlConnection(_connectionString);
            using SqlCommand sqlcmd = new SqlCommand("SP_TRUNCATE_TABLE", sqlcon) { CommandType = CommandType.StoredProcedure };
            try
            {
                sqlcon.Open();
                await sqlcmd.ExecuteNonQueryAsync();
            }
            catch(Exception e)
            {
                throw;
            }
        }


        [TestMethod]
        public async Task Register_NewAccount_ReturnTrue()
        {
            //Arrange
            RegisterInput registerInput = new RegisterInput()
            {
                Email = "chenfan0213@gmail.com",
                Password = "qwerty",
                PhoneNumber = "1234567890"
            };
            CryptoProcess cryptoProcess = new CryptoProcess();
            AccountConnection conn = new AccountConnection(cryptoProcess, _connectionString); 

            //Act
            var result = await conn.Register(registerInput);
            
            //Assert
            Assert.AreEqual(1, result);
        }


        [TestMethod]
        public async Task Register_ExistingAccount_ReturnTrue()
        {
            //Arrange
            RegisterInput registerInput = new RegisterInput()
            {
                Email = "chenfan0213@gmail.com",
                Password = "qwerty",
                PhoneNumber = "1234567890"
            };
            CryptoProcess cryptoProcess = new CryptoProcess();
            AccountConnection conn = new AccountConnection(cryptoProcess, _connectionString);

            //Act
            await conn.Register(registerInput);
            var result = await conn.Register(registerInput);

            //Assert
            Assert.AreEqual(-1, result);
        }


        [TestMethod]
        public async Task Login_WithIncorrectCredential_ReturnTrue()
        {
            //Arrange
            RegisterInput registerInput = new RegisterInput()
            {
                Email = "chenfan0213@gmail.com",
                Password = "qwerty",
                PhoneNumber = "1234567890"
            };
            LoginInput loginInput = new LoginInput()
            {
                Email = "chenfan0213@gmail.com",
                Password = "qwertyxxx"
            };
            CryptoProcess cryptoProcess = new CryptoProcess();
            AccountConnection conn = new AccountConnection(cryptoProcess, _connectionString);

            //Act
            await conn.Register(registerInput);
            var result = await conn.Login(loginInput);

            //Assert
            Assert.AreEqual(-9, result);
        }


        [TestMethod]
        public async Task Login_WithNotRegisteredAccount_ReturnTrue()
        {
            //Arrange
            LoginInput loginInput = new LoginInput()
            {
                Email = "chenfan0213@gmail.com",
                Password = "qwertyxxx"
            };
            CryptoProcess cryptoProcess = new CryptoProcess();
            AccountConnection conn = new AccountConnection(cryptoProcess, _connectionString);

            //Act
            var result = await conn.Login(loginInput);

            //Assert
            Assert.AreEqual(-9, result);
        }

        
        [TestMethod]
        public async Task Login_WithCorrectCredentialButNotVerified_ReturnTrue()
        {
            //Arrange
            RegisterInput registerInput = new RegisterInput()
            {
                Email = "chenfan0213@gmail.com",
                Password = "qwerty",
                PhoneNumber = "1234567890"
            };
            LoginInput loginInput = new LoginInput()
            {
                Email = "chenfan0213@gmail.com",
                Password = "qwerty"
            };
            CryptoProcess cryptoProcess = new CryptoProcess();
            AccountConnection conn = new AccountConnection(cryptoProcess, _connectionString);

            //Act
            await conn.Register(registerInput);
            var result = await conn.Login(loginInput);

            //Assert
            Assert.AreEqual(0, result);
        }


        [TestMethod]
        public async Task Login_WithCorrectCredentialAndVerified_ReturnTrue()
        {
            //Arrange
            RegisterInput registerInput = new RegisterInput()
            {
                Email = "chenfan0213@gmail.com",
                Password = "qwerty",
                PhoneNumber = "1234567890"
            };
            LoginInput loginInput = new LoginInput()
            {
                Email = "chenfan0213@gmail.com",
                Password = "qwerty"
            };
            CryptoProcess cryptoProcess = new CryptoProcess();
            AccountConnection conn = new AccountConnection(cryptoProcess, _connectionString);

            //Act
            await conn.Register(registerInput);
            await changeStatus(1, loginInput.Email);
            var result = await conn.Login(loginInput);

            //Assert
            Assert.AreEqual(1, result);
        }


        [TestMethod]
        public async Task Login_WithCorrectCredentialAndLocked_ReturnTrue()
        {
            //Arrange
            RegisterInput registerInput = new RegisterInput()
            {
                Email = "chenfan0213@gmail.com",
                Password = "qwerty",
                PhoneNumber = "1234567890"
            };
            LoginInput loginInput = new LoginInput()
            {
                Email = "chenfan0213@gmail.com",
                Password = "qwerty"
            };
            CryptoProcess cryptoProcess = new CryptoProcess();
            AccountConnection conn = new AccountConnection(cryptoProcess, _connectionString);

            //Act
            await conn.Register(registerInput);
            await changeStatus(-1, loginInput.Email);
            var result = await conn.Login(loginInput);

            //Assert
            Assert.AreEqual(-1, result);
        }


        [TestMethod]
        public async Task Login_WithCorrectCredentialAndDisabled_ReturnTrue()
        {
            //Arrange
            RegisterInput registerInput = new RegisterInput()
            {
                Email = "chenfan0213@gmail.com",
                Password = "qwerty",
                PhoneNumber = "1234567890"
            };
            LoginInput loginInput = new LoginInput()
            {
                Email = "chenfan0213@gmail.com",
                Password = "qwerty"
            };
            CryptoProcess cryptoProcess = new CryptoProcess();
            AccountConnection conn = new AccountConnection(cryptoProcess, _connectionString);

            //Act
            await conn.Register(registerInput);
            await changeStatus(-7, loginInput.Email);
            var result = await conn.Login(loginInput);

            //Assert
            Assert.AreEqual(-7, result);
        }
    }
}
