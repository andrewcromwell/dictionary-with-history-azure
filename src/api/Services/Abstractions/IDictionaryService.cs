using LookupWord_Api.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LookupWord_Api.Services.Abstractions
{
    public interface IDictionaryService
    {
        Task<Object> getDefinition(string word);
    }
}
