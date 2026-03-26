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

builder.AddProject<Projects.Customer_Mangment>("customer-mangment")
    .WithReference(sqlDb)
    .WithReference(mongoDb)
    .WithReference(rabbit)
    .WaitFor(rabbit);

builder.Build().Run();
