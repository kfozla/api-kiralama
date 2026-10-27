using System.Text;
using System.Text.Json.Serialization;
using api_kiralama.Data;
using api_kiralama.Repositories;
using api_kiralama.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace api_kiralama;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode =
                StatusCodes.Status429TooManyRequests;

            options.AddPolicy("login", context =>
            {
                var ip = context.Connection.RemoteIpAddress?
                    .MapToIPv4()
                    .ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"login:{ip}",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }
                );
            });
            
            options.AddPolicy("refresh", context =>
            {
                var ip = context.Connection.RemoteIpAddress?
                    .MapToIPv4()
                    .ToString() ?? "unknown";

                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: $"refresh:{ip}",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }
                );
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode =
                    StatusCodes.Status429TooManyRequests;

                context.HttpContext.Response.ContentType =
                    "application/json";

                context.HttpContext.Response.Headers.RetryAfter = "60";

                await context.HttpContext.Response.WriteAsJsonAsync(
                    new
                    {
                        message = "Çok fazla giriş denemesi. Lütfen biraz sonra tekrar deneyin."
                    },
                    cancellationToken
                );
            };
        });
        

        
        builder.Services.AddAuthorization();
        

        builder.Services.AddScoped<UserService>();
        builder.Services.AddScoped<JwtService>();
        builder.Services.AddScoped<EmlakSahibiService>();
        builder.Services.AddScoped<EmlakOzellikService>();
        builder.Services.AddScoped<EmlakService>();
        builder.Services.AddScoped<BolgeService>();
        
        builder.Services.AddScoped<UserRepository>();
        builder.Services.AddScoped<EmlakSahibiRepository>();
        builder.Services.AddScoped<EmlakOzellikRepository>();
        builder.Services.AddScoped<EmlakRepository>();
        builder.Services.AddScoped<BolgeRepository>();
        
        
        builder.Services.AddScoped<EmlakFiyatRepository>();
        builder.Services.AddScoped<EmlakBlokeTarihRepository>();
        builder.Services.AddScoped<RezervasyonRepository>();
        builder.Services.AddScoped<EmlakTakvimRepository>();
        builder.Services.AddScoped<IsletmeAyarlariRepository>();
        builder.Services.AddScoped<EmlakFotografRepository>();
        builder.Services.AddScoped<AnasayfaBolumRepository>();


        builder.Services.AddScoped<EmlakTakvimLock>();
        builder.Services.AddScoped<EmlakFiyatService>();
        builder.Services.AddScoped<EmlakBlokeTarihService>();
        builder.Services.AddScoped<RezervasyonService>();
        builder.Services.AddScoped<EmlakTakvimService>();
        builder.Services.AddScoped<IsletmeAyarlariService>();
        builder.Services.AddScoped<DashboardService>();
        builder.Services.AddScoped<EmlakFotografService>();
        
        
        builder.Services.AddScoped<HtmlContentService>();

        builder.Services.AddScoped<SayfaIcerigiService>();

        builder.Services.AddScoped<SikcaSorulanSoruService>();

        builder.Services.AddScoped<BankaHesabiService>();
        
        
        builder.Services.AddScoped<AnasayfaBolumService>();

       
        
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });
        

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            
        
            c.SwaggerDoc("v1", new() { Title = "api_kiralama", Version = "v1" });
            
            c.AddSecurityDefinition("Bearer",new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\""
            });
            
            c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
            {
                {
                    new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                    {
                        Reference = new Microsoft.OpenApi.Models.OpenApiReference
                        {
                            Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] {}
                }
            });
            
        });
        //JWT
        var jwtSettings = builder.Configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Secret"];

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
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ClockSkew = TimeSpan.Zero
                };
            });

        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy", policyBuilder =>
                policyBuilder
                    .WithOrigins(
                        "http://localhost:3000",
                        "https://admin.goksoft.com"
                    )
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
        });
        
        var app = builder.Build();
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        
        

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseCors("CorsPolicy");
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        
        
        app.MapControllers();
        app.Run();
        
    }
}