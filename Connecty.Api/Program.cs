using System.Data;
using System.Text;
using Asp.Versioning;
using Connecty.Api.Middlewares;
using Connecty.Application;
using Connecty.Application.data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Connecty.Api.Auth;
using Connecty.Api.Scalar;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddDatabase(builder.Configuration.GetConnectionString("postgres") ?? throw new InvalidOperationException())
    .AddControllers();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    var jwtOptions = builder.Configuration.GetSection("Jwt");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidIssuer = jwtOptions.GetValue<string>("Issuer"),
        ValidateIssuer = false,
        ValidAudience = jwtOptions.GetValue<string>("Audience"),
        ValidateAudience = false,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.GetValue<string>("Key") ?? throw new ArgumentException("Please Provide Jwt Key"))),
        ValidateIssuerSigningKey = true
    };
});

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthConstants.AdminPolicy, policyBuilder =>
    {
        policyBuilder.RequireClaim(AuthConstants.AdminPolicy, "true");
    
    }).AddPolicy(AuthConstants.TrustedMember, policyBuilder => policyBuilder.RequireAssertion(context =>
    {
          return context.User.HasClaim(claim => claim is { Type: AuthConstants.AdminPolicy, Value: "true" }) ||
                 context.User.HasClaim(claim => claim is { Type: AuthConstants.TrustedMember, Value: "true" }); 
    }));

builder.Services.AddApiVersioning(options =>
{
     options.ApiVersionReader = new MediaTypeApiVersionReader("api-version");
     options.DefaultApiVersion = new ApiVersion(1, 0);
     options.AssumeDefaultVersionWhenUnspecified = true;
     options.ReportApiVersions = true;
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'V";
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer<AuthenticationOpenApiTransformer>();
});

builder.Services.AddOpenApi("v2", options =>
{
    options.AddOperationTransformer<MediaTypeVersionTransformer>();
    options.AddDocumentTransformer<AuthenticationOpenApiTransformer>();
});

// builder.Services.AddResponseCaching();

builder.Services.AddOutputCache(options =>
{
    options.AddPolicy("movies", policyBuilder =>
    {
        policyBuilder
            .Expire(TimeSpan.FromMinutes(1))
            .SetVaryByHeader(["Accept", "Accept-Encoding", "User-Agent"])
            .SetVaryByQuery(["title", "year", "sortBy", "page", "pageSize"])
            .Tag("movies");
    });
});

var app = builder.Build();

app.MapOpenApi();
app.MapScalarApiReference(options =>
{
    options.AddDocument("v1", "Version 1.0", "openapi/v1.json");
    options.AddDocument("v2", "Version 2.0", "openapi/v2.json");
});

app.UseMiddleware<ValidationMappingMiddleware>();

app.UseCors();
app.UseOutputCache();
// app.UseResponseCaching();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using IDbConnection connection = await app.Services
    .GetRequiredService<IDatabaseConnectionFactory>()
    .CreateConnectionAsync();
await new DatabaseInitializer(connection).InitializeAsync();

app.Run();