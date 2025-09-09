using System.Text.Json.Serialization;
using Dapr;
using Dapr.Client;

//run with: PS C:\Git\DaprPlayground\src\sdk\order-processor2> dapr run --app-id order-processor --resources-path ../../../components --app-port 7007 -- dotnet run
//NOTE: --app-id has to be identical with other worker tasks, so they use the same queue, otherwise the get separate queues! 

const string DAPR_STORE_NAME = "statestore";
var builder = WebApplication.CreateBuilder(args);
var client = new DaprClientBuilder().Build();
var app = builder.Build();

// Dapr will send serialized event object vs. being raw CloudEvent
app.UseCloudEvents();

// needed for Dapr pub/sub routing
app.MapSubscribeHandler();

if (app.Environment.IsDevelopment()) {app.UseDeveloperExceptionPage();}

// Dapr subscription in [Topic] routes orders topic to this route
app.MapPost("/orders", [Topic("redispubsub", "orders")] async (Order order) => {
    Console.WriteLine("Subscriber 2 received : " + order);
    
    var amount = await client.GetStateAsync<int>(DAPR_STORE_NAME, "Amount");
    var amount2 = await client.GetStateAsync<int>(DAPR_STORE_NAME, "Amount2") +1;
    Console.WriteLine("Overall received packages in Subscriber 1: " + amount);
    Console.WriteLine("Overall received packages in Subscriber 2: " + amount2);
    await client.SaveStateAsync(DAPR_STORE_NAME, "Amount2", amount2);
    
    await Task.Delay(TimeSpan.FromSeconds(1.69));
    return Results.Ok(order);
});

await app.RunAsync();

public record Order([property: JsonPropertyName("orderId")] int OrderId);
