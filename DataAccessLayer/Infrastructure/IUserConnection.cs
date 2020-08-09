using DTO;
using Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Infrastructure
{
    public interface IUserConnection
    {
        Task<bool> UpdateProfile(ProfileDTO profile);
        Task<User> GetProfile(string email);
    }
}
