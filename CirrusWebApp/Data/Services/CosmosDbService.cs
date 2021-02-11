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
        private string CosmosUserCategoryContainerName = "cirrus-user-categories";

        private CosmosClient CosmosDbClient;
        private Container CosmosUserContainer;
        private Container CosmosUserFileContainer;
        private Container CosmosUserCategoryContainer;
        public CosmosDbService()
        {
            CosmosDbClient = new CosmosClient(CosmosURI, CosmosKey);
            CosmosUserContainer = CosmosDbClient.GetContainer(CosmosDB, CosmosUserContainerName);
            CosmosUserFileContainer = CosmosDbClient.GetContainer(CosmosDB, CosmosUserFileContainerName);
            CosmosUserCategoryContainer = CosmosDbClient.GetContainer(CosmosDB, CosmosUserCategoryContainerName);
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

        public async Task AddCategory(string UserId, Category Category)
        {
            await CosmosUserCategoryContainer.CreateItemAsync(Category, new PartitionKey(UserId));
        }

        public async Task<List<Category>> GetCategories(string UserId)
        {
            var queryText = "SELECT * FROM c WHERE c.userid = '" + UserId + "'";
            QueryDefinition queryDefinition = new(queryText);
            FeedIterator<Category> queryResultIterator = CosmosUserCategoryContainer.GetItemQueryIterator<Category>(queryDefinition);

            List<Category> returnList = new();

            while(queryResultIterator.HasMoreResults)
            {
                FeedResponse<Category> result = await queryResultIterator.ReadNextAsync();
                if (result.Count > 0)
                {
                    foreach (Category category in result)
                    {
                        returnList.Add(category);
                    }
                }
            }

            return returnList;
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

            return fileResult;
        }

        public async Task<List<File>> GetFiles(string UserId, string Category)
        {
            var queryText = "SELECT * FROM c WHERE c.userid = '" + UserId + "' AND c.Categories[0].CategoryName = '" + Category + "'";
            QueryDefinition queryDefinition = new QueryDefinition(queryText);
            FeedIterator<File> feedIterator = CosmosUserFileContainer.GetItemQueryIterator<File>(queryDefinition);

            List<File> fileResult = new();
            while (feedIterator.HasMoreResults)
            {
                var result = await feedIterator.ReadNextAsync();
                if(result.Count > 0)
                {
                    foreach (File file in result)
                    {
                        fileResult.Add(file);
                    }
                }
            }

            return fileResult;
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
                else
                {
                    await CosmosUserFileContainer.CreateItemAsync(File, new PartitionKey(File.UserId));
                }
            }
        }

        public async Task DeleteFile(File File)
        {
            await CosmosUserFileContainer.DeleteItemAsync<File>(File.id, new PartitionKey(File.UserId));
        }

        public async Task<List<dynamic>> GetUserData(string UserId)
        {
            List<dynamic> responseList = new();
            var FileAndCategoryQueryText = "SELECT * FROM c WHERE c.userid = '" + UserId + "'";
            var UserQueryText = "SELECT * FROM c WHERE c.id = '" + UserId + "'";

            QueryDefinition FileAndCategoryQuery = new QueryDefinition(FileAndCategoryQueryText);
            QueryDefinition UserQuery = new QueryDefinition(UserQueryText);

            FeedIterator<Models.User> UserIterator = CosmosUserContainer.GetItemQueryIterator<Models.User>(UserQuery);
            FeedIterator<Category> CategoryIterator = CosmosUserCategoryContainer.GetItemQueryIterator<Category>(FileAndCategoryQuery);
            FeedIterator<File> FileIterator = CosmosUserFileContainer.GetItemQueryIterator<File>(FileAndCategoryQuery);

            if (UserIterator.HasMoreResults)
            {
                var userResult = await UserIterator.ReadNextAsync();
                responseList.Add(userResult.FirstOrDefault());
            }

            if (CategoryIterator.HasMoreResults)
            {
                var categoryResult = await CategoryIterator.ReadNextAsync();
                if(categoryResult.Count() > 0)
                {
                    foreach (Category category in categoryResult)
                    {
                        responseList.Add(category);
                    }
                }
            }

            if (FileIterator.HasMoreResults)
            {
                var fileResult = await FileIterator.ReadNextAsync();
                if(fileResult.Count() > 0)
                {
                    foreach (File file in fileResult)
                    {
                        responseList.Add(file);
                    }
                }
            }

            return responseList;
        }

        public async Task DeleteFullUserData(string UserId)
        {
            var files = await GetFiles(UserId);
            foreach (File file in files)
            {
                await CosmosUserFileContainer.DeleteItemAsync<File>(file.id, new PartitionKey(UserId));
            }

            var categories = await GetCategories(UserId);
            foreach (Category category in categories)
            {
                await CosmosUserCategoryContainer.DeleteItemAsync<Category>(category.id, new PartitionKey(UserId));
            }

            var user = await GetUser(UserId);
            await CosmosUserContainer.DeleteItemAsync<Models.User>(user.id, new PartitionKey(UserId));
        }
    }
}
