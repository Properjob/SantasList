using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

var cache = builder.AddRedisContainer("cache");

var storageBuilder = builder.AddAzureStorage("storage").UseEmulator(49634, 49635, 49636);

var queue = storageBuilder.AddQueues("queue");
var table = storageBuilder.AddTables("table");

var apiservice = builder.AddProject<Projects.SantasList_ApiService>("apiservice")
    .WithReference(queue)
    .WithReference(table);

builder.AddProject<Projects.SantasList_Web>("webfrontend")
    .WithReference(cache)
    .WithReference(apiservice);

builder.AddProject<Projects.SantasList_Worker>("santaslist.worker")
    .WithReference(queue)
    .WithReference(table);

builder.Build().Run();
