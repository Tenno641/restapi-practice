using Connecty.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();