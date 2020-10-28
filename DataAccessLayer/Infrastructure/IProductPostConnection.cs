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
        Task<MyPost> GetPostNoAuth(int postID);
        Task<List<MyPost>> GetMyListing(string email);
        Task<List<MyPost>> GetListing();
        Task<EStatus> UpdatePost(ProductPost productPost, string email, int postId);
        Task<EStatus> DeletePost(string email, int postId);
        Task<EStatus> NewComment(MyComment comment, string email);
        Task<List<CommentDTO>> GetCommentList(int postId);
    }
}
