using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text;
using StackExchange.Redis;

using Auction.API.Data;                 // AppDbContext
using AuctionApp.Models;                // Identity User, entities
using Auction.API.Hubs;                 // BiddingHub

var builder = WebApplication.CreateBuilder(args);

// DbContext (PostgreSQL) 
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Background Hosted Service 
builder.Services.AddHostedService<Auction.API.Services.AutoFinalizeAuctionsService>();

//Identity (User + Role)
builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

//CORS (Vue dev server & websockets)
const string AllowDev = "_allowDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: AllowDev, policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",
                "https://localhost:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // required for SignalR with JWT over websockets
    });
});

//Authentication (JWT Bearer)
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSection = builder.Configuration.GetSection("Jwt");

    var key = jwtSection["Key"];
    if (string.IsNullOrWhiteSpace(key))
        throw new InvalidOperationException("JWT:Key is missing. Add it to appsettings.json or user secrets.");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer           = true,
        ValidateAudience         = true,
        ValidateLifetime         = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer              = jwtSection["Issuer"],
        ValidAudience            = jwtSection["Audience"],
        IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

// SignalR 
var signalRBuilder = builder.Services.AddSignalR()
    .AddStackExchangeRedis("localhost:6379", options =>
    {
        options.Configuration.ChannelPrefix = "AuctionHub";
    });

var redisConn = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
if (!string.IsNullOrWhiteSpace(redisConn))
{
    signalRBuilder.AddStackExchangeRedis(redisConn, options =>
    {
        options.Configuration.ChannelPrefix = new RedisChannel("auction", RedisChannel.PatternMode.Literal);
    });
}

// Swagger w/ JWT support 
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Auction API",
        Version = "v1",
        Description = "Online Auction System API with JWT Authentication and SignalR"
    });

    var jwtSecurityScheme = new OpenApiSecurityScheme
    {
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Description = "Enter 'Bearer <token>'",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };

    options.AddSecurityDefinition(jwtSecurityScheme.Reference.Id, jwtSecurityScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        { jwtSecurityScheme, Array.Empty<string>() }
    });

    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

builder.Services.AddControllers();

var app = builder.Build();

// Seed roles & initial data 
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        await SeedData.InitializeAsync(services);

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = new[] { "Admin", "Seller", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Seed");
        logger.LogError(ex, "Error while seeding the database.");
    }
}

// Middleware pipeline
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors(AllowDev);        // CORS BEFORE auth for SignalR

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// SignalR hubs
app.MapHub<BiddingHub>("/Hubs/BiddingHub");

app.Run();

