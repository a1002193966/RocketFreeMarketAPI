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
    }
}
