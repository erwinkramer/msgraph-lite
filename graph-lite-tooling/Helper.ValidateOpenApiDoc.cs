using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;

partial class Helper
{
    public static void ValidateOpenApiDoc(OpenApiDocument document)
    {
        var validationErrors = document.Validate(Microsoft.OpenApi.Validations.ValidationRuleSet.GetDefaultRuleSet());
        if (validationErrors.Any())
        {
            throw new Exception($"One or more validation errors found in doc with title '{document.Info.Title}'.");
        }
    }
}