using Microsoft.OpenApi.Readers;

var tenantId = "b81eb003-1c5c-45fd-848f-90d9d3f8d016";
var subscriptionId = "beb880cc-af9a-4e4d-8e8e-54739967674f";
var resourceGroupId = "rg-erwin";
var apimResourceId = "api-erwin";

var outputDirectoryOperationIdsGrouped = "../../../generated/operationIdsGrouped";
var outputDirectoryGraphLite = "../../../generated/graphLite";

Directory.CreateDirectory(outputDirectoryOperationIdsGrouped);
Directory.CreateDirectory(outputDirectoryGraphLite);

var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://raw.githubusercontent.com/microsoftgraph/msgraph-metadata/refs/heads/master/")
};

var stream = await httpClient.GetStreamAsync("openapi/v1.0/openapi.yaml");
var openApiDocument = new OpenApiStreamReader().Read(stream, out var diagnostic);
//var openApiDocument = new OpenApiStringReader().Read(File.ReadAllText(@"C:\Users\me\hello.json"), out var diagnostics);

if (Helper.HasDuplicateOperationIds(openApiDocument, out List<string> duplicates))
{
    Console.WriteLine("Duplicate OperationIds found:");
    foreach (var id in duplicates)
    {
        Console.WriteLine(id);
    }
}
else
{
    Console.WriteLine("No duplicate OperationIds found.");
}

var groupedOperationIds = Helper.GroupOperationIdsByTagAndMethod(openApiDocument);

var totalOperationCount = Helper.WriteGroupsToFiles(groupedOperationIds, outputDirectoryOperationIdsGrouped);

Console.WriteLine($"There are {totalOperationCount} operations in the Graph API!");

var apiFiles = await Helper.WriteGroupsAsOpenApiSpecToFiles(groupedOperationIds, outputDirectoryGraphLite, openApiDocument);

foreach (var apiFile in apiFiles)
{
    await Helper.ImportApiAsync(tenantId, subscriptionId, resourceGroupId, apimResourceId, apiFile.ApiName, apiFile.FilePath);
}
