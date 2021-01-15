using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CirrusWebApp.Data.Models
{
    public class User
    {
        [JsonProperty(PropertyName = "id")]
        public string id { get; set; }
        public string Firstname { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string PasswordSalt { get; set; }
        public string Password { get; set; }
        public DateTime RegisteredDateTime { get; set; }
    }
}
