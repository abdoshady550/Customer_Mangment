using Customer_Mangment.CQRS.Customers.Mappers;
using Customer_Mangment.Data;
using Customer_Mangment.Middlewares;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.OpenApi;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Interfaces.AppMediator;
using Customer_Mangment.Repository.Services;
using Customer_Mangment.Repository.Services.AppMediator;
using Customer_Mangment.Repository.Services.Background;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Scalar.AspNetCore;
using System.Text;
using System.Text.Json.Serialization;
using Wolverine;
using Wolverine.FluentValidation;

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
            builder.Host.UseWolverine(opts =>
            {
                opts.Discovery.IncludeAssembly(typeof(IAssmblyMarker).Assembly);

                opts.UseFluentValidation();

                opts.UseFluentValidation(RegistrationBehavior.ExplicitRegistration);
            });
            builder.Services.AddScoped<IDispatcher, AppDispatcher>();

            builder.Services.AddValidatorsFromAssembly(typeof(IAssmblyMarker).Assembly);
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
            //DI RabbitMQ
            builder.Services.AddMassTransitWithRabbitMq(builder.Configuration);

            //Hosted Service for Migration
            builder.Services.AddQuartz(q =>
            {
                q.SchedulerId = "AUTO";
                q.SchedulerName = "CustomerMgmtScheduler";

                q.UseDefaultThreadPool(tp => tp.MaxConcurrency = 5);

                q.UsePersistentStore(store =>
                {
                    store.UseProperties = true;
                    store.RetryInterval = TimeSpan.FromMinutes(1);

                    store.UseSqlServer(sql =>
                    {
                        sql.ConnectionString = builder.Configuration
                            .GetConnectionString("DefaultConnection")!;

                        sql.TablePrefix = "QRTZ_";
                    });

                    store.UseNewtonsoftJsonSerializer();
                });

                var jobKey = new JobKey("MigrationJob");

                q.AddJob<MigrationJob>(opts => opts
                    .WithIdentity(jobKey)
                    .StoreDurably());

                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("MigrationJob-trigger", "MigrationGroup")
                    .WithCronSchedule("0 * * ? * *")
                    .StartNow());
            });
            builder.Services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
                options.AwaitApplicationStarted = true;

            });

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



            app.Run();
        }
    }
}
