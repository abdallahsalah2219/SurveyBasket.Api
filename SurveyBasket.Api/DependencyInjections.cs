using Asp.Versioning;
using Hangfire;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using SurveyBasket.Api.Authentication;
using SurveyBasket.Api.Health;
using SurveyBasket.Api.Services.AuthService;
using SurveyBasket.Api.Services.EmailService;
using SurveyBasket.Api.Services.NotificationService;
using SurveyBasket.Api.Services.ResultService;
using SurveyBasket.Api.Services.RoleService;
using SurveyBasket.Api.Services.UserService;
using SurveyBasket.Api.Services.VoteService;
using SurveyBasket.Api.Settings;
using SurveyBasket.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;

namespace SurveyBasket.Api;

public static class DependencyInjections
{
    public static IServiceCollection AddDependencies(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddControllers();

        // Add Service Of HybridCache
        services.AddHybridCache();

        services.AddCors(options =>
        options.AddDefaultPolicy( builder =>
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader()      
        )
        );


        services.AddAuthConfig(configuration);
        #region Add DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection") ??
                throw new InvalidOperationException("Connection String 'DefaultConnection' not found!"));
        });
        #endregion

        services.AddSwaggerServices()
            .AddMapsterConfig()
            .AddFluentValidationConfig()
            .AddRateLimitingConfig();

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IPollService, PollService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IVoteService, VoteService>();
        services.AddScoped<IResultService, ResultService>();
        services.AddScoped<IEmailSender, EmailService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IRoleService, RoleService>();


        //  If I Want to Use  DistributedMemoryCache
        //services.AddScoped<ICacheService, CacheService>();

        // Add ExceptionHandler service
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        // Add Background Jobs Configuration
        services.AddBackgroundJobsConfig(configuration);

        // HttpContext Accessor
        services.AddHttpContextAccessor();

        // Mail Settings Configuration
        services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));

        // Health Checks Configuration service and DbContext Check and Hangfire Check and Mail Service Check
        services.AddHealthChecks()
            .AddSqlServer(name:"database",connectionString:configuration.GetConnectionString("DefaultConnection")!)
            .AddHangfire(optionx => { optionx.MinimumAvailableServers = 1; })
            .AddCheck<MailProviderHealthCheck>(name:"Mail Service");

        // Add ApiVersioning Configuration
        services.AddApiVersioning(options => 
        {
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options => 
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }

    private static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            //options.SwaggerDoc("v1", new OpenApiInfo
            //{
            //    Version = "v1",
            //    Title = "ToDo API",
            //    Description = "An ASP.NET Core Web API for managing ToDo items",
            //    TermsOfService = new Uri("https://example.com/terms"),
            //    Contact = new OpenApiContact
            //    {
            //        Name = "Example Contact",
            //        Url = new Uri("https://example.com/contact")
            //    },
            //    License = new OpenApiLicense
            //    {
            //        Name = "Example License",
            //        Url = new Uri("https://example.com/license")
            //    }
            //});

            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

            options.OperationFilter<SwaggerDefaultValues>();
        });

        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        return services;
    }
    private static IServiceCollection AddMapsterConfig(this IServiceCollection services)
    {


        var mappingConfig = TypeAdapterConfig.GlobalSettings;
        mappingConfig.Scan(Assembly.GetExecutingAssembly());
        services.AddSingleton<IMapper>(new Mapper(mappingConfig));


        
        return services;

    }

    private static IServiceCollection AddFluentValidationConfig(this IServiceCollection services)
    {


        services
            .AddFluentValidationAutoValidation()
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());



        return services;

    }
    // Add Authentication Configuration
    private static IServiceCollection AddAuthConfig(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();


        // Jwt 
        services.AddSingleton<IJwtProvider, JwtProvider>();

        //services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));


        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var jwtSettings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();



        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
         .AddJwtBearer(o =>
         {
             o.SaveToken = true;
             o.TokenValidationParameters = new TokenValidationParameters
             {
                 ValidateIssuerSigningKey = true,
                 ValidateIssuer = true,
                 ValidateAudience = true,
                 ValidateLifetime = true,
                 IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Key!)),
                 ValidIssuer = jwtSettings?.Issuer,
                 ValidAudience = jwtSettings?.Audience,
             };
         });

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequiredLength = 8;
            options.SignIn.RequireConfirmedEmail = true;
            options.User.RequireUniqueEmail = true;

        });

        return services;

    }
    // Add Hangfire Background Jobs Configuration
    private static IServiceCollection AddBackgroundJobsConfig(this IServiceCollection services, IConfiguration configuration)
    {

        // Add Hangfire services.
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection")));

        // Add the processing server as IHostedService
        services.AddHangfireServer();

        return services;
    }
    private static IServiceCollection AddRateLimitingConfig(this IServiceCollection services)
    {
        // Add Rate Limiter Configuration
        services.AddRateLimiter(rateLimiterOptions =>
        {
            // Rejection Status Code Configuration (429 Too Many Requests) 
            rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // IP Based Rate Limiting Policy
            rateLimiterOptions.AddPolicy("ipLimit", httpContext =>
                     RateLimitPartition.GetFixedWindowLimiter
                     (
                         partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
                         factory: _ => new FixedWindowRateLimiterOptions
                         {
                             PermitLimit = 2,
                             Window = TimeSpan.FromSeconds(20),

                         }
                     )
            );

            // User Limit Configuration 
            rateLimiterOptions.AddPolicy("userLimit", httpContext =>
                     RateLimitPartition.GetFixedWindowLimiter
                     (
                         partitionKey: httpContext.User.GetUserId(),
                         factory: _ => new FixedWindowRateLimiterOptions
                         {
                             PermitLimit = 2,
                             Window = TimeSpan.FromSeconds(20),

                         }
                     )
            );

            // Concurrency Limiter Configuration
            rateLimiterOptions.AddConcurrencyLimiter("concurrency", options =>
            {
                options.PermitLimit = 1000;
                options.QueueLimit = 100;
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });


            #region Token Bucket Limiter Configuration
            //rateLimiterOptions.AddTokenBucketLimiter("token", options => 
            //{
            //    options.TokenLimit = 100;
            //    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            //    options.QueueLimit = 50;
            //    options.ReplenishmentPeriod = TimeSpan.FromSeconds(30);
            //    options.TokensPerPeriod = 2;
            //    options.AutoReplenishment = true;
            //});
            #endregion

            #region Fixed Window Limiter Configuration
            //rateLimiterOptions.AddFixedWindowLimiter("fixed", options =>
            //{
            //    options.PermitLimit = 100;
            //    options.QueueLimit = 50;
            //    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            //    options.Window = TimeSpan.FromMinutes(1);
            //});
            #endregion

            #region Sliding Window Limiter Configuration
            //rateLimiterOptions.AddSlidingWindowLimiter("sliding", options =>
            //{
            //    options.PermitLimit = 100;
            //    options.QueueLimit = 50;
            //    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            //    options.Window = TimeSpan.FromSeconds(20);
            //    options.SegmentsPerWindow = 2; // 2 segments of 10 seconds each
            //});
            #endregion

        });
        return services;
    }

}

