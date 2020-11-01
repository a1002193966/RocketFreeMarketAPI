using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using BusinessLogicLayer.Infrastructure;
using DataAccessLayer.Infrastructure;
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace RocketFreeMarketAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class AccountsController : ControllerBase
    {
        private readonly IAccountConnection _conn;
        private readonly IAccountValidation _accVal;
        private readonly IEmailSender _emailSender;
        private readonly ILoginToken _loginToken;

        public AccountsController(IAccountValidation accVal, IAccountConnection conn, IEmailSender emailSender, ILoginToken loginToken)
        {
            _conn = conn;
            _accVal = accVal;
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
        public async Task<IActionResult> Register([FromBody]RegisterInput registerInput)
        {
            try
            {
                EStatus status = await _accVal.RegisterValidation(registerInput);
                switch (status)
                {
                    case EStatus.Succeeded:
                        await _emailSender.ExecuteSender(registerInput.Email, "Email");
                        return Ok(new
                        {
                            status = EStatus.Succeeded,
                            message = "Successfully registerd."
                        });
                    case EStatus.EmailExists:
                        return BadRequest(new
                        {
                            status = EStatus.EmailExists,
                            message = "Account already exists."
                        });
                    case EStatus.ReCaptchaFailed:
                        return BadRequest(new
                        {
                            status = EStatus.ReCaptchaFailed,
                            message = "ReCaptcha not verified."
                        });
                    default:
                        return BadRequest(new
                        {
                            status = EStatus.DatabaseError,
                            message = "Internal server error."
                        });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = EStatus.DatabaseError,
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
        public async Task<IActionResult> Login([FromBody]LoginInput loginInput)
        {
            try
            {
                ELoginStatus status = await _accVal.LoginValidation(loginInput);
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
                    case ELoginStatus.ReCaptchaFailed:
                        return BadRequest(new
                        {
                            status = ELoginStatus.ReCaptchaFailed,
                            message = "ReCaptcha not verified."
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
            try
            {
                EStatus status = await _accVal.ActivateAccountValidation(e, t);
                return status switch
                {
                    EStatus.Succeeded => Ok(new
                    {
                        status = EStatus.Succeeded,
                        message = "Account has been activated."
                    }),
                    EStatus.Failed => BadRequest(new
                    {
                        status = EStatus.Failed,
                        message = "Account has already been activated or link expired."
                    }),
                    EStatus.InvalidLink => BadRequest(new
                    {
                        status = EStatus.InvalidLink,
                        message = "Invalid link."
                    }),
                    _ => BadRequest(new
                    {
                        status = EStatus.DatabaseError,
                        message = "Internal Server Error."
                    })
                };
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = EStatus.DatabaseError,
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
                EStatus status = await _conn.ChangePassword(changePasswordInput);
                return status switch
                {
                    EStatus.Succeeded => Ok(new 
                    { 
                        status = EStatus.Succeeded,
                        message = "Password successfully changed."
                    }),
                    EStatus.Failed => BadRequest(new 
                    { 
                        status = EStatus.Failed,
                        message = "Old password does not match."
                    }),
                    _ => BadRequest(new 
                    { 
                        status = EStatus.DatabaseError,
                        message = "Internal Server Error."
                    })
                };
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    status = EStatus.DatabaseError,
                    message = ex.Message
                });
            }
        }
        

        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody]ResetPasswordInput resetInput)
        {
            if (resetInput.EncryptedEmail == null || resetInput.Password == null || resetInput.Token == null)
                return BadRequest(new
                {
                    status = EStatus.InvalidLink,
                    message = "Invalid Link"
                });
            try
            {
                EStatus status = await _conn.ResetPassword(resetInput);
                return status switch
                {
                    EStatus.Succeeded => Ok(new
                    {
                        status = EStatus.Succeeded,
                        message = "Password has been successfully reset."
                    }),
                    EStatus.Failed => BadRequest(new
                    {
                        status = EStatus.Failed,
                        message = "Password reset failed."
                    }),
                    _ => BadRequest(new
                    {
                        status = EStatus.DatabaseError,
                        message = "Password reset failed."
                    })
                };
            }
            catch(Exception ex)
            {
                return BadRequest(new
                {
                    status = EStatus.DatabaseError,
                    message = ex.Message
                });
            } 
        }


        [HttpPost("ResetPasswordConfirmation")]
        public async Task<IActionResult> ResetPasswordConfirmation([FromBody]JsonElement e)
        {
            try
            { 
                dynamic data = JsonConvert.DeserializeObject(e.GetRawText()) ;
                await _conn.SendResetLink(data.Email.ToString());
                return Ok();
            }
            catch (Exception ex) 
            {
                return Ok();
            }
        }

    }
}
