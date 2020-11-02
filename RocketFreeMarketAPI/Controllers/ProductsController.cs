using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using DataAccessLayer.Infrastructure;
using DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


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


        // GET <ProductsController>/GetPost/{postId}
        [Authorize]
        [HttpGet("GetPost/{postId}")]
        public async Task<MyPost> GetPost([FromRoute]int postId)
        {
            string email = getEmailFromToken();
            try
            {
                return await _conn.GetPost(email, postId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        // GET <ProductsController>/GetPostNoAuth/{postId}
        [Authorize]
        [HttpGet("GetPostNoAuth/{postId}")]
        public async Task<MyPost> GetPostNoAuth([FromRoute]int postId)
        {
            string email = getEmailFromToken();
            try
            {
                return await _conn.GetPostNoAuth(postId);
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        //GET<ProductsController>/GetMyListing
        [Authorize]
        [HttpGet("GetMyListing")]
        public async Task<List<MyPost>> GetMyListing()
        {
            string email = getEmailFromToken();
            try
            {
                return await _conn.GetMyListing(email);
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        // GET <ProductsController>/GetListing
        [HttpGet("GetListing")]
        public async Task<List<MyPost>> GetListing()
        {
            try
            {
                return await _conn.GetListing();
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        // POST <ProductsController>/NewProductPost
        [Authorize]
        [HttpPost("NewProductPost")]
        public async Task<IActionResult> NewProductPost([FromBody]ProductPost productPost)
        {
            string email = getEmailFromToken();
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


        // PUT <ProductsController>/UpdatePost/{postId}
        [Authorize]
        [HttpPut("UpdatePost/{postId}")]
        public async Task<IActionResult> UpdatePost([FromBody]ProductPost productPost, [FromRoute]int postId)
        {
            string email = getEmailFromToken();
            try
            {
                EStatus status = await _conn.UpdatePost(productPost, email, postId);
                switch (status)
                {
                    case EStatus.Succeeded:
                        return Ok(new
                        {
                            status = EStatus.Succeeded,
                            message = "Successfully Updated."
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


        // DELETE <ProductsController>/DeletePost/{postId}
        [Authorize]
        [HttpDelete("DeletePost/{postId}")]
        public async Task<IActionResult> DeletePost([FromRoute]int postId)
        {
            string email = getEmailFromToken();
            try
            {
                EStatus status = await _conn.DeletePost(email, postId);
                return Ok(status);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }


        [Authorize]
        [HttpPost("NewComment")]
        public async Task<IActionResult> NewComment([FromBody]MyComment comment)
        {
            string email = getEmailFromToken();
            try
            {
                EStatus status = await _conn.NewComment(comment, email);
                switch (status)
                {
                    case EStatus.Succeeded:
                        return Ok(new 
                        {
                            status = EStatus.Succeeded,
                            message = "Comment posted"
                        });
                    default:
                        return BadRequest(new
                        {
                            status = EStatus.Failed,
                            message = "Failed"
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


        [Authorize]
        [HttpGet("GetCommentList/{postId}")]
        public async Task<List<CommentDTO>> GetCommentList([FromRoute]int postId)
        {
            try
            {
                List<CommentDTO> comments = await _conn.GetCommentList(postId);
                return comments;
            }
            catch (Exception ex)
            {
                return null;
            }
        }



        #region Private Help Function

        [Authorize]
        private string getEmailFromToken()
        {
            ClaimsIdentity identity = HttpContext.User.Identity as ClaimsIdentity;
            List<Claim> claims = identity.Claims.ToList();
            string email = claims[0].Value.ToUpper();
            return email;
        }

        #endregion
    }
}
