using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LookupWord_Api.Model
{
    public class Definition
    {
        public string partOfSpeech { get; set; }
        public List<string> meanings { get; set; }
    }
}
