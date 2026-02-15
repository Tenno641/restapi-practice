using System.Data;
using System.Text;
using Asp.Versioning;
using Connecty.Api.Middlewares;
using Connecty.Application;
using Connecty.Application.data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Connecty.Api.Auth;

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
}).AddApiExplorer();

var app = builder.Build();

app.UseMiddleware<ValidationMappingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

using IDbConnection connection = await app.Services
    .GetRequiredService<IDatabaseConnectionFactory>()
    .CreateConnectionAsync();
await new DatabaseInitializer(connection).InitializeAsync();

app.Run();