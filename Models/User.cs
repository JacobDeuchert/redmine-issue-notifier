using System;

namespace redmine_notifier.Models
{
    public class User
    {
        public long id { get; set; }
        public string login { get; set; }
        public bool admin { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string mail { get; set; }
        public string created_on { get; set; }
        public string updated_on { get; set; }
        public string last_login_on { get; set; }
        public string api_key { get; set; }        
    }

    public class UserResponse
    { 
        public User user { get; set; }
    }
}