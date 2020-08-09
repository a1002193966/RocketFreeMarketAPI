using DTO;
using Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.Infrastructure
{
    public interface IUserConnection
    {
        bool UpdateProfile(ProfileDTO profile);
        User GetProfile(string email);
    }
}
