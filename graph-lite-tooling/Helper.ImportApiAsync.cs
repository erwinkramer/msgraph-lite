using Azure;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.ApiManagement;
using Azure.ResourceManager.ApiManagement.Models;

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
        var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            TenantId = tenantId
        });

        // Create the ARM client for Api Management operations
        var armClient = new ArmClient(credential, subscriptionId);
        var serviceResourceId = ApiManagementServiceResource.CreateResourceIdentifier(subscriptionId, resourceGroupName, serviceName);
        var apiManagementService = armClient.GetApiManagementServiceResource(serviceResourceId);
        var apiCollection = apiManagementService.GetApis();
        var apiContent = File.ReadAllText(apiFilePath);

        try
        {
            var apiParameters = new ApiCreateOrUpdateContent
            {
                DisplayName = $"Graph L - {apiId}",
                Path = apiId,
                Format = ContentFormat.OpenApiJson,
                Value = apiContent
            };

            await apiCollection.CreateOrUpdateAsync(WaitUntil.Completed, apiId, apiParameters);

            Console.WriteLine($"Successfully imported API '{apiId}' from file '{apiFilePath}'.");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed importing API '{apiId}' from file '{apiFilePath}'. Exception: {e.Message}.");
        }
    }
}
