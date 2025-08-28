using MapsterMapper;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using System.Reflection;

namespace SurveyBasket.Api;

public static class DependencyInjections
{
    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        services.AddControllers();

        services.AddSwaggerServices()
            .AddMapsterConfig()
            .AddFluentValidationConfig();




        services.AddScoped<IPollService, PollService>();

        

        return services;
    }

    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
    public static IServiceCollection AddMapsterConfig(this IServiceCollection services)
    {


        var mappingConfig = TypeAdapterConfig.GlobalSettings;
        mappingConfig.Scan(Assembly.GetExecutingAssembly());
        services.AddSingleton<IMapper>(new Mapper(mappingConfig));

        return services;

    } 
    
    public static IServiceCollection AddFluentValidationConfig(this IServiceCollection services)
    {

        
        services
            .AddFluentValidationAutoValidation()
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
       


        return services;

    }

}

