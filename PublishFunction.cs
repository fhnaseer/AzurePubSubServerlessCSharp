using System.Net.Http;
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
    public class PublishFunctionInput
    {
        [JsonProperty("subscriptionTopic")]
        public string SubscriptionTopic { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }
    }

    public static class PublishFunction
    {
        private static readonly HttpClient Client = new HttpClient();

        [FunctionName("PublishFunction")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            var input = Common.GetPostObject<PublishFunctionInput>(req);
            var entities = await Common.GetEntities<FunctionEntity>(Common.FunctionsTableName, input.SubscriptionTopic);
            foreach (var entity in entities)
            {
                var queue = Common.GetSubsriberQueue(entity.RowKey);
                if (entity.FunctionType == "url")
                {
                    var response = await Client.GetAsync(entity.MatchingFunction);
                    var responseString = await response.Content.ReadAsStringAsync();
                    var message = new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(responseString)));
                    queue.SendAsync(message);
                }
            }
            return new OkObjectResult(Common.GetPublishResponse());
        }
    }
}
