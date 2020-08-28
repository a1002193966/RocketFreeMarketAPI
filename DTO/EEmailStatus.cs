using System;
using System.Collections.Generic;
using System.Text;

namespace DTO
{
    public enum EEmailStatus
    {
        RegistarEmailSuccess = 1,
        EmailExists = -1,
        InternalServerError = 0
    }
}
