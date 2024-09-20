using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Services;
using Microsoft.OpenApi.Writers;

partial class Helper
{
    public static async Task<List<(string FilePath, string ApiName)>> WriteGroupsAsOpenApiSpecToFiles(Dictionary<string, List<string>> groupedOperationIds, string outputDirectory, OpenApiDocument openApiDocument)
    {
        var fileInfos = new List<(string FilePath, string FileName)>();

        foreach (var group in groupedOperationIds)
        {

            Func<string, OperationType?, OpenApiOperation, bool> predicate = (operationId, operationType, operation) =>
                 {
                     return group.Value.Contains(operation.OperationId); //funny enough, operationId is not the same as operation.OperationId
                 };

            var filteredDoc = OpenApiFilterService.CreateFilteredDocument(openApiDocument, predicate);
            var sanitizedDoc = RemoveDuplicateParameters(filteredDoc);

            // Write the OpenApiDocument to a file
            var apiName = group.Key.Replace('.', '-');
            var fileName = $"{apiName}.json";
            var outputDirectoryFile = Path.Combine(outputDirectory, fileName);
            using var fileStream = new FileStream(outputDirectoryFile, FileMode.Create, FileAccess.Write);
            using var streamWriter = new StreamWriter(fileStream);
            var jsonWriter = new OpenApiJsonWriter(streamWriter);
            sanitizedDoc.SerializeAsV3(jsonWriter);

            await streamWriter.FlushAsync();

            // Add the tuple of file path and file name to the list
            fileInfos.Add((outputDirectoryFile, apiName));
        }
        // Return the list of tuples
        return fileInfos;
    }
}