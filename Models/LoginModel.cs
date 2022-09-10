using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class LoginModel
    {
        public string Email { get;  set; }
        public string Password { get;  set; }
        public bool RememberMe { get;  set; } = false;
        public string ReturnUrl { get;  set; } = "/";
    }
}
