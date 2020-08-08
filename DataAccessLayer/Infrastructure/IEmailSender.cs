using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.Infrastructure
{
    public interface IEmailSender
    {
        bool ExecuteSender(string email);
    }
}
