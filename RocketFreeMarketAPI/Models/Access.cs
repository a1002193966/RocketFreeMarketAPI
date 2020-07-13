using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RocketFreeMarketAPI.Models
{
    public class Access
    {
        public int AccountID { get; set; }
        public byte[] AesKey { get; set; }
    }
}
