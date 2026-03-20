using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Backend.Common.Swagger;

/// <summary>
/// Rewrites all query parameter names to camelCase in the OpenAPI spec.
/// ASP.NET Core model binding is case-insensitive, so ?roomId=5 binds correctly
/// to a property named RoomId without any changes to the C# model classes.
/// </summary>
public class CamelCaseQueryParameterFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters == null) return;

        foreach (var parameter in operation.Parameters)
        {
            if (parameter.In == ParameterLocation.Query && parameter is OpenApiParameter concreteParam)
            {
                concreteParam.Name = ToCamelCase(concreteParam.Name);
            }
        }
    }

    private static string ToCamelCase(string? name) =>
        string.IsNullOrEmpty(name) ? name ?? string.Empty : char.ToLowerInvariant(name[0]) + name[1..];
}
