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
    internal class SubscribeContentInput
    {
        [JsonProperty("subscriberId")]
        public string SubscriberId { get; set; }

        [JsonProperty("content")]
        public List<Content> Contents { get; set; }
    }

    internal class Content
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }

        [JsonProperty("condition")]
        public string Condition { get; set; }
    }

    internal class SubscribeContentEntity : TableEntity
    {
        public SubscribeContentEntity(string key, string subscriberId, string value, string condition)
        {
            PartitionKey = key;
            RowKey = subscriberId;
            Value = value;
            Condition = condition;
        }

        public SubscribeContentEntity() { }

        public string Value { get; set; }

        public string Condition { get; set; }
    }

    public static class SubscribeContent
    {
        [FunctionName("SubscribeContent")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]
            HttpRequest req, TraceWriter log)
        {
            var input = Common.GetPostObject<SubscribeContentInput>(req);
            var table = Common.GetAzureTable(Common.ContentsTableName);
            foreach (var content in input.Contents)
            {
                var insertOperation = TableOperation.Insert(new SubscribeContentEntity(content.Key, input.SubscriberId, content.Value, content.Condition));
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
