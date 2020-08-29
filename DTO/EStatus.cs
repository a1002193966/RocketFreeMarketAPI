using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public enum EEmailRegister
    {
        RegistarEmailSuccess = 1,
        EmailExists = -1,
        InternalServerError = 0
    }

    public enum EAccountStatus
    {
        LoginSuccess = 1,
        EmailNotActivated = 0,
        AccountLocked = -1,
        AccountDisabled = -7,
        WrongLoginInfo = -9
    }

    public enum EActivateAccount
    {
        ActivatedAccount = 1,
        ActivateAccountFailed = 0,
        InternalServerError = -1
    }

}
