using ExternalIdentity;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .Configure<JwtOptions>(builder.Configuration.GetSection("Jwt"))
    .AddScoped<JsonTokenService>()
    .AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();