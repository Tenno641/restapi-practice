using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Connecty.Api.Scalar;

public class MediaTypeVersionTransformer : IOpenApiOperationTransformer 
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        operation.Parameters ??= [];
        
        operation.Parameters.Add(new OpenApiParameter
        {
            In = ParameterLocation.Header,
            Name = "Accept",
            Required = true,
            Description = "Specify API version",
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String
            }
        });
        
        return Task.CompletedTask;
    }
}