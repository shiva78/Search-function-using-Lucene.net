using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Search_function.Models
{
    public class ListofResult : IEquatable<ListofResult>
    {
        public string FileName { get; set; }
        public string URL { get; set; }
        public string Content { get; set; }
        public override string ToString()
        {
            return FileName + Content + URL;
        }
        public bool Equals(ListofResult other)
        {
            if (other == null) return false;
            return (this.FileName.Equals(other.FileName));
        }
    }
}