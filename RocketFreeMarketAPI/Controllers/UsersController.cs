using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataAccessLayer.Infrastructure;
using DTO;
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
