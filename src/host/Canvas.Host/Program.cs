using Canvas.Host.Modules;
using Canvas.Infrastructure.Shared.Modules;
using Scalar.AspNetCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) =>
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .WriteTo.Console());

builder.Services.AddHealthChecks();
builder.Services.AddOpenApi();
builder.Services.AddModules(builder.Configuration, typeof(Program).Assembly);

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapOpenApi();

if (app.Environment.IsDevelopment())
    app.MapScalarApiReference();

app.MapModules(app.Services);

app.Run();

public partial class Program { }
