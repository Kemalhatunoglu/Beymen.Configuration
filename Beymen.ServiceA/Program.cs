using Beymen.ConfigLibrary;
using Beymen.ConfigStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(provider =>
{
    var storage = new MongoConfigStorage("mongodb://localhost:27017", "ConfigDB", "Configs");
    return new ConfigurationManager("SERVICE-A", TimeSpan.FromSeconds(30), () => storage.LoadConfigurationsAsync("SERVICE-A"));
});

var app = builder.Build();

app.MapGet("/config/{key}", (string key, ConfigurationManager configManager) =>
{
    var value = configManager.GetConfiguration<string>(key);
    return value is not null ? Results.Ok(value) : Results.NotFound();
});

app.Run();