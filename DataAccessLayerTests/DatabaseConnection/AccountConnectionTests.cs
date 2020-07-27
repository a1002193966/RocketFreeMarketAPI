using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataAccessLayer.DatabaseConnection;
using System;
using System.Collections.Generic;
using System.Text;
using DataAccessLayer.Infrastructure;
using DTO;
using Microsoft.Extensions.Configuration;
using Autofac.Extras.Moq;

namespace DataAccessLayer.DatabaseConnection.Tests
{

    [TestClass]
    public class AccountConnectionTests
    {
        private readonly IAccountConnection _conn;

        [TestMethod]
        public void RegisterTest()
        {
            // Arrange
            var registerInput = new RegisterInput();
            registerInput.Email = "122@gmailcom";
            registerInput.Password = "Hello";
            registerInput.PhoneNumber = "123456";

            using(var mock = AutoMock.GetFromRepository((Moq.MockRepository)_conn))
            {
                mock.Mock<IAccountConnection>()
                    .Setup(x => x.Register(registerInput))
                    .Returns(true);
           
                var cls = mock.Create<IAccountConnection>();

                // Act
                var actual = cls.Register(registerInput);

                //Assert
                var expected = true;
                Assert.AreEqual(expected, actual);


            }
           

        }
    }
}