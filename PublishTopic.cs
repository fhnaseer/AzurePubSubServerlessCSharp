using System;
using System.Collections.Generic;
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
    public class PublishTopicInput
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("fromPublisher")]
        public string FromPublisher { get; set; }

        [JsonProperty("topics")]
        public List<string> Topics { get; set; }
    }

    public static class PublishTopic
    {
        [FunctionName("PublishTopic")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            var input = Common.GetPostObject<PublishTopicInput>(req);
            var functionInvoked = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");

            var taskList = new List<Task>();
            foreach (var topic in input.Topics)
            {
                var entities = await Common.GetEntities<TopicEntity>(Common.TopicsTableName, topic);
                var databaseAccessed = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff");

                foreach (var entity in entities)
                {
                    var queue = Common.GetSubsriberQueue(entity.RowKey);
                    var messageBody = new
                    {
                        type = $"Topic-based: {topic}",
                        message = input.Message,
                        fromPublisher = input.FromPublisher,
                        functionInvoked = functionInvoked,
                        databaseAccessed = databaseAccessed
                    };
                    var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(messageBody)));
                    taskList.Add(queue.SendAsync(message));
                }
            }
            await Task.WhenAll(taskList);
            return new OkObjectResult(Common.GetPublishResponse());
        }
    }
}
