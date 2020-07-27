using System;

namespace Entities
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
