using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities;
using Microsoft.AspNetCore.Mvc;
using DataAccessLayer.Infrastructure;
using Microsoft.AspNetCore.Cors;
using DTO;
using System.Net.Http;
using System.Net;

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
        public HttpStatusCode GetAccountInfo([FromRoute] string email)
        {
            try
            {
                Account account = _conn.GetAccountInfo(email);
                if (account.AccountID == 0)
                {
                    return HttpStatusCode.NoContent;
                }
            }
            catch(Exception e)
            {
                throw;
            }

            return HttpStatusCode.OK;
        }

        //Login <AccountsController>/login
        [HttpPost("login")]
        public HttpStatusCode Login([FromBody] LoginInput loginInput)
        {
            try
            {
                bool Verified = _conn.Login(loginInput);

                if (Verified)
                {
                    return HttpStatusCode.OK;
                }
            }
            catch(Exception e)
            {
                throw;
            }
            return HttpStatusCode.BadRequest;
        }

        // Register <AccountsController>/register
        [HttpPost("register")]
        public HttpStatusCode Register([FromBody] RegisterInput registerInput)
        {
            HttpStatusCode status = HttpStatusCode.BadRequest;
            try
            {
                bool isDone = _conn.Register(registerInput);

                if (isDone)
                {
                    status = HttpStatusCode.Created;
                    
                    _emailSender.ExecuteSender(registerInput.Email);
                }
            }
            catch (Exception e)
            {
                throw;
            }
            return status;
        }

        [HttpGet("ConfirmEmail")]
        public HttpStatusCode ConfirmEmail(string e, string t)
        {
            // e == email, t == token
            if (e == null || t == null) 
                return HttpStatusCode.Unauthorized;

            return  HttpStatusCode.OK;
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
