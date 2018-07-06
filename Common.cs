using System;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.ServiceBus.Management;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace AzurePubSubServerlessCSharp
{
    public static class Common
    {
        public const string StorageConnectionString = "StorageConnectionString";
        public const string ServiceBusConnectionString = "ServiceBusConnectionString";

        public const string ContentsTableName = "content";
        public const string TopicsTableName = "topics";
        public const string FunctionsTableName = "functions";

        public static string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

        public static string GetServiceBusConnectionString()
        {
            return GetEnvironmentVariable(ServiceBusConnectionString);
        }

        public static string GetStorageConnectionString()
        {
            return GetEnvironmentVariable(StorageConnectionString);
        }

        private static ManagementClient GetServiceBusClient()
        {
            return new ManagementClient(GetEnvironmentVariable(ServiceBusConnectionString));
        }

        public static T GetPostObject<T>(HttpRequest request)
        {
            var requestBody = new StreamReader(request.Body).ReadToEnd();
            return JsonConvert.DeserializeObject<T>(requestBody);
        }

        public static Subscriber CreateSubscriberQueue(string id)
        {
            var client = GetServiceBusClient();
            client.CreateQueueAsync(id, CancellationToken.None);
            return new Subscriber
            {
                ConnectionString = GetServiceBusConnectionString(),
                QueueName = id
            };
        }

        public static Subscriber GetSubscriberResponse(string id)
        {
            return new Subscriber
            {
                ConnectionString = GetServiceBusConnectionString(),
                QueueName = id
            };
        }

        public static CloudTable GetAzureTable(string tableName)
        {
            var connectionString = GetStorageConnectionString();
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var tableClient = storageAccount.CreateCloudTableClient();
            return tableClient.GetTableReference(tableName);
        }
    }
}
