using DTO;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.Infrastructure
{
    public interface IUserConnection
    {
        bool UpdateProfile(ProfileDTO profile);
    }
}
