using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using CirrusWebApp.Data.Models;

namespace CirrusWebApp.Data.Services
{
    public class CosmosDbService
    {
        private string CosmosURI = "https://cirrus-csp.documents.azure.com:443/";
        private string CosmosKey = "VV4xsp3QkDSnqaDllWIpblTo9uJjybNkDHlAzqGhDwA6YUC4TdOHsCQuPkwgU6shIQl4Lfu6s7N844WUQXVEVw=="; /*Environment.GetEnvironmentVariable("CosmosKey");*/
        private string CosmosDB = "cirrus-db";
        private string CosmosContainerName = "cirrus-container";

        private CosmosClient CosmosDbClient;
        private Container CosmosContainer;
        private PartitionKey UserPartitionKey = new PartitionKey(@"/user/email");
        private PartitionKey FilePartitionKey = new PartitionKey(@"/files/id");
        public CosmosDbService()
        {
            CosmosDbClient = new CosmosClient(CosmosURI, CosmosKey);
            CosmosContainer = CosmosDbClient.GetContainer(CosmosDB, CosmosContainerName);
        }

        public async Task AddUser(Models.User User)
        {
            await CosmosContainer.CreateItemAsync(User, UserPartitionKey);
        }

        public async Task<Models.User> GetUser(Models.User User)
        {
            var queryText = "SELECT * FROM c WHERE c.id = '" + User.Email + "'";
            QueryDefinition queryDefinition = new (queryText);
            FeedIterator<Models.User> queryResultIterator = CosmosContainer.GetItemQueryIterator<Models.User>(queryDefinition);

            List<Models.User> userResult = new();
            while(queryResultIterator.HasMoreResults)
            {
                FeedResponse<Models.User> result = await queryResultIterator.ReadNextAsync();
                return result.FirstOrDefault();
            }

            return null;
        }

        public async Task AddFile(File File)
        {
            await CosmosContainer.CreateItemAsync(File, FilePartitionKey);
        }
    }
}
