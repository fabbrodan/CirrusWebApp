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
        private string CosmosUserContainerName = "cirrus-user-container";
        private string CosmosUserFileContainerName = "cirrus-user-file-container";

        private CosmosClient CosmosDbClient;
        private Container CosmosUserContainer;
        private Container CosmosUserFileContainer;
        private PartitionKey UserPartitionKey = new PartitionKey(@"/user/id");
        private PartitionKey FilePartitionKey = new PartitionKey(@"/user/id");
        public CosmosDbService()
        {
            CosmosDbClient = new CosmosClient(CosmosURI, CosmosKey);
            CosmosUserContainer = CosmosDbClient.GetContainer(CosmosDB, CosmosUserContainerName);
            CosmosUserFileContainer = CosmosDbClient.GetContainer(CosmosDB, CosmosUserContainerName);
        }

        public async Task AddUser(Models.User User)
        {
            await CosmosUserContainer.CreateItemAsync(User);
        }

        public async Task<Models.User> GetUser(Models.User User)
        {
            var queryText = "SELECT * FROM c WHERE c.id = '" + User.id + "'";
            QueryDefinition queryDefinition = new (queryText);
            FeedIterator<Models.User> queryResultIterator = CosmosUserContainer.GetItemQueryIterator<Models.User>(queryDefinition);

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
            await CosmosUserFileContainer.CreateItemAsync(File, FilePartitionKey);
        }
    }
}
