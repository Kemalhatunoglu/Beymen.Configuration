using Beymen.ConfigStorage;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

string mongoConnectionString = builder.Configuration["MongoConnectionString"];
string mongoDatabaseName = builder.Configuration["MongoDatabaseName"];
string mongoCollectionName = builder.Configuration["MongoCollectionName"];
string rabbitMQConnectionString = builder.Configuration["RabbitMQConnectionString"];
string serviceName = builder.Configuration["ServiceName"];
int configCheckIntervalSeconds = int.Parse(builder.Configuration["ConfigCheckIntervalSeconds"]);

builder.Services.AddSingleton(provider => new Beymen.ConfigLibrary.ConfigurationManager(
    serviceName,
    TimeSpan.FromSeconds(configCheckIntervalSeconds),
    () => new MongoConfigStorage(
        mongoConnectionString,
        mongoDatabaseName,
        mongoCollectionName,
        rabbitMQConnectionString).LoadConfigurationsAsync(serviceName),
    rabbitMQConnectionString
));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServiceA API V1");
    });
}

app.MapGet("/config/{key}", async (string key, Beymen.ConfigLibrary.ConfigurationManager configManager) =>
{
    var value = configManager.GetConfiguration<string>(key);
    return value is not null ? Results.Ok(value) : Results.NotFound();
});

app.Run();