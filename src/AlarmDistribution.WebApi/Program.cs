using AlarmDistribution.WebApi.Extensions;
using AlarmDistribution.WebApi.Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi("v1");

builder.Services.AddAppServices(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    var initScriptPath = Path.Combine(AppContext.BaseDirectory, "initializedb.sql");
    if (File.Exists(initScriptPath))
    {
        var initScript = await File.ReadAllTextAsync(initScriptPath);
        await dbContext.Database.ExecuteSqlRawAsync(initScript);
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "AlarmDistribution API v1");
    });
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
