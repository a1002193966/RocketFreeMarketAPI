using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.Infrastructure
{
    public interface IEmailSender
    {
        void ExecuteSender(string email);
    }
}
