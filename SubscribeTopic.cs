using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace AzurePubSubServerlessCSharp
{
    public class SubscribeTopicInput
    {
        [JsonProperty("subscriberId")]
        public string SubscriberId { get; set; }

        [JsonProperty("topics")]
        public List<string> Topics { get; set; }
    }

    internal class TopicEntity : TableEntity
    {
        public TopicEntity(string topic, string subscriberId)
        {
            PartitionKey = topic;
            RowKey = subscriberId;
        }

        public TopicEntity() { }
    }

    public static class SubscribeTopic
    {
        [FunctionName("SubscribeTopic")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            var input = Common.GetPostObject<SubscribeTopicInput>(req);
            var table = Common.GetAzureTable(Common.TopicsTableName);
            foreach (var topic in input.Topics)
            {
                var insertOperation = TableOperation.Insert(new TopicEntity(topic, input.SubscriberId));
                try
                {
                    table.ExecuteAsync(insertOperation);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }
            return new OkObjectResult(Common.GetSubscriberResponse(input.SubscriberId));
        }
    }
}
