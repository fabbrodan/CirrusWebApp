using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace CirrusWebApp.Data.Services
{
    public class CosmosDbService
    {
        private string CosmosURI = "https://cirrus-csp.documents.azure.com:443/";
        private string CosmosKey = Environment.GetEnvironmentVariable("CosmosKey");
        private string CosmosDB = "cirrus-db";
        private string CosmosContainerName = "cirrus-container";

        private CosmosClient CosmosDbClient;
        public Container CosmosContainer;
        public CosmosDbService()
        {
            CosmosDbClient = new CosmosClient(CosmosURI, CosmosKey);
            CosmosContainer = CosmosDbClient.GetContainer(CosmosDB, CosmosContainerName);
        }
    }
}
