using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

// Dont connect to cosmos at first. calls will be stuck in pending 
// cause determisnic issue
// 

namespace Company.Function
{public class ToDoItem
{
    public string User { get; set; }
    public string id { get; set; }
    public string Description { get; set; }
    public string Time { get; set; }
}

    public  class DurableFunctionsOrchestrationCSharp1
    {

        [FunctionName(nameof(StoreResults))]
        public static async Task StoreResults(
            [CosmosDB(
                databaseName: "ToDoList",
                containerName: "Items",
                Connection = "CosmosDBConnection")] IAsyncCollector<dynamic> documents,
            [ActivityTrigger] List<ToDoItem> outputs)
        {
            foreach (var output in outputs)  
            {  
                await documents.AddAsync(output);  
            } 
        }



        [FunctionName("DurableFunctionsOrchestrationCSharp1")]
        public async Task<List<ToDoItem>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {

            var currentUser = context.NewGuid().ToString();
            var date =  DateTime.Now.ToString();
            var outputs = new List<ToDoItem>();
            var tasks = new List<Task<ToDoItem>>();
            var userTodoList = new List<ToDoItem>();

            ToDoItem toDoItem = await context.CallActivityAsync<ToDoItem>(nameof(SayHello), new ToDoItem {id = Guid.NewGuid().ToString(), User = currentUser, Description = "Read a book", Time = date });
            ToDoItem toDoItem2 =  await context.CallActivityAsync<ToDoItem>(nameof(SayHello), new ToDoItem {id = Guid.NewGuid().ToString(), User = currentUser, Description = "Walk the dog", Time = date});
            
            userTodoList.Add(toDoItem);
            userTodoList.Add(toDoItem2);

            await context.CallActivityAsync(nameof(StoreResults), userTodoList);

            return userTodoList;
        }


        [FunctionName(nameof(SayHello))]
        public static ToDoItem SayHello([ActivityTrigger] ToDoItem item, ILogger log)
        {
            log.LogInformation("Item.id = {name}.", item.User);
            return item;
        }

        [FunctionName("DurableFunctionsOrchestrationCSharp1_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestMessage req,
            [OrchestrationClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("DurableFunctionsOrchestrationCSharp1", null);

            log.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}