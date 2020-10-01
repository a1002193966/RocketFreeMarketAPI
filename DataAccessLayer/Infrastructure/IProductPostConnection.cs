using DTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Infrastructure
{
    public interface IProductPostConnection
    {
        Task<EStatus> NewProductPost(ProductPost productPost, string email);
        Task<MyPost> GetPost(string email, int postID);
        Task<List<MyPost>> GetMyListing(string email);
        Task<EStatus> UpdatePost(ProductPost productPost, string email, int postId);
        Task<EStatus> DeletePost(string email, int postId);
    }
}
