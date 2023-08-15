using LookupWord_Api.Model;
using LookupWord_Api.RequestVO;
using LookupWord_Api.ResponseVO;
using LookupWord_Api.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LookupWord_Api.Services
{
    public class LookupService : ILookupService
    {
        CosmosClient _client;
        public LookupService(IConfiguration configuration)
        {
            string connectionString = 
                configuration[configuration["AZURE_COSMOS_CONNECTION_STRING_KEY"]];
            _client = new CosmosClient(
                connectionString: connectionString
            );
        }
        public async Task<WordLookup> Lookup(LookupRequest lookupRequest, string userId)
        {
            // New instance of CosmosClient class

            Container container = _client.GetContainer("WordLookups", "WordLookups");
            long currentUnixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            lookupRequest.word = lookupRequest.word.Trim();

            Lookup lookup = new Lookup
            {
                media = lookupRequest.media,
                lookupDate = currentUnixTimestamp
            };

            PartitionKey partitionKey = new PartitionKey(userId);

            try
            {
                WordLookup readItem = await container.ReadItemAsync<WordLookup>(
                    id: lookupRequest.word,
                    partitionKey: partitionKey
                );

                // update the entry to reflect the current lookup.
                readItem.lookups.Add(lookup);
                readItem.lastUpdatedAt = currentUnixTimestamp;
                readItem.numberOfLookups++;

                WordLookup updatedItem = await container.UpsertItemAsync<WordLookup>(
                    item: readItem,
                    partitionKey: partitionKey
                );

                return updatedItem;
            }
            catch (CosmosException ce)
            {
                if (ce.Message.Contains("NotFound"))
                {
                    // Lordy loo. We just need to add the entry. Cosmos freaks out
                    // when it doesn't find the item.
                    WordLookup newLookup = new WordLookup
                    {
                        id = lookupRequest.word,
                        userId = userId,
                        numberOfLookups = 1,
                        createdAt = currentUnixTimestamp,
                        lastUpdatedAt = currentUnixTimestamp,
                        lookups = new List<Lookup> { lookup }
                    };

                    WordLookup createdItem = await container.CreateItemAsync<WordLookup>(
                        item: newLookup,
                        partitionKey: partitionKey
                    );


                    return createdItem;
                }
                else
                {
                    // if it's a different exception, I'm not handling it.
                    throw ce;
                }
            }
        }
    }
}
