using Microsoft.OpenApi.Models;

partial class Helper
{
    private const int SplitThreshold = 500;
    private const int MinimumSubGroupSize = 10;  // Minimum size of sub-group to keep separate
    private static readonly HashSet<string> ReadOperationKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "get",
        "list",
        "check",
        "retrieve",
        "fetch",
        "view"
    };

    public static Dictionary<string, List<string>> GroupOperationIdsByTagAndMethod(OpenApiDocument openApiDocument)
    {
        var operationsGrouped = new Dictionary<string, List<string>>();

        foreach (var path in openApiDocument.Paths)
        {
            foreach (var operation in path.Value.Operations)
            {
                var method = operation.Key.ToString();
                var operationItem = operation.Value;

                var isReadOperation = IsReadOperation(operationItem.OperationId);

                var baseGroupKey = (method == "Get" || isReadOperation) ? $"{GetFirstTagPart(operationItem.Tags.FirstOrDefault()?.Name ?? string.Empty)}.read" : $"{GetFirstTagPart(operationItem.Tags.FirstOrDefault()?.Name ?? string.Empty)}.modify";

                if (!operationsGrouped.ContainsKey(baseGroupKey))
                {
                    operationsGrouped[baseGroupKey] = new List<string>();
                }

                operationsGrouped[baseGroupKey].Add(operationItem.OperationId);
            }
        }

        var finalGroupedOperations = new Dictionary<string, List<string>>();
        foreach (var group in operationsGrouped)
        {
            if (group.Value.Count > SplitThreshold)
            {
                var subGroups = SplitLargeGroupBasedOnSecondPart(group.Key, group.Value);
                if (subGroups.Count == 2)
                {
                    var mainGroup = subGroups.First().Value;
                    var subGroup = subGroups.Last().Value;

                    // Combine main group and single sub-group
                    var combinedKey = subGroups.First().Key;
                    if (subGroups.First().Key == group.Key)
                    {
                        combinedKey = $"{group.Key}.combined";
                    }

                    finalGroupedOperations[combinedKey] = mainGroup.Concat(subGroup).ToList();
                }
                else
                {
                    foreach (var subGroup in subGroups)
                    {
                        finalGroupedOperations[subGroup.Key] = subGroup.Value;
                    }
                }
            }
            else
            {
                finalGroupedOperations[group.Key] = group.Value;
            }
        }

        return finalGroupedOperations;
    }

    private static List<KeyValuePair<string, List<string>>> SplitLargeGroupBasedOnSecondPart(string baseGroupKey, List<string> operations)
    {
        var subGroups = new Dictionary<string, List<string>>();

        foreach (var operationId in operations)
        {
            var secondPart = GetSecondPartOfOperationId(operationId);

            if (!subGroups.ContainsKey(secondPart))
            {
                subGroups[secondPart] = new List<string>();
            }

            subGroups[secondPart].Add(operationId);
        }

        // Flatten sub-groups into a list of key-value pairs
        var resultGroups = new List<KeyValuePair<string, List<string>>>();

        foreach (var subGroup in subGroups)
        {
            if (subGroup.Value.Count < MinimumSubGroupSize)
            {
                resultGroups.Add(new KeyValuePair<string, List<string>>(baseGroupKey, subGroup.Value));
            }
            else
            {
                var subGroupKey = $"{baseGroupKey}.{subGroup.Key}";
                resultGroups.Add(new KeyValuePair<string, List<string>>(subGroupKey, subGroup.Value));
            }
        }

        return resultGroups;
    }

    private static string GetSecondPartOfOperationId(string operationId)
    {
        var parts = operationId.Split('.');
        return parts.Length > 1 ? parts[1] : string.Empty;
    }

    private static bool IsReadOperation(string operationId)
    {
        return ReadOperationKeywords.Any(keyword => operationId.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
    }

    private static string GetFirstTagPart(string tagName)
    {
        var parts = tagName.Split('.');
        return parts.Length > 0 ? parts[0] : tagName;
    }

}