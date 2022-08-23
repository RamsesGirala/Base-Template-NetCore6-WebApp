using System.Reflection;
using test.Application.Common.Behaviours;
using FluentValidation;
using MediatR;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        //Busca todos los mapeos realizados y los registra en el DI.
        services.AddAutoMapper(Assembly.GetExecutingAssembly());
        //Busca todas las clases validadoras y las registra en el DI
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        //Identifica todos las clases IRequest (Commands and Queries) de la dll y los asocia con sus handlers
        services.AddMediatR(Assembly.GetExecutingAssembly());

        /*******************************************************************
         * Cuando usamos mediator, se activa un 2do pipeline interno de mediator, donde cada eslabon
         * de esa cadena se configura aqui, cada eslabon de la cadena llama al siguiente eslabon.
         * Analizar el codigo de cada uno de estos behaviors da un gran entendimiento del 
         * funcionamiento interno de la aplicacion.
         * 
         * nota: importante diferneciar el Autorize Attribute usado en controladores
         *       del usado en los Request CQRS 
         * *******************************************************************/
        //TODO:Revisar si se ejecuta el login behavior, sino agregar aca.
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));

        return services;
    }
}
