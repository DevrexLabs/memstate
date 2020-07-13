# Azure Table Storage

The storage provider uses StreamStone, a rock solid event sourcing library.
Multiple streams can be written to the same table using the stream name as PartitionKey.

## How to use the storage provider

1. Make sure you have the Memstate.Azure nuget installed then something like this:

```csharp
var config = Config.CreateDefault();

var cloudTable = ConfigureCloudTable(); //see helper method below
config.UseAzureTableStorage(cloudTable); //here is where the magic happens

//The provider will use the StreamName from EngineSettings, the default is "memstate":
config.GetSettings<EngineSettings>().StreamName = "my-stream";

//That's it, you should be all set. Start your engines!
var engine = await Engine.Start<RedisModel>(config);

//This is just basic Azure SDK configuration
private static CloudTable GetCloudTable(string tableName)
{
    //you can find the connection string in the Azure Portal.
    //This example uses environment variables which is one way to do it
    var connectionString = Environment.GetEnvironmentVariable("AZURE_CLOUDSTORAGE_CONNECTION");

    if (String.IsNullOrEmpty(connectionString))
       throw new Exception("AZURE_CLOUDSTORAGE_CONNECTION env variable not set");

    var account = CloudStorageAccount.Parse(connectionString);
    var client = account.CreateCloudTableClient();
    return client.GetTableReference(tableName);
}
```