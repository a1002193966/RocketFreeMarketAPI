﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Entities;
using Microsoft.AspNetCore.Mvc;
using RocketFreeMarketAPI.Infrastracture;

namespace RocketFreeMarketAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IDBConnection _conn;
        public AccountsController(IDBConnection conn)
        {
            _conn = conn;
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
            return _conn.Register(registerInput);
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
