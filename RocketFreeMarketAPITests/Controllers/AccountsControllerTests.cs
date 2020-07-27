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
        AccountsController _controller;
        private  IAccountConnection _conn;
        private  IEmailSender _emailSender;
        private  ICryptoProcess _cryptoProcess;
        private string _connectionString;
        private IConfiguration _configuration;

        public AccountsControllerTests(IAccountConnection conn, IEmailSender emailSender)
        {
            _conn = conn;
            _emailSender = emailSender;
        }

        [ClassInitialize]
        public void SetUp()
        {
            _cryptoProcess = new CryptoProcess();
            _conn = new AccountConnection(_cryptoProcess,  _configuration);
            _emailSender = new EmailSender(_configuration, _cryptoProcess);
        }

        [TestInitialize]
        public void Initalize()
        {
            _cryptoProcess = new CryptoProcess();
            _conn = new AccountConnection(_cryptoProcess, _configuration);
            _emailSender = new EmailSender(_configuration, _cryptoProcess);
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
            var actual = _controller.Register(registerInput);

            // Assert
            var expected = HttpStatusCode.Created;
            Assert.AreEqual(expected, actual);
        }
    }
}