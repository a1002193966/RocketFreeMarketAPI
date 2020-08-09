using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DataAccessLayer.Infrastructure
{
    public interface IEmailSender
    {
        Task<bool> ExecuteSender(string email);
    }
}
