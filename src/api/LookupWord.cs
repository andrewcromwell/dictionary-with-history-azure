using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using System.Text.Json.Nodes;
using Newtonsoft.Json;
using System.Security.Principal;
using Google.Protobuf.Collections;
using LookupWord_Api.RequestVO;
using LookupWord_Api.ResponseVO;
using LookupWord_Api.Model;
using LookupWord_Api.Services.Abstractions;
using System.IdentityModel.Tokens.Jwt;

namespace LookupWord.Api
{
    public class LookupWord
    {
        private readonly ILogger _logger;
        private readonly ILookupService _lookupService;
        private readonly IDictionaryService _dictionary;


        public LookupWord(ILoggerFactory loggerFactory, ILookupService lookupService, IDictionaryService dictionary)
        {
            _logger = loggerFactory.CreateLogger<LookupWord>();
            _lookupService = lookupService;
            _dictionary = dictionary;
        }

        [Function("LookupWord")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequestData req)
        {
            try
            {
                LookupRequest lookupRequest = System.Text.Json.JsonSerializer.Deserialize<LookupRequest>(req.Body);

                var token = req.Headers.GetValues("Authorization").FirstOrDefault();
                token = token.Replace("Bearer ", "");
                var decoded = new JwtSecurityToken(token);
                var userId = decoded.Subject;
                WordLookup lookupInfo = await _lookupService.Lookup(lookupRequest, userId);

                Object def = await _dictionary.getDefinition(lookupRequest.word.Trim());
                var responseData = new LookupResponse
                {
                    word = lookupRequest.word.Trim(),
                    lookupInfo = lookupInfo,
                    definition = def
                };

                _logger.LogInformation("C# HTTP trigger function processed a request.");

                var response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/json; charset=utf-8");

                response.WriteString(System.Text.Json.JsonSerializer.Serialize(responseData));

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                return req.CreateResponse(HttpStatusCode.OK);
            }
        }
    }
}
