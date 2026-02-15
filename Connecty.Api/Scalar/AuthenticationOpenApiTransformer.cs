using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Connecty.Api.Scalar;

public class AuthenticationOpenApiTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        
        document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            In = ParameterLocation.Header,
            Name = "Authorization",
            Scheme = "Bearer",
            Description = "Provide Valid Web Token"
        });
        
        return Task.CompletedTask;
    }
}