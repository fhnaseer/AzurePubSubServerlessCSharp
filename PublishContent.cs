using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace AzurePubSubServerlessCSharp
{
    internal class PublishContentInput
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("content")]
        public List<Content> Contents { get; set; }
    }

    public static class PublishContent
    {
        [FunctionName("PublishContent")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            var input = Common.GetPostObject<PublishContentInput>(req);

            var entities = await Common.GetEntities<ContentEntity>(Common.ContentsTableName);
            foreach (var topic in input.Contents)
            {
                var entity = entities.FirstOrDefault(e => e.PartitionKey == topic.Key);
                if (entity == null) continue;
                var messageBody = new
                {
                    topic,
                    message = input.Message
                };
                var queue = Common.GetSubsriberQueue(entity.RowKey);
                if (!CheckConditions(entity.Condition, entity.Value, entity.Value)) continue;
                var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageBody)));
                queue.SendAsync(message);
            }
            return new OkObjectResult(Common.GetPublishResponse());
        }

        private static bool CheckConditions(string condition, string publicationValueString, string subscriberValueString)
        {
            if (condition == ">=" || condition == ">" || condition == "<" || condition == "<=")
            {
                var publicationVal = double.Parse(publicationValueString);
                var subscriberVal = double.Parse(subscriberValueString);
                if (double.IsNaN(publicationVal) || double.IsNaN(subscriberVal))
                    return false;
                if (condition == ">=")
                {
                    if (publicationVal >= subscriberVal)
                        return true;
                }
                else if (condition == ">")
                {
                    if (publicationVal > subscriberVal)
                        return true;
                }
                else if (condition == "<")
                {
                    if (publicationVal < subscriberVal)
                        return true;
                }
                else if (condition == "<=")
                {
                    if (publicationVal <= subscriberVal)
                        return true;
                }
            }
            else if (condition == "=")
            {
                var publicationVal = publicationValueString;
                var subscriberVal = subscriberValueString;
                if (string.Equals(publicationVal, subscriberVal, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}
