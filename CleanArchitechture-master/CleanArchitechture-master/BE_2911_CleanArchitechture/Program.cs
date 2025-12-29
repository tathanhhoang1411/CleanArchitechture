using CleanArchitecture.Application;
using CleanArchitecture.Application.Pipeline;
using CleanArchitecture.Application.Repository;
using CleanArchitecture.Infrastructure.Persistence;
using CleanArchitecture.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Serilog;
using BE_2911_CleanArchitechture.Handler;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AutoMapper;
using CleanArchitecture.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using BE_2911_CleanArchitechture.Logging;
using CleanArchitecture.Application.Interfaces;
using StackExchange.Redis;
using CleanArchitecture.Entites.Interfaces;
using BE_2911_CleanArchitechture.Filters;
using CleanArchitecture.Application.Services;
using BE_2911_CleanArchitechture.Hubs;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add services to the container.
builder.Services.AddControllers(options =>
{
    options.Filters.Add<PaginationValidationFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "TATHANHHOANG API", Version = "v1", Description = "tathanhhoang.work@gmail.com" });
    c.EnableAnnotations();
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter your token",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new string[] {}
        }
    });
});

// ✅ Database & Infrastructure
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("ConnectionString"), b => b.MigrationsAssembly("BE_2911_CleanArchitechture")));

// ✅ Optimize Redis Connection (Lazy)
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(builder.Configuration["Redis:ConnectionString"], true);
    configuration.AbortOnConnectFail = false; // Prevent crash on startup
    return ConnectionMultiplexer.Connect(configuration);
});
// Thêm class này vào cuối file Program.cs hoặc một file mới

builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddTransient<ApplicationContext, ApplicationContext>();
builder.Services.AddTransient<IReviewRepository, ReviewRepository>();
builder.Services.AddTransient<IReviewServices, ReviewServices>();
builder.Services.AddTransient<IProductRepository, ProductRepository>();
builder.Services.AddTransient<ICommentServices, CommentServices>();
builder.Services.AddTransient<ICommentRepository, CommentRepository>();
builder.Services.AddTransient<IProductServices, ProductServices>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<IUserServices, UserServices>();
builder.Services.AddTransient<IUserDetailRepository, UserDetailRepository>();
builder.Services.AddTransient<IUserDetailsServices, UserDetailsServices>();
builder.Services.AddTransient<IImageServices, ImageServices>();
builder.Services.AddTransient<IFriendRepository, FriendRepository>();
builder.Services.AddTransient<IFriendServices, FriendServices>();

// ✅ Chat & SignalR
builder.Services.AddTransient<IChatRepository, ChatRepository>();
builder.Services.AddTransient<IChatNotificationService, BE_2911_CleanArchitechture.Logging.ChatNotificationService>();
builder.Services.AddTransient<IChatServices, ChatServices>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>(); // <--- THÊM DÒNG NÀY

builder.Services.AddSingleton<ICustomLogger, CustomLogger>();
builder.Services.AddScoped<PaginationValidationFilter>();
builder.Services.AddApplicationMediaR();
builder.Services.AddAuthorizationCore();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy => policy.RequireRole("Admin"));
    options.AddPolicy("RequireUserRole", policy => policy.RequireRole("User"));
    options.AddPolicy("RequireAdminOrUserRole", policy => policy.RequireRole("Admin", "User"));
});

// ✅ Authentication & JWT Configuration (With SignalR Token Support)
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
        ValidateIssuer = true,
        ValidateAudience = true,
        ClockSkew = TimeSpan.Zero,
        ValidateLifetime = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };

    // Fix for SignalR authentication via Query String
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chatHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddMvc();
var automapper = new MapperConfiguration(item => item.AddProfile(new MappingProfile()));
IMapper mapper = automapper.CreateMapper();
builder.Services.AddSingleton(mapper);

// ✅ Correct CORS for SignalR (Specific Origin + Credentials)
var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() 
    ?? new[] { "http://localhost:5173" };

builder.Services.AddCors(options => options.AddPolicy("CorsPolicy",
    p => p.WithOrigins(allowedOrigins)
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials()
));

// ✅ Logging
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(DataAnnotationValidationBehavior<,>));

var app = builder.Build();

// ✅ Error Handling Middleware
app.Use(async (context, next) =>
{
    try { await next(); }
    catch (CleanArchitecture.Application.Utilities.ValidationException vex)
    {
        context.Response.StatusCode = 400;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(CleanArchitecture.Application.Utilities.ApiResponse<List<string>>.CreateErrorResponse(vex.Errors ?? new List<string>(), false));
    }
});

// ✅ Configure the HTTP request pipeline.
// ✅ Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaThanhHoang API V1"));

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();

// ✅ CORS MUST be before Authentication
app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    endpoints.MapHub<ChatHub>("/chatHub");
    
    // Redirect root URL to Swagger
    endpoints.MapGet("/", async context =>
    {
        context.Response.Redirect("/swagger/index.html");
        await Task.CompletedTask;
    });
});
app.Run();
public class CustomUserIdProvider : IUserIdProvider
{
    public string GetUserId(HubConnectionContext connection)
    {
        // SignalR sẽ tìm ID người dùng trong thuộc tính 'nameid' của Token
        return connection.User?.FindFirst("nameid")?.Value
            ?? connection.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    }
}