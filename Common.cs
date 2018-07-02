using System;
using System.Threading;
using Microsoft.Azure.ServiceBus.Management;

namespace AzurePubSubServerlessCSharp
{
    public static class Common
    {
        public const string StorageConnectionString = "StorageConnectionString";
        public const string ServiceBusConnectionString = "ServiceBusConnectionString";

        public static string GetEnvironmentVariable(string name)
        {
            return Environment.GetEnvironmentVariable(name, EnvironmentVariableTarget.Process);
        }

        public static string GetServiceBusConnectionString()
        {
            return Common.GetEnvironmentVariable(Common.ServiceBusConnectionString);
        }

        public static string GetStorageConnectionString()
        {
            return Common.GetEnvironmentVariable(Common.ServiceBusConnectionString);
        }

        private static ManagementClient GetServiceBusClient()
        {
            return new ManagementClient(Common.GetEnvironmentVariable(Common.ServiceBusConnectionString));
        }

        public static Subscriber CreateSubscriberQueue(string id)
        {
            var client = GetServiceBusClient();
            client.CreateQueueAsync(id, CancellationToken.None);
            return new Subscriber
            {
                ConnectionString = Common.GetServiceBusConnectionString(),
                QueueName = id
            };
        }
    }
}
