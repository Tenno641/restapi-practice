using System.Data;
using System.Net;
using System.Security.Claims;
using System.Text;
using Connecty.Api;
using Connecty.Api.Controllers;
using Connecty.Api.Middlewares;
using Connecty.Application;
using Connecty.Application.data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AuthenticationSchemes = Microsoft.AspNetCore.Server.HttpSys.AuthenticationSchemes;

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
        policyBuilder.RequireAssertion(context =>
        {
            return context.User.HasClaim(claim => claim is { Type: AuthConstants.AdminPolicy, Value: "true" }) ||
                   context.User.HasClaim(claim => claim is { Type: AuthConstants.TrustedMember, Value: "true" });
        });
    });

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