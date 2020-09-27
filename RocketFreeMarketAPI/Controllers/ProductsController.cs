﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataAccessLayer.Infrastructure;
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace RocketFreeMarketAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductPostConnection _conn;
        public ProductsController(IProductPostConnection conn)
        {
            _conn = conn;
        }

        // POST <ProductsController>
        [Authorize]
        [HttpPost("NewProductPost")]
        public async Task<IActionResult> NewProductPost([FromBody]ProductPost productPost)
        {
            ClaimsIdentity identity = HttpContext.User.Identity as ClaimsIdentity;
            List<Claim> claims = identity.Claims.ToList();
            string email = claims[0].Value.ToUpper();
            try
            {
                EStatus status = await _conn.NewProductPost(productPost, email);
                switch (status)
                {
                    case EStatus.Succeeded:
                        return Ok(new
                        {
                            status = EStatus.Succeeded,
                            message = "Successfully Posted."
                        });
                    case EStatus.Failed:
                        return BadRequest(new
                        {
                            status = EStatus.Failed,
                            message = "Something went wrong."
                        });
                    default:
                        return BadRequest(new
                        {
                            status = EStatus.DatabaseError,
                            message = "Internal Server Error."
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

    }
}