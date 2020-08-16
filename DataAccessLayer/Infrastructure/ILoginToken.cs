using System;
using System.Collections.Generic;
using System.Text;

using DTO;

namespace DataAccessLayer.Infrastructure
{
    public interface ILoginToken
    {
        string GenerateToken(LoginInput loginInput);
    }
}
