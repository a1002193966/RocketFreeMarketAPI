using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using DataAccessLayer.Infrastructure;
using DTO;
using Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace RocketFreeMarketAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountConnection _conn;
        private readonly IEmailSender _emailSender;
        private readonly ILoginToken _loginToken;

        public AccountsController(IAccountConnection conn, IEmailSender emailSender, ILoginToken loginToken)
        {
            _conn = conn;
            _emailSender = emailSender;
            _loginToken = loginToken;
        }

        // GetAccountInfo <AccountsController>/test@test.com
        [HttpGet("{email}")]
        public async Task<Account> GetAccountInfo([FromRoute] string email)
        {
            try
            {
                Account account = await _conn.GetAccountInfo(email);
                if (account.AccountID == 0)
                {
                    return null;
                }
                return account;
            }
            catch (Exception)
            {
                throw;
            }
        }
        [Authorize]
        [HttpPost("test")]
        public string test()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var cliam = identity.Claims.ToList();
            var email = cliam[0].Value;
            return "Hello " + email;
        }

        //Login <AccountsController>/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginInput loginInput)
        {

            try{
                var result = await _conn.Login(loginInput);
                IActionResult response = Unauthorized();
                if (result == 1)
                {
                    var tokenString = _loginToken.GenerateToken(loginInput);
                    response = Ok(new { token = tokenString });
                }
                return response;

            }
            catch (Exception)
            {
                throw;
            }
        }

        // Register <AccountsController>/register
        [HttpPost("register")]
        public async Task<HttpStatusCode> Register([FromBody] RegisterInput registerInput)
        {
            HttpStatusCode status = HttpStatusCode.BadRequest;
            try
            {
                bool isDone = await _conn.Register(registerInput);
                if (isDone)
                {
                    status = HttpStatusCode.Created;
                    await _emailSender.ExecuteSender(registerInput.Email);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return status;
        }

        [HttpGet("ConfirmEmail")]
        public async Task<HttpStatusCode> ConfirmEmail(string e, string t)
        {
            // e == email, t == token
            try
            {
                if (e == null || t == null || await _conn.ActivateAccount(e, t) == false)
                    return HttpStatusCode.Unauthorized;
            }
            catch
            {
                return HttpStatusCode.Unauthorized;
            }
            return HttpStatusCode.OK;
        }

    }
}
