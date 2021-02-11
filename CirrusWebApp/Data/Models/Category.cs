using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CirrusWebApp.Data.Models
{
    public class Category : IEquatable<Category>
    {
        [JsonProperty(PropertyName = "userid")]
        public string UserId { get; set; }
        public string id { get; set; }
        public string CategoryName { get; set; }

        public Category() { }
        public Category(string CategoryName, string UserId)
        {
            this.UserId = UserId;
            this.CategoryName = CategoryName;
        }

        public bool Equals(Category other)
        {
            if (this.CategoryName == other.CategoryName)
            {
                return true;
            }
            return false;
        }
    }
}
