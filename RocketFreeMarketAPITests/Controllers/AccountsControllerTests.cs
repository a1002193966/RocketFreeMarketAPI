using DataAccessLayer.Cryptography;
using DataAccessLayer.DatabaseConnection;
using DataAccessLayer.EmailSender;
using DataAccessLayer.Infrastructure;
using DTO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Configuration;
using System.Net;

namespace RocketFreeMarketAPI.Controllers.Tests
{
    [TestClass]
    public class AccountsControllerTests: ControllerBase
    {
        private AccountsController controller = new AccountsController();
        private readonly IAccountConnection _accountConnection;
        private readonly IEmailSender _emailSender;
        private readonly ICryptoProcess _cryptoProcess;  
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public AccountsControllerTests(IAccountConnection accountConnection, IEmailSender emailSender, ICryptoProcess cryptoProcess, IConfiguration configuration)
        {
            _accountConnection = accountConnection;
            _emailSender = emailSender;
            _cryptoProcess = cryptoProcess;
            _configuration = configuration;
        }

        [ClassInitialize]
        public void SetUp()
        {
        }

        [TestInitialize]
        public void Initalize()
        {
        }

        [TestMethod]
        public void RegisterTest()
        {
            // Good Cases
            // Arrange


            RegisterInput registerInput = new RegisterInput();
            registerInput.PhoneNumber = "123456";
            registerInput.Email = "test566@gmail.com";
            registerInput.Password = "Hello";

            // Act
            var actual = controller.Register(registerInput);

            // Assert
            var expected = HttpStatusCode.Created;
            Assert.AreEqual(expected, actual);
        }
    }
}