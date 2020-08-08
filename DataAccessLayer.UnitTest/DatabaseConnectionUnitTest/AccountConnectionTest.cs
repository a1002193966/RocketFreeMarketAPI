using DataAccessLayer.Cryptography;
using DataAccessLayer.DatabaseConnection;
using DataAccessLayer.UnitTest.DatabaseConnectionUnitTest;
using DTO;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;

namespace DataAccessLayerUnitTest.DatabaseConnection.UnitTest
{
    [TestClass]
    public class AccountConnectionTest
    {
        private string _connectionString;
        private ConnectionDTO connection;

        [TestInitialize()]
        public void Initialize()
        {
            using StreamReader file = File.OpenText(@"../../../DatabaseConnectionUnitTest/Config.json");
            JsonSerializer deserializer = new JsonSerializer();
            connection = (ConnectionDTO)deserializer.Deserialize(file, typeof(ConnectionDTO));
            _connectionString = connection.TestConnection;
            using SqlConnection sqlcon = new SqlConnection(_connectionString);
            sqlcon.Open();
            using SqlCommand sqlcmd = new SqlCommand("SP_TRUNCATE_TABLE", sqlcon)
            {
                CommandType = CommandType.StoredProcedure
            };
            sqlcmd.ExecuteNonQuery();
            sqlcon.Close();
        }


        [TestMethod]
        public void Register_NewAccount_ReturnTrue()
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
            var result = conn.Register(registerInput);
            
            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Register_ExistingAccount_ReturnFalse()
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
            conn.Register(registerInput);
            var result = conn.Register(registerInput);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Login_WithCorrectEmailAndPassword_ReturnTrue()
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
            conn.Register(registerInput);
            var result = conn.Login(loginInput);

            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Login_WithCorrectEmailAndIncorrectPassword_ReturnFalse()
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
            conn.Register(registerInput);
            var result = conn.Login(loginInput);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Login_WithIncorrectEmailAndCorrectPassword_ReturnFalse()
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
                Email = "chenfan0213xxx@gmail.com",
                Password = "qwerty"
            };
            CryptoProcess cryptoProcess = new CryptoProcess();
            AccountConnection conn = new AccountConnection(cryptoProcess, _connectionString);

            //Act
            conn.Register(registerInput);
            var result = conn.Login(loginInput);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Login_WithIncorrectEmailAndIncorrectPassword_ReturnFalse()
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
                Email = "chenfan0213xxx@gmail.com",
                Password = "qwertyxxx"
            };
            CryptoProcess cryptoProcess = new CryptoProcess();
            AccountConnection conn = new AccountConnection(cryptoProcess, _connectionString);

            //Act
            conn.Register(registerInput);
            var result = conn.Login(loginInput);

            //Assert
            Assert.IsFalse(result);
        }
    }
}
