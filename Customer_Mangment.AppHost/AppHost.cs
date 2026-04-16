var builder = DistributedApplication.CreateBuilder(args);

// SQL Server 
var sqlDb = builder.AddConnectionString("DefaultConnection");
var alahly = builder.AddConnectionString("alahly");
var meccano = builder.AddConnectionString("meccano");
var sqlServerCache = builder.AddConnectionString("sqlServerCache");

// MongoDB 
var mongoDb = builder.AddConnectionString("mongodb");

// RabbitMQ 
var rabbit = builder.AddRabbitMQ("rabbitmq",
        userName: builder.AddParameter("rabbitmq-user"),
        password: builder.AddParameter("rabbitmq-password", secret: true),
        port: 5672)
    .WithDataVolume("rabbitmq-data")
    .WithManagementPlugin(port: 15672);
// Redis
var redis = builder.AddRedis("redis", port: 6379)
    .WithDataVolume("redis-data");

//API

//builder.AddProject<Projects.Customer_Mangment>("customer-mangment")
builder.AddContainer("customer-mangment", "customer-mangment")
    .WithDockerfile(
        contextPath: "../",
        dockerfilePath: "Customer_Mangment/Docker/Dockerfile"
    )
    .WithReference(sqlDb)
    .WithReference(sqlServerCache)
    .WithReference(alahly)
    .WithReference(meccano)
    .WithReference(mongoDb)
    .WithReference(rabbit)
    .WithReference(redis)
    .WaitFor(rabbit)
    .WaitFor(redis)
    .WithHttpEndpoint(port: 5000, targetPort: 8080)
    .WithUrls(c =>
    {
        c.Urls.Add(new() { Url = "http://localhost:5000/swagger", DisplayText = "Swagger UI" });
        c.Urls.Add(new() { Url = "http://localhost:5000/scalar/v1", DisplayText = "Scalar UI" });
    })
    .WithHttpHealthCheck("/health");

builder.AddProject<Projects.Customer_Mangment_IdentityServer>("customer-mangment-identityserver");

builder.Build().Run();
