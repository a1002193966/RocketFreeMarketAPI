using System;

namespace Entities
{
    public class Access
    {
        public int AccountID { get; set; }
        public byte[] AesKey { get; set; }
    }
}
