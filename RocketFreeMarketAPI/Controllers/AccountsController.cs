using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataAccessLayer.Infrastructure;
using DTO;
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


        // <summary>      
        // If email exists => return -1
        // Successfully registered => return 1
        // Database error => 0
        // Register <AccountsController>/register
        // </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterInput registerInput)
        {
            try
            {
                EEmailStatus status = (EEmailStatus)await _conn.Register(registerInput);
                switch (status)
                {
                    case EEmailStatus.RegistarEmailSuccess:
                        await _emailSender.ExecuteSender(registerInput.Email);
                        return Ok(new { status = EEmailStatus.RegistarEmailSuccess, message = "Successfully registerd." });
                    case EEmailStatus.EmailExists:
                        return BadRequest(new { status = EEmailStatus.EmailExists, message = "Account already exists." });
                    default:
                        return BadRequest(new { status = EEmailStatus.InternalServerError, message = "Internal server error." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = EEmailStatus.InternalServerError, message = ex.Message });
            }
        }


        // <summary>
        // Incorrect email or password => return -9
        // Successfully logged in => return 1
        // Require email verification => return 0
        // Account Locked => return -1
        // Account disabled => return -7
        // Login <AccountsController>/login
        // </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody]LoginInput loginInput)
        {
            try
            {
                EAccountStatus status = (EAccountStatus)await _conn.Login(loginInput);
                switch (status)
                {
                    case EAccountStatus.LoginSuccess:
                        string tokenString = _loginToken.GenerateToken(loginInput);
                        return Ok(new { status = EAccountStatus.LoginSuccess, token = tokenString });
                    case EAccountStatus.EmailNotActivated:
                        return Unauthorized(new { status = EAccountStatus.EmailNotActivated, message = "Please activate your email address." });
                    case EAccountStatus.AccountLocked:
                        return Unauthorized(new { status = EAccountStatus.AccountLocked, message = "Account locked. Please reset your password." });
                    case EAccountStatus.AccountDisabled:
                        return Unauthorized(new { status = EAccountStatus.AccountDisabled, message = "Account disabled. Please contact the customer support." });
                    default:
                        return BadRequest(new { status = EAccountStatus.WrongLoginInfo, message = "Incorrect email or password." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { status = EAccountStatus.WrongLoginInfo, message = ex.Message });
            }
        }


        // <summary>
        // Activate account by verifying email address.
        // ConfirmEmail <AccountsController>/confirmemail?e={e}&t={t}
        // e => email, t => token
        // </summary>
        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string e, string t)
        {
            if (e == null || t == null)
                return BadRequest( new { status = -1, message = "Invalid link." } );
            try
            {
                int status = await _conn.ActivateAccount(e, t);
                return status switch
                {
                    1 => Ok(new { status = 1, message = "Account has been activated." }),
                    0 => BadRequest(new { status = 0, message = "Account has already been activated or link expired." }),
                    _ => BadRequest(new { status = -1, message = "Internal Server Error." }),
                };
            }
            catch (Exception ex)
            {
                return BadRequest( new { status = -1, message = ex.Message } );
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
