using Azure.Identity;
using Azure.Core;
using Microsoft.Azure.Management.ApiManagement;
using Microsoft.Azure.Management.ApiManagement.Models;
using Microsoft.Rest;

partial class Helper
{
    public static async Task ImportApiAsync(string tenantId, string subscriptionId, string resourceGroupName, string serviceName,
                                            string apiId, string apiFilePath)
    {
        if (!File.Exists(apiFilePath))
        {
            throw new FileNotFoundException("File not found.", apiFilePath);
        }

        // Authenticate interactively using Azure Identity
        var serviceClientCredentials = await GetServiceClientCredentialsAsync(tenantId);

        // Create the ApiManagementClient
        using var client = new ApiManagementClient(serviceClientCredentials) { SubscriptionId = subscriptionId };
        var apiContent = File.ReadAllText(apiFilePath);

        try
        {
            await client.Api.CreateOrUpdateAsync(resourceGroupName, serviceName, apiId, new ApiCreateOrUpdateParameter
            {
                Format = ContentFormat.Openapijson,
                Value = apiContent,
                Path = apiId,
                DisplayName = $"Graph L - {apiId}",
            });

            Console.WriteLine($"Successfully imported API '{apiId}' from file '{apiFilePath}'.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed importing API '{apiId}' from file '{apiFilePath}'. Exception: {e.Message}.");
        }
    }

    private static async Task<ServiceClientCredentials> GetServiceClientCredentialsAsync(string tenantId)
    {

        var tokenCredential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            TenantId = tenantId
        });

        // Get the access token for Azure Management
        var tokenRequestContext = new TokenRequestContext(new[] { "https://management.azure.com/.default" });
        var token = await tokenCredential.GetTokenAsync(tokenRequestContext);

        return new TokenCredentials(token.Token);
    }
}
