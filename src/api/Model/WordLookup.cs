using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LookupWord_Api.Model
{
    public class WordLookup
    {
        public string id { get; set; }
        public string userId { get; set; }
        public int numberOfLookups { get; set; }
        public List<Lookup> lookups { get; set; }
        public long createdAt { get; set; }
        public long lastUpdatedAt { get; set; }
    }
}
