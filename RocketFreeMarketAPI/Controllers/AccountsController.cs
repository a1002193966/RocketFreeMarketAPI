using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer.Infrastructure;
using Microsoft.AspNetCore.Cors;
using DTO;
using Microsoft.Extensions.Configuration;


namespace RocketFreeMarketAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class AccountsController : ControllerBase    
    {
        private readonly IAccountConnection _conn;
        private readonly IEmailSender _emailSender;
        private readonly ICryptoProcess _cryptoProcess;
        private readonly IConfiguration _configuration;

        public AccountsController(IAccountConnection conn, IEmailSender emailSender, ICryptoProcess cryptoProcess, IConfiguration configuration)
        {
            _conn = conn;
            _emailSender = emailSender;
            _cryptoProcess = cryptoProcess;
            _configuration = configuration;
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
            bool isDone =  _conn.Register(registerInput);
            if(isDone)
            {
                try 
                {
                    _emailSender.ExecuteSender(registerInput.Email);
                }                
                catch(Exception e)
                {
                    throw;
                }
            }
            return isDone;
        }

        [HttpGet("ConfirmEmail")]
        public bool ConfirmEmail(string e, string t)
        {
            // e == email, t == token
            if (e == null || t == null) 
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
