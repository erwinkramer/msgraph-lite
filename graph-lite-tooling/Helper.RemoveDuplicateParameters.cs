/*

Removes duplicate parameters from an API document.

Can be removed if fixed in OpenAPI.NET: https://github.com/microsoft/OpenAPI.NET/issues/1833

*/

using Microsoft.OpenApi.Models;

partial class Helper
{
    public static OpenApiDocument RemoveDuplicateParameters(OpenApiDocument document)
    {
        // Iterate over each path in the OpenApiDocument
        foreach (var pathItem in document.Paths.Values)
        {
            // Create a list to keep track of unique parameters
            var uniqueParameters = new List<OpenApiParameter>();

            foreach (var parameter in pathItem.Parameters)
            {
                // Check if the parameter is already present in the unique list
                if (!uniqueParameters.Any(p => p.Name == parameter.Name && p.In == parameter.In))
                {
                    // Add parameter to the unique list if not a duplicate
                    uniqueParameters.Add(parameter);
                }
            }

            // Replace the operation's parameters with the unique list
            pathItem.Parameters = uniqueParameters;

        }

        return document;
    }
}
