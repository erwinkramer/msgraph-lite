using Microsoft.OpenApi.Reader;
using Microsoft.OpenApi.YamlReader;

var tenantId = "b81eb003-1c5c-45fd-848f-90d9d3f8d016";
var subscriptionId = "beb880cc-af9a-4e4d-8e8e-54739967674f";
var resourceGroupId = "apim001";
var apimResourceId = "apim001-r";

var outputDirectoryOperationIdsGrouped = "../../../../generated/operationIdsGrouped";
var outputDirectoryGraphLite = "../../../../generated/graphLite";

Directory.CreateDirectory(outputDirectoryOperationIdsGrouped);
Directory.CreateDirectory(outputDirectoryGraphLite);

var graphOpenApiSpecUrl = "https://raw.githubusercontent.com/microsoftgraph/msgraph-metadata/refs/heads/master/openapi/v1.0/openapi.yaml";
var httpClient = new HttpClient();

var stream = await httpClient.GetStreamAsync(graphOpenApiSpecUrl);
var openApiReadResult = await new OpenApiYamlReader().ReadAsync(stream, new Uri("https://graph.microsoft.com/v1.0"), new OpenApiReaderSettings() );
//var openApiDocument = new OpenApiStringReader().Read(File.ReadAllText(@"C:\Users\me\hello.json"), out var diagnostics);

if (Helper.HasDuplicateOperationIds(openApiReadResult.Document, out List<string> duplicates))
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

var groupedOperationIds = Helper.GroupOperationIdsByTagAndMethod(openApiReadResult.Document);

var totalOperationCount = Helper.WriteGroupsToFiles(groupedOperationIds, outputDirectoryOperationIdsGrouped);

Console.WriteLine($"There are {totalOperationCount} operations in the Graph API!");

var apiFiles = await Helper.WriteGroupsAsOpenApiSpecToFiles(groupedOperationIds, outputDirectoryGraphLite, openApiReadResult.Document);

foreach (var apiFile in apiFiles)
{
    await Helper.ImportApiAsync(tenantId, subscriptionId, resourceGroupId, apimResourceId, apiFile.ApiName, apiFile.FilePath);
}
