using System.Data;
using Connecty.Application;
using Connecty.Application.data;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddDatabase(builder.Configuration.GetConnectionString("postgres") ?? throw new InvalidOperationException())
    .AddControllers();

var app = builder.Build();

app.MapControllers();

using IDbConnection connection = await app.Services
    .GetRequiredService<IDatabaseConnectionFactory>()
    .CreateConnectionAsync();

await new DatabaseInitializer(connection).InitializeAsync();

app.Run();