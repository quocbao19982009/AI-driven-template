using System.Reflection;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Backend.Common.Swagger;

/// <summary>
/// Schema filter that marks non-nullable properties as required in the OpenAPI schema.
/// This replaces [Required] data annotations on DTOs, allowing FluentValidation to be
/// the single source of truth for validation while still generating correct Swagger schemas.
/// </summary>
public class RequiredSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema.Properties == null || schema.Properties.Count == 0)
            return;

        var properties = context.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var property in properties)
        {
            // Get the schema property name (camelCase by default)
            var schemaPropertyName = char.ToLowerInvariant(property.Name[0]) + property.Name[1..];

            if (!schema.Properties.ContainsKey(schemaPropertyName))
                continue;

            var isNullable = IsNullableProperty(property);

            if (!isNullable)
            {
                schema.Required.Add(schemaPropertyName);
            }
        }
    }

    private static bool IsNullableProperty(PropertyInfo property)
    {
        // Value types: check for Nullable<T>
        if (property.PropertyType.IsValueType)
            return Nullable.GetUnderlyingType(property.PropertyType) != null;

        // Reference types: check NullabilityInfoContext
        var nullabilityContext = new NullabilityInfoContext();
        var nullabilityInfo = nullabilityContext.Create(property);
        return nullabilityInfo.WriteState == NullabilityState.Nullable;
    }
}
