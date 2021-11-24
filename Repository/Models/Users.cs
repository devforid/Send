using System;
using System.Collections.Generic;
using System.Text;

namespace Repository.Models
{
    public class Users
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool? isLoggedin { get; set; }
    }
}
