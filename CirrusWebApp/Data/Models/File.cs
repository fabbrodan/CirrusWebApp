using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CirrusWebApp.Data.Models
{
    [Serializable]
    public class File : IComparable, IEquatable<File>
    {
        [JsonProperty(PropertyName = "userid")]
        public string UserId { get; set; }
        public string id { get; set; }
        public string FileName { get; set; }
        public string FileTitle { get; set; }
        public string FileExtension { get; set; }
        public string PrimaryFilePath { get; set; }
        public DateTime LastModified { get; set; }
        public List<Category> Categories { get; set; } = new();

        public int CompareTo(object obj)
        {
            if (obj is File f)
            {
                if (this.FileName == f.FileName && this.FileExtension == f.FileExtension)
                {
                    return 1;
                }
                return 0;
            }
            return -1;
        }

        public bool Equals(File other)
        {
            if (this.FileName == other.FileName && this.FileExtension == other.FileExtension)
            {
                return true;
            }
            return false;
        }
    }
}
