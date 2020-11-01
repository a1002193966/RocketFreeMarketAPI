using DTO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLogicLayer.Infrastructure
{
    public interface IAccountValidation
    {
        Task<EStatus> RegisterValidation(RegisterInput registerInput);
        Task<ELoginStatus> LoginValidation(LoginInput loginInput);
        Task<EStatus> ActivateAccountValidation(string e, string t);
    }
}
