using DataAccessLayer.Cryptography;
using DataAccessLayer.DatabaseConnection;
using DTO;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace DataAccessLayerUnitTest.DatabaseConnection.UnitTest
{
    [TestClass]
    public class AccountConnectionTest
    {
        private readonly string _connectionString = "Server=.\\SQLEXPRESS; Database=RocketFreeMarket; Trusted_Connection=True;";

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
            var result = conn.Register(registerInput);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Login_WithCorrectEmailAndPassword_ReturnTrue()
        {
            //Arrange
            LoginInput loginInput = new LoginInput()
            {
                Email = "chenfan0213@gmail.com",
                Password = "qwerty"
            };
            CryptoProcess cryptoProcess = new CryptoProcess();
            AccountConnection conn = new AccountConnection(cryptoProcess, _connectionString);

            //Act
            var result = conn.Login(loginInput);

            //Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Login_WithCorrectEmailAndIncorrectPassword_ReturnFalse()
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
            var result = conn.Login(loginInput);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Login_WithIncorrectEmailAndCorrectPassword_ReturnFalse()
        {
            //Arrange
            LoginInput loginInput = new LoginInput()
            {
                Email = "chenfan0213xxx@gmail.com",
                Password = "qwerty"
            };
            CryptoProcess cryptoProcess = new CryptoProcess();
            AccountConnection conn = new AccountConnection(cryptoProcess, _connectionString);

            //Act
            var result = conn.Login(loginInput);

            //Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Login_WithIncorrectEmailAndIncorrectPassword_ReturnFalse()
        {
            //Arrange
            LoginInput loginInput = new LoginInput()
            {
                Email = "chenfan0213xxx@gmail.com",
                Password = "qwertyxxx"
            };
            CryptoProcess cryptoProcess = new CryptoProcess();
            AccountConnection conn = new AccountConnection(cryptoProcess, _connectionString);

            //Act
            var result = conn.Login(loginInput);

            //Assert
            Assert.IsFalse(result);
        }
    }
}
