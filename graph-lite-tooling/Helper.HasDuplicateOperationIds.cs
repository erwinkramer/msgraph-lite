using Microsoft.OpenApi.Models;

partial class Helper
{
    public static bool HasDuplicateOperationIds(OpenApiDocument document, out List<string> duplicateOperationIds)
    {
        // Dictionary to track operation ids and their counts
        var operationIdCount = new Dictionary<string, int>();
        duplicateOperationIds = new List<string>();

        // Iterate through all paths and operations
        foreach (var pathItem in document.Paths)
        {
            foreach (var operation in pathItem.Value.Operations.Values)
            {
                if (!string.IsNullOrEmpty(operation.OperationId))
                {
                    if (operationIdCount.ContainsKey(operation.OperationId))
                    {
                        // Increment the count if already exists
                        operationIdCount[operation.OperationId]++;
                    }
                    else
                    {
                        // Add to dictionary if not exists
                        operationIdCount[operation.OperationId] = 1;
                    }
                }
            }
        }

        // Find operation ids with more than one occurrence
        duplicateOperationIds = operationIdCount
            .Where(kvp => kvp.Value > 1)
            .Select(kvp => kvp.Key)
            .ToList();

        // Return true if there are any duplicates
        return duplicateOperationIds.Count > 0;
    }
}
