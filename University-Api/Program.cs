using Autofac.Extensions.DependencyInjection;
using EventBus.Abstructions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using RabbitMqCustomLib;
using System.Text;
using UniversityApi.Data;
using UniversityApi.Extensions;
using UniversityApi.Grpc;
using UniversityApi.Infrastructure.UnitOfWork;
using UniversityApi.IntegrationEvents.Events;
using UniversityApi.Services.CryptoService;
using UniversityApi.Services.FacultieService;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcServices(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddSingleton<ICryptoService,  CryptoService>();
builder.Services.AddSingleton<IFacultieService, FacultieService>();
builder.Services.AddGrpcHealthChecks(o =>
{
    o.Services.MapService("", r => r.Tags.Contains("public"));
});
builder.Services.AddEventBus(builder.Configuration);
builder.Services.AddIntegrationServices(builder.Configuration);
builder.Services.AddDataProtection().SetApplicationName("Chat")
            .AddKeyManagementOptions(options =>
            {
                options.AutoGenerateKeys = false;
                
            });

builder.Services.AddDbContext<ApplicationDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration["AppSettings:MSS"]);
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {

        ValidateIssuerSigningKey = false,
        ValidateIssuer = false,
        ValidateAudience = false,
        RequireExpirationTime = false,
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]))
    };
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy",
        builder => builder
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed((host) => true)
            .AllowAnyHeader());
});


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");


app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
app.MapControllers();

app.MapGrpcService<UniversityService>();
app.MapGrpcService<StudentGrpcServer>();

app.MapGrpcHealthChecksService();

app.MapGrpcService<DeanServicesGrpc>();


app.Run();


