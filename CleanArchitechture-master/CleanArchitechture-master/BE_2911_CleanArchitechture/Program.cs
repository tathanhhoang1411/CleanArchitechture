using CleanArchitecture.Application;
using CleanArchitecture.Application.IRepository;
using CleanArchitecture.Application.Repository;
using CleanArchitecture.Infrastructure.DBContext;
using CleanArchitecture.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using BE_2911_CleanArchitechture.Handler;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
//using CleanArchitecture.Application.IServices;
//using CleanArchitecture.Application.Services;
using AutoMapper;
using CleanArchitecture.Application.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(
                            builder.Configuration.GetConnectionString("ConnectionString"), b => b.MigrationsAssembly("BE_2911_CleanArchitechture")));

builder.Services.AddTransient<ApplicationContext, ApplicationContext>();
builder.Services.AddTransient<IProductRepository, ProductRepository>();
builder.Services.AddTransient<IProductServices, ProductServices>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IUserServices, UserService>();
builder.Services.AddApplicationMediaR();
builder.Services.AddAuthorizationCore();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));
});
// Thêm dịch vụ xác thực JWT
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.IncludeErrorDetails = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {

            ValidateIssuerSigningKey = true,
            ValidateIssuer = true, // Có thể cấu hình theo nhu cầu
            ValidateAudience = true, // Có thể cấu hình theo nhu cầu
            ClockSkew = TimeSpan.Zero, // Không cho phép thời gian trễ
            ValidateLifetime = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });


builder.Services.AddMvc();
var automapper = new MapperConfiguration(item => item.AddProfile(new MappingProfile()));
IMapper mapper = automapper.CreateMapper();
builder.Services.AddSingleton(mapper);
//builder.Services.AddCors(options =>
//{
//    options.AddDefaultPolicy(policy =>
//    {
//        //policy.WithOrigins("http://localhost:4200");
//        policy.AllowAnyOrigin();
//        policy.WithMethods("GET", "POST", "DELETE", "PUT");
//        policy.AllowAnyHeader(); // <--- list the allowed headers here
//        policy.AllowAnyOrigin();
//    });
//});
builder.Services.AddCors(options => options.AddPolicy("CorsPolicy",
    builder => builder.AllowAnyOrigin()
    .WithMethods("GET", "POST", "PUT", "DELETE")
    .AllowAnyHeader()
    .AllowCredentials()
    .Build()
));
var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
if (!Directory.Exists(logDirectory))
{
    Directory.CreateDirectory(logDirectory);
}

var _logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.File(Path.Combine(logDirectory, "ApiLog-.log"), rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Logging.AddSerilog(_logger);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
app.Run();
