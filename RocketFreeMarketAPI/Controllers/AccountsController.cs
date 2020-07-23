﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer.Infrastructure;
using Microsoft.AspNetCore.Cors;
using DTO;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace RocketFreeMarketAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class AccountsController : ControllerBase    
    {
        private readonly IAccountConnection _conn;
        private readonly IEmailSender _emailSender;

        public AccountsController(IAccountConnection conn, IEmailSender emailSender)
        {
            _conn = conn;
            _emailSender = emailSender;
        }


        // GetAccountInfo <AccountsController>/test@test.com
        [HttpGet("{email}")]
        public Account GetAccountInfo([FromRoute] string email)
        {      
            return _conn.GetAccountInfo(email);
        }

        //Login <AccountsController>/login
        [HttpPost("login")]
        public bool Login([FromBody] LoginInput loginInput)
        {
            return _conn.Login(loginInput);
        }

        // Register <AccountsController>/register
        [HttpPost("register")]
        public bool Register([FromBody] RegisterInput registerInput)
        {
            _emailSender.SendEmailConfirmation(registerInput.Email);
            return _conn.Register(registerInput);
        }

        [HttpGet("ConfirmEmail")]
        public bool ConfirmEmail(string email, string token)
        {
            if (email == null || token == null) 
                return false;

            return true;
        }
        




        // PUT <AccountsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE <AccountsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
