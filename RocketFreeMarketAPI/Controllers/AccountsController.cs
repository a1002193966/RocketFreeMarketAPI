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


        //Login <AccountsController>/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]LoginInput loginInput)
        {
            try
            {
                int status = await _conn.Login(loginInput);
                switch (status)
                {
                    case 1:
                        string tokenString = _loginToken.GenerateToken(loginInput);
                        return Ok(new { status = 1, token = tokenString });
                    case 0:
                        return Unauthorized(new { status = 0, message = "Please verify your email address." });
                    case -1:
                        return Unauthorized(new { status = -1, message = "Account locked. Please reset your password." });
                    case -7:
                        return Unauthorized(new { status = -7, message = "Account disabled. Please contact the customer support." });
                    default:
                        return BadRequest(new { status = -9, message = "Incorrect email or password." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = -9, message = ex.Message });
            }
        }


        // Register <AccountsController>/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody]RegisterInput registerInput)
        {
            try
            {
                int status = await _conn.Register(registerInput);
                switch (status)
                {
                    case 1:
                        await _emailSender.ExecuteSender(registerInput.Email);
                        return Ok(new { status = 1, message = "Successfully registerd." });
                    case -1:
                        return BadRequest(new { status = -1, message = "Account already exists." });
                    default:
                        return BadRequest(new { status = 0, message = "Internal server error." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = 0, message = ex.Message });
            }
        }


        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string e, string t)
        {
            // e == email, t == token
            if (e == null || t == null)
                return BadRequest( new { status = -1, message = "Invalid link." } );
            try
            {
                bool isActivated = await _conn.ActivateAccount(e, t);
                if (isActivated)
                    return Ok(new { status = 1, message = "Account has been activated." });
                else
                    return BadRequest( new { status = 0, message = "Account has already been activated or link expired." } );
            }
            catch (Exception ex)
            {
                return BadRequest( new { status = -1, message = ex.Message } );
            }
        }


        // GetAccountInfo <AccountsController>/test@test.com
        [HttpGet("{email}")]
        public async Task<Account> GetAccountInfo([FromRoute] string email)
        {
            try
            {
                Account account = await _conn.GetAccountInfo(email);
                if (account.AccountID == null)
                {
                    return null;
                }
                return account;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [Authorize]
        [HttpPost("test")]
        public IActionResult test()
        {
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            var cliam = identity.Claims.ToList();
            var email_value = cliam[0].Value;
            return Ok(new { email = email_value });
          
        }
    }
}
