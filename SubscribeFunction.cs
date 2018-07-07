using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace AzurePubSubServerlessCSharp
{
    public class SubscribeFunctionInput
    {
        [JsonProperty("subscriptionTopic")]
        public string SubscriptionTopic { get; set; }

        [JsonProperty("functionType")]
        public string FunctionType { get; set; }

        [JsonProperty("subscriberId")]
        public string SubscriberId { get; set; }

        [JsonProperty("matchingInputs")]
        public string MatchingInputs { get; set; }

        [JsonProperty("matchingFunction")]
        public string MatchingFunction { get; set; }
    }

    internal class FunctionEntity : TableEntity
    {
        public FunctionEntity(string subscriptionTopic, string subscriberId, string functionType, string matchinFunction, string matchingInputs)
        {
            PartitionKey = subscriptionTopic;
            RowKey = subscriberId;
            FunctionType = functionType;
            MatchingFunction = matchinFunction;
            MatchingInputs = matchingInputs;
        }

        public string FunctionType { get; set; }

        public string MatchingFunction { get; set; }

        public string MatchingInputs { get; set; }
    }

    public static class SubscribeFunction
    {
        [FunctionName("SubscribeFunction")]
        public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)]HttpRequest req, TraceWriter log)
        {
            var input = Common.GetPostObject<SubscribeFunctionInput>(req);
            var table = Common.GetAzureTable(Common.FunctionsTableName);
            var insertOperation = TableOperation.Insert(new FunctionEntity(input.SubscriptionTopic, input.SubscriberId, input.FunctionType, input.MatchingFunction, input.MatchingInputs));
            try
            {
                table.ExecuteAsync(insertOperation);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            return new OkObjectResult(Common.GetSubscriberResponse(input.SubscriberId));
        }
    }
}
