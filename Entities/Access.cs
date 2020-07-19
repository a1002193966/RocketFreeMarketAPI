using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class Access
    {
        public int AccountID { get; set; }
        public byte[] AesKey { get; set; }
    }
}
