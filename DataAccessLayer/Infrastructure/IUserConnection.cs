using DTO;
using Entities;
using System.Threading.Tasks;

namespace DataAccessLayer.Infrastructure
{
    public interface IUserConnection
    {
        Task<bool> UpdateProfile(ProfileDTO profile);
        Task<User> GetProfile(string email);
    }
}
