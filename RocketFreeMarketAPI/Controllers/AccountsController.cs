using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RocketFreeMarketAPI.Infrastracture;
using RocketFreeMarketAPI.Models;

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



        // GET: api/<AccountsController>
        [HttpGet]
        public async Task<List<Account>> Get()
        {
            List<Account> AccountList = await _conn.ExcuteCommand("SELECT * FROM Account");
            return AccountList;   
        }

        // GET api/<AccountsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<AccountsController>
        [HttpPost]
        public bool Post([FromBody] Account acc)
        {
            return _conn.Register(acc.Email, acc.PasswordHash, acc.PhoneNumber);
        }

        // PUT api/<AccountsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<AccountsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
