using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
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
    public class UsersController : ControllerBase
    {
        private readonly IUserConnection _conn;

        public UsersController(IUserConnection conn)
        {
            _conn = conn;
        }

        //GET <UserController>/
        //With Token
        [Authorize]
        [HttpGet]
        public async Task<User> GetProfile()
        {
            ClaimsIdentity identity = HttpContext.User.Identity as ClaimsIdentity;
            List<Claim> claims = identity.Claims.ToList();
            string email = claims[0].Value.ToUpper();
            try
            {
                return await _conn.GetProfile(email);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // PUT <UsersController>
        [Authorize]
        [HttpPut("UpdateProfile")]
        public async Task<bool> UpdateProfile([FromBody]ProfileDTO profile)
        {
            try
            {
                return await _conn.UpdateProfile(profile);
            }
            catch(Exception ex)
            {
                throw;
            }
        }

    }
}
