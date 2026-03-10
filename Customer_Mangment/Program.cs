using Customer_Mangment.Controllers;
using Customer_Mangment.CQRS.Customers.Mappers;
using Customer_Mangment.Data;
using Customer_Mangment.Extensions;
using Customer_Mangment.Hubs;
using Customer_Mangment.Middlewares;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.OpenApi;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.Audit;
using Customer_Mangment.Repository.Services;
using Customer_Mangment.Repository.Services.AuditServices.MongoDB;
using Customer_Mangment.Repository.Services.Background;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json.Serialization;

namespace Customer_Mangment
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers()
                    .AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                        options.JsonSerializerOptions.WriteIndented = true;
                    });
            //OpenApi
            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
                options.AddOperationTransformer<BearerSecuritySchemeTransformer>();
            });
            //Global Exception Handler
            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
            //Wolverine
            builder.Host.AddWolverineMessaging(builder.Configuration);

            builder.Services.AddWolverineServices();

            //massaging
            builder.Services.AddMessaging(typeof(Program).Assembly);

            builder.Services.AddScoped<ISnapshotPublisher, SnapshotPublisher>();
            //DbContext and Identity
            builder.Services.AddDbContext<AppDbContext>(options =>
               options.UseSqlServer(
                   builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<User, IdentityRole>(
               options =>
               {
                   options.Password.RequireNonAlphanumeric = false;
               }
            ).AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
            //DI database and migration
            builder.Services.AddDataBaseConfig(builder.Configuration);
            builder.Services.AddScoped<IMigrationService, DataMigrationService>();

            //Hosted Service for Migration
            builder.Services.AddQuartzJobs(builder.Configuration);

            // SignalR
            builder.Services.AddSignalR();
            //LoggerMiddleware
            builder.Services.AddTransient<LoggerMiddleware>();

            //Authentication and Authorization
            builder.Services.AddAuthentication(option =>
            {
                option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(option =>
            {
                var jwtSettings = builder.Configuration.GetSection("Jwt");
                option.TokenValidationParameters = new()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!)),
                };
            });
            builder.Services.AddAuthorization();

            // Services
            builder.Services.AddScoped<ITokenProvider, TokenProvider>();
            builder.Services.AddScoped<IIdentityService, IdentityService>();
            builder.Services.AddScoped<ICustomerMapper, CustomerMapper>();
            builder.Services.AddHttpClient<RabbitMQ_ManagementController>(client =>
            {
                client.BaseAddress = new Uri("http://localhost:15672");
            });

            var app = builder.Build();

            await app.InitialiseDatabaseAsync();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "Customer Management API V1");

                    options.EnableDeepLinking();
                    options.DisplayRequestDuration();
                    options.EnableFilter();
                });

                app.MapScalarApiReference(options =>
                {
                    options.Title = "Customer Management API Reference";

                    options.WithTheme(Scalar.AspNetCore.ScalarTheme.DeepSpace);
                });
            }
            app.UseMiddleware<LoggerMiddleware>();
            app.UseExceptionHandler();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.MapHub<QueueMonitorHub>("/hubs/queue-monitor");

            app.Run();
        }
    }
}
