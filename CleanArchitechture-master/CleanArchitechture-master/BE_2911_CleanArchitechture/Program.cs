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
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using BE_2911_CleanArchitechture.Logging;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TATHANHHOANG API", Version = "v1", Description= "tathanhhoang.work@gmail.com" });
    c.EnableAnnotations(); // Kích hoạt Annotations
});
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(
                            builder.Configuration.GetConnectionString("ConnectionString"), b => b.MigrationsAssembly("BE_2911_CleanArchitechture")));

builder.Services.AddTransient<ApplicationContext, ApplicationContext>();
builder.Services.AddTransient<IProductRepository, ProductRepository>();
builder.Services.AddTransient<IProductServices, ProductServices>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IUserServices, UserService>();
builder.Services.AddTransient<IimageServices, ImageServices>();
builder.Services.AddSingleton<ICustomLogger, CustomLogger>(); // Đăng ký CustomLogger
builder.Services.AddApplicationMediaR();
builder.Services.AddAuthorizationCore();
builder.Services.AddAuthorization(options =>
{
    //Role admin
    options.AddPolicy("RequireAdminRole", policy => 
    policy.RequireRole("Admin"));
    //Role User
    options.AddPolicy("RequireUserRole", policy => 
    policy.RequireRole("User"));
    //Role admin, user
    options.AddPolicy("RequireAdminOrUserRole", policy =>
    policy.RequireRole("Admin", "User"));
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
// Cấu hình Serilog
var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
if (!Directory.Exists(logDirectory))
{
    Directory.CreateDirectory(logDirectory);
}


Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Logging.ClearProviders(); // Xóa các logger mặc định
builder.Logging.AddSerilog(); // Thêm Serilog



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaThanhHoang API V1");
    });
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
