var builder = DistributedApplication.CreateBuilder(args);

// SQL Server 
var sqlDb = builder.AddConnectionString("DefaultConnection");

// MongoDB 
var mongoDb = builder.AddConnectionString("mongodb");

// RabbitMQ 
var rabbit = builder.AddRabbitMQ("rabbitmq",
        userName: builder.AddParameter("rabbitmq-user"),
        password: builder.AddParameter("rabbitmq-password", secret: true),
        port: 5672)
    .WithDataVolume("rabbitmq-data")
    .WithManagementPlugin(port: 15672);

//API

//builder.AddProject<Projects.Customer_Mangment>("customer-mangment")
builder.AddContainer("customer-mangment", "customer-mangment")
    .WithDockerfile(
        contextPath: "../",
        dockerfilePath: "Customer_Mangment/Docker/Dockerfile"
    )
    .WithReference(sqlDb)
    .WithReference(mongoDb)
    .WithReference(rabbit)
    .WaitFor(rabbit)
    .WithHttpEndpoint(port: 5000, targetPort: 8080)
    .WithUrls(c =>
    {
        c.Urls.Add(new() { Url = "http://localhost:5000/swagger", DisplayText = "Swagger UI" });
        c.Urls.Add(new() { Url = "http://localhost:5000/scalar/v1", DisplayText = "Scalar UI" });
    })
    .WithHttpHealthCheck("/health");

builder.Build().Run();
