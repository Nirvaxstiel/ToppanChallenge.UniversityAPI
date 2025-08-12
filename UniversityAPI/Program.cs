using System.Reflection;
using System.Text;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using UniversityAPI.Framework;
using UniversityAPI.Framework.Database;
using UniversityAPI.Framework.Model.User;
using UniversityAPI.Middleware;
using UniversityAPI.Service;
using UniversityAPI.Utility;
using UniversityAPI.Utility.Interfaces;

var builder = WebApplication.CreateBuilder(args);

#region Service Registration

// OpenAPI/Swagger
builder.Services.AddOpenApi();

// Database
var dbConnection = builder.Configuration["TOPPAN_UNIVERSITYAPI_DB_CONNECTION"]
    ?? Environment.GetEnvironmentVariable("TOPPAN_UNIVERSITYAPI_DB_CONNECTION")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        dbConnection,
        b => b.MigrationsAssembly("UniversityAPI"));
});

// Identity
builder.Services.AddIdentity<UserDM, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;
    options.Password.RequiredUniqueChars = 5;
});

// Authentication (JWT)
var jwtKey = builder.Configuration["TOPPAN_UNIVERSITYAPI_JWT_KEY"]
    ?? Environment.GetEnvironmentVariable("TOPPAN_UNIVERSITYAPI_JWT_KEY")
    ?? builder.Configuration["Jwt:Key"];
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidAlgorithms = [SecurityAlgorithms.HmacSha512, SecurityAlgorithms.HmacSha512Signature]
    };
});

// Authorization
builder.Services.AddAuthorizationBuilder()
                .AddPolicy("RequireAdmin", policy => policy.RequireRole("Admin"))
                .AddPolicy("CanEditUniversity", policy =>
                {
                    policy.RequireAssertion(context => context.User.IsInRole("Admin")
                                                       || context.User.HasClaim("Permission", "EditUniversity"));
                });
// Utility/Service Layers
builder.Services.AddFrameworkLayer();
builder.Services.AddUtilityLayer();
builder.Services.AddServiceLayer();

// Rate Limiting, Caching, CORS
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins ?? Array.Empty<string>())
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Controllers & Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(config =>
{
    config.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "University API",
        Version = "v1",
        Description = "API for managing universities and bookmarks",
        Contact = new OpenApiContact
        {
            Name = "Your Name",
            Email = "your.email@example.com"
        }
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        config.IncludeXmlComments(xmlPath);
    }
    config.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: 'Authorization: Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    config.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});

#endregion Service Registration

var app = builder.Build();

#region Middleware Pipeline

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(config => config.SwaggerEndpoint("/swagger/v1/swagger.json", "University API V1"));
}

app.UseHttpsRedirection();
app.UseCors("DefaultPolicy");
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

#endregion Middleware Pipeline

#region Database Seeding

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<UserDM>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var configHelper = services.GetRequiredService<IConfigHelper>();
        configHelper.InjectStaticConfig();
        await Seed.SeedData(context, userManager, roleManager, configHelper);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during migration");
    }
}

#endregion Database Seeding

app.Run();