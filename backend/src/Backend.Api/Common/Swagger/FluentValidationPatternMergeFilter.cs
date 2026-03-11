using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Backend.Common.Swagger;

/// <summary>
/// Merges multiple allOf pattern-only entries (emitted by MicroElements for each .Matches() rule)
/// into a single combined lookahead regex on the parent string schema.
/// This prevents Orval from generating zod.unknown().regex() which causes TypeScript errors
/// because ZodUnknown has no .regex() method in Zod v4.
///
/// Implemented as IDocumentFilter (not ISchemaFilter) because MicroElements adds patterns
/// after all schema filters have run, so a schema filter would see an empty AllOf.
/// </summary>
public class FluentValidationPatternMergeFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        foreach (IOpenApiSchema schema in swaggerDoc.Components.Schemas.Values)
            MergePatterns(schema);
    }

    private static void MergePatterns(IOpenApiSchema schema)
    {
        if (schema is not OpenApiSchema concreteSchema)
            return;

        // Recurse into properties first
        if (concreteSchema.Properties != null)
        {
            foreach (IOpenApiSchema prop in concreteSchema.Properties.Values)
                MergePatterns(prop);
        }

        if (concreteSchema.Type != JsonSchemaType.String || concreteSchema.AllOf == null || concreteSchema.AllOf.Count == 0)
            return;

        List<IOpenApiSchema> patternOnlyEntries = [.. concreteSchema.AllOf
            .Where(s => s.Pattern != null
                && s.Type == null
                && (s.Properties == null || s.Properties.Count == 0)
                && (s.AllOf == null || s.AllOf.Count == 0)
                && s.MinLength == null && s.MaxLength == null
                && s.Minimum == null && s.Maximum == null)];

        if (patternOnlyEntries.Count == 0)
            return;

        string combined = string.Concat(patternOnlyEntries.Select(s => $"(?=.*{s.Pattern})"));
        concreteSchema.Pattern = combined;

        List<IOpenApiSchema> remaining = concreteSchema.AllOf
            .Where(s => !patternOnlyEntries.Contains(s))
            .ToList();

        concreteSchema.AllOf = remaining.Count > 0 ? remaining : null;
    }
}
