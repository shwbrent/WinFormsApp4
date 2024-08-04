using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oven.Entity
{
    public class User
    {
        public string Username { get; set; }
        public string Password { get; set; } // 雜湊後的密碼
        public UserLevel Level { get; set; }
    }

    public enum UserLevel
    {
        Operator = 1,
        Engineer = 2,
        Admin = 3,
        SuperAdmin = 4
    }
}
