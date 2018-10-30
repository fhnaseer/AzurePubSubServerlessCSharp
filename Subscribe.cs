using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;

namespace AzurePubSubServerlessCSharp
{
    public class Subscriber
    {
        [JsonProperty("connectionString")]
        public string ConnectionString { get; set; }

        [JsonProperty("queueName")]
        public string QueueName { get; set; }

        [JsonProperty("QueueUrl")]
        public string QueueUrl { get; set; }

        [JsonProperty("SubscriberId")]
        public string SubscriberId { get; set; }
    }

    public static class Subscribe
    {
        [FunctionName("Subscribe")]
        public static Subscriber Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            var guid = Guid.NewGuid().ToString().Replace("-", "");
            var id = $"subscriber{guid}";
            try
            {
                return Common.CreateSubscriberQueue(id);
            }
            catch (Exception e)
            {
                log.Info(e.Message);
                return null;
            }
        }
    }
}
