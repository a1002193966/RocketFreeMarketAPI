﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DataAccessLayer.Infrastructure;
using DTO;
using Entities;
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


        public AccountsController(IAccountConnection conn, IEmailSender emailSender)
        {
            _conn = conn;
            _emailSender = emailSender;

        }



        // GetAccountInfo <AccountsController>/test@test.com
        [HttpGet("{email}")]
        public Account GetAccountInfo([FromRoute] string email)
        {
            try
            {
                Account account = _conn.GetAccountInfo(email);
                if (account.AccountID == 0)
                {
                    return null;
                }
                return account;
            }
            catch (Exception e)
            {
                throw;
            }
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
            catch (Exception e)
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
            
            try
            {
                if (e == null || t == null || _conn.ActivateAccount(e, t) == false)
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
