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


        // GET <AccountsController>/test@test.com
        [HttpGet("{email}")]
        public Account Get([FromRoute] string email)
        {
            return _conn.GetAccountInfo(email);
        }


        // POST <AccountsController>
        [HttpPost]
        public bool Post([FromBody] RegisterInput registerInput)
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
