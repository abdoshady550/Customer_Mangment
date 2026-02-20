using Customer_Mangment.Behaviors;
using Customer_Mangment.Data;
using Customer_Mangment.Middleware;
using Customer_Mangment.Model.Entities;
using Customer_Mangment.OpenApi;
using Customer_Mangment.Repository;
using Customer_Mangment.Repository.Interfaces;
using Customer_Mangment.Repository.Services;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Reflection;
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

            builder.Services.AddOpenApi(options =>
            {
                options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
                options.AddOperationTransformer<BearerSecuritySchemeTransformer>();
            });
            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();


            builder.Services.AddMediatR(optoin =>
            {
                optoin.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            });

            builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            builder.Services.AddAutoMapper(optoin => optoin.AddMaps(Assembly.GetExecutingAssembly()));

            builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            builder.Services.AddDbContext<AppDbContext>
            (option => option.UseSqlServer((builder.Configuration.GetConnectionString("DefaultConnection"))));

            builder.Services.AddIdentity<User, IdentityRole>(
               options =>
               {
                   options.Password.RequireNonAlphanumeric = false;
               }
            ).AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();
            builder.Services.AddScoped<ApplicationDbContextInitialiser>();


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


            builder.Services.AddScoped(typeof(IGenericRepo<>), typeof(GenericRepo<>));
            builder.Services.AddScoped<ITokenProvider, TokenProvider>();
            builder.Services.AddScoped<IIdentityService, IdentityService>();


            var app = builder.Build();

            await app.InitialiseDatabaseAsync();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI();
                app.MapScalarApiReference();
            }
            app.UseExceptionHandler();

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();



            app.Run();
        }
    }
}
