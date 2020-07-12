using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RocketFreeMarketAPI.Models
{
    public class Secret
    {
        public byte[] PasswordHash { get; set; }
        public byte[] Key { get; set; }
        public byte[] IV { get; set; }
    }
}
