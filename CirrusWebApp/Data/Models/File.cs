using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CirrusWebApp.Data.Models
{
    public class File
    {
        public string id { get; set; }
        public string FileName { get; set; }
        public string FileTitle { get; set; }
        public List<string> Categories { get; set; } = new ();
    }
}
