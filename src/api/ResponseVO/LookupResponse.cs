using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LookupWord_Api.Model;

namespace LookupWord_Api.ResponseVO
{
    public class LookupResponse
    {
        public string word { get; set; }
        public WordLookup lookupInfo { get; set; }
        public Object definition { get; set; }
    }
}
