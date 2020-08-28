using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public enum EAccountStatus
    {
        LoginSuccess = 1,
        EmailNotActivated = 0,
        AccountLocked = -1,
        AccountDisabled = -7,
        WrongLoginInfo = -9
           
    }
}
