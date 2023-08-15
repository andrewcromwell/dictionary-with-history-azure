using LookupWord_Api.Model;
using LookupWord_Api.RequestVO;
using LookupWord_Api.ResponseVO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LookupWord_Api.Services.Abstractions
{
    public interface ILookupService
    {
        Task<WordLookup> Lookup(LookupRequest lookupRequest, string userId);
    }
}
