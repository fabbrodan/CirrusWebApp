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
        public CosmosDbService()
        {
            CosmosDbClient = new CosmosClient(CosmosURI, CosmosKey);
            CosmosUserContainer = CosmosDbClient.GetContainer(CosmosDB, CosmosUserContainerName);
            CosmosUserFileContainer = CosmosDbClient.GetContainer(CosmosDB, CosmosUserFileContainerName);
        }

        public async Task AddUser(Models.User User)
        {
            await CosmosUserContainer.CreateItemAsync(User, new PartitionKey(User.id));
        }

        public async Task<Models.User> GetUser(string UserId)
        {
            var queryText = "SELECT * FROM c WHERE c.id = '" + UserId + "'";
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

        public async Task<List<File>> GetFiles(string UserId)
        {
            var queryText = "SELECT * FROM c WHERE c.userid = '" + UserId + "'";
            QueryDefinition queryDefinition = new QueryDefinition(queryText);
            FeedIterator<File> feedIterator = CosmosUserFileContainer.GetItemQueryIterator<File>(queryDefinition);

            List<File> fileResult = new();
            while (feedIterator.HasMoreResults)
            {
                var result = await feedIterator.ReadNextAsync();
                if (result.Count > 0)
                {
                    foreach(File file in result)
                    {
                        fileResult.Add(file);
                    }
                }
                else
                {
                    continue;
                }
                
            }

            return fileResult.Count > 0 ? fileResult : null;
        }
        public async Task AddFile(File File)
        {
            var queryText = "SELECT * FROM c WHERE c.userid = '" + File.UserId + "' AND c.FileName = '" + File.FileName + "'";
            QueryDefinition queryDefinition = new QueryDefinition(queryText);
            FeedIterator<File> feedIterator = CosmosUserFileContainer.GetItemQueryIterator<File>(queryDefinition);
            if (feedIterator.HasMoreResults)
            {
                var result = await feedIterator.ReadNextAsync();
                if (result.Count > 0)
                {
                    File updateFile = result.SingleOrDefault();
                    updateFile.LastModified = File.LastModified;
                    await CosmosUserFileContainer.UpsertItemAsync(updateFile);
                }
            }
            else
            {
                await CosmosUserFileContainer.CreateItemAsync(File, new PartitionKey(File.UserId));
            }
        }

        public async Task DeleteFile(File File)
        {
            await CosmosUserFileContainer.DeleteItemAsync<File>(File.id, new PartitionKey(File.UserId));
        }
    }
}
