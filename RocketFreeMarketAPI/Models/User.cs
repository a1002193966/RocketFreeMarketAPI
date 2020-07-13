using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace RocketFreeMarketAPI.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DOB { get; set; }
        public int AccountID { get; set; }
        public int UpdateID { get; set; }
        public DateTime UpdateDate { get; set; }
    }
}
