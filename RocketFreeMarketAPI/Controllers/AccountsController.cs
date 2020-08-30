using System;
using System.Collections.Generic;
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
                ERegisterStatus status = await _conn.Register(registerInput);
                switch (status)
                {
                    case ERegisterStatus.RegisterSucceeded:
                        await _emailSender.ExecuteSender(registerInput.Email);
                        return Ok(new
                        {
                            status = ERegisterStatus.RegisterSucceeded,
                            message = "Successfully registerd."
                        });
                    case ERegisterStatus.EmailExists:
                        return BadRequest(new
                        {
                            status = ERegisterStatus.EmailExists,
                            message = "Account already exists."
                        });
                    default:
                        return BadRequest(new
                        {
                            status = ERegisterStatus.InternalServerError,
                            message = "Internal server error."
                        });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = ERegisterStatus.InternalServerError,
                    message = ex.Message
                });
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
        public async Task<IActionResult> Login([FromBody] LoginInput loginInput)
        {
            try
            {
                ELoginStatus status = await _conn.Login(loginInput);
                switch (status)
                {
                    case ELoginStatus.LoginSucceeded:
                        string tokenString = await _loginToken.GenerateToken(loginInput);
                        return Ok(new
                        {
                            status = ELoginStatus.LoginSucceeded,
                            token = tokenString
                        });
                    case ELoginStatus.EmailNotVerified:
                        return Unauthorized(new
                        {
                            status = ELoginStatus.EmailNotVerified,
                            message = "Please activate your email address."
                        });
                    case ELoginStatus.AccountLocked:
                        return Unauthorized(new
                        {
                            status = ELoginStatus.AccountLocked,
                            message = "Account locked. Please reset your password."
                        });
                    case ELoginStatus.AccountDisabled:
                        return Unauthorized(new
                        {
                            status = ELoginStatus.AccountDisabled,
                            message = "Account disabled. Please contact the customer support."
                        });
                    default:
                        return BadRequest(new
                        {
                            status = ELoginStatus.IncorrectCredential,
                            message = "Incorrect email or password."
                        });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = ELoginStatus.IncorrectCredential,
                    message = ex.Message
                });
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
                return BadRequest(new {
                    status = EEmailVerifyStatus.InternalServerError,
                    message = "Invalid link."
                });
            try
            {
                EEmailVerifyStatus status = await _conn.ActivateAccount(e, t);
                return status switch
                {
                    EEmailVerifyStatus.VerifySucceeded => Ok(new
                    {
                        status = EEmailVerifyStatus.VerifySucceeded,
                        message = "Account has been activated."
                    }),
                    EEmailVerifyStatus.VerifyFailed => BadRequest(new
                    {
                        status = EEmailVerifyStatus.VerifyFailed,
                        message = "Account has already been activated or link expired."
                    }),
                    _ => BadRequest(new
                    {
                        status = EEmailVerifyStatus.InternalServerError,
                        message = "Internal Server Error."
                    })
                };
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = EEmailVerifyStatus.InternalServerError,
                    message = ex.Message
                });
            }
        }


        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordInput changePasswordInput)
        {
            ClaimsIdentity identity = HttpContext.User.Identity as ClaimsIdentity;
            List<Claim> claims = identity.Claims.ToList();
            string email = claims[0].Value;
            changePasswordInput.Email = email;
            try
            {
                EChangePasswordStatus status = await _conn.ChangePassword(changePasswordInput);
                return status switch
                {
                    EChangePasswordStatus.ChangeSucceeded => Ok(new 
                    { 
                        status = EChangePasswordStatus.ChangeSucceeded,
                        message = "Password successfully changed."
                    }),
                    EChangePasswordStatus.ChangeFaild => BadRequest(new 
                    { 
                        status = EChangePasswordStatus.ChangeFaild,
                        message = "Old password does not match."
                    }),
                    _ => BadRequest(new 
                    { 
                        status = EChangePasswordStatus.InternalServerError,
                        message = "Internal Server Error."
                    })
                };
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = EChangePasswordStatus.InternalServerError,
                    message = "Internal Server Error."
                });
            }
        }


    }
}
