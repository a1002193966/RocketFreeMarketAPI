using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer.Infrastructure;
using DTO;
using Entities;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;


namespace RocketFreeMarketAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [EnableCors("CorsPolicy")]
    public class UsersController : ControllerBase
    {
        private readonly IUserConnection _conn;

        public UsersController(IUserConnection conn)
        {
            _conn = conn;
        }

        //GET <UserController>/{email}
        [HttpGet("{email}")]
        public User GetProfile([FromRoute]string email)
        {
            try
            {
                return _conn.GetProfile(email);
            }
            catch (Exception)
            {
                throw;
            }
        }


        // PUT <UsersController>
        [HttpPut("UpdateProfile")]
        public bool UpdateProfile([FromBody]ProfileDTO profile)
        {
            try
            {
                return _conn.UpdateProfile(profile);
            }
            catch(Exception e)
            {
                throw;
            }
        }

    }
}
