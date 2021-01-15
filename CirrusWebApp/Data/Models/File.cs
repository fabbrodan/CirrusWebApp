using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CirrusWebApp.Data.Models
{
    public class File
    {
        [JsonProperty(PropertyName="userid")]
        public string UserId { get; set; }
        public string id { get; set; }
        public string FileName { get; set; }
        public string FileTitle { get; set; }
        public string FileExtension { get; set; }
        public string PrimaryFilePath { get; set; }
        public DateTime LastModified { get; set; }
        public List<string> Categories { get; set; } = new ();
    }
}
