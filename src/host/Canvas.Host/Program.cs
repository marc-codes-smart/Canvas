using Canvas.Host.Modules;
using Canvas.Infrastructure.Shared.Modules;
using Canvas.Infrastructure.Shared.Persistence;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .WriteTo.Console());

builder.Services.AddDbContext<CanvasDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CanvasDb")));

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

builder.Services.AddHealthChecks();

builder.Services.AddOpenApi();
builder.Services.AddModules(builder.Configuration, typeof(Program).Assembly);

var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    await using var scope = app.Services.CreateAsyncScope();
    var db = scope.ServiceProvider.GetRequiredService<CanvasDbContext>();
    await db.Database.MigrateAsync();
}

app.UseCors();

app.MapHealthChecks("/health");
app.MapOpenApi();

if (app.Environment.IsDevelopment())
    app.MapScalarApiReference();

app.MapModules(app.Services);

app.Run();

public partial class Program { }
