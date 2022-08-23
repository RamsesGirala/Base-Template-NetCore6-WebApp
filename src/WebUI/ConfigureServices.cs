using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Rewrite;
using test.Application.Common.Interfaces;
using test.Infrastructure.Persistence;
using WebUI.Common;
using WebUI.Filters;
using WebUI.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddWebUIServices(this IServiceCollection services)
    {
        services.AddDatabaseDeveloperPageExceptionFilter();

        //Servicio fundamental para obtener el usuario actual, extrae el Id del usuario actual del token jwt.
        services.AddSingleton<ICurrentUserService, CurrentUserService>();
        //Registra este servicio utilizado internamente por ICurrentUserService
        services.AddHttpContextAccessor();

        //Agrega los servicios requeridos para manejar Razor Pages, las paginas de Identity son RazorPages
        services.AddRazorPages();

        //Agregar soporte para MVC, configura un filtro que estandariza la forma en que los errores son
        //entregados al cliente. muy util para unificar ajax calls.
        services.AddControllersWithViews(options =>
        {
            options.Filters.Add(new ExceptionFilter(services));
            //Estas lineas permite procesar los valores procedentes de request tipo post con datos en el From
            //como "culture.invariant", netcore por defecto toma el current culture para procesar esto,
            //por lo que la coma decimal puede generar problemas para bindear datos,
            //ver que pasa con las fechas...
            // Leer => https://docs.microsoft.com/en-us/aspnet/core/mvc/models/model-binding?view=aspnetcore-5.0
            var index = options.ValueProviderFactories.IndexOf(options.ValueProviderFactories.OfType<FormValueProviderFactory>().Single());
            options.ValueProviderFactories[index] = new InvariantFormValueProviderFactory();
        }).AddRazorRuntimeCompilation(); //Permite modificar paginas cshtml sin recompilar toda la app.

        return services;
    }


    public static async Task InitDatabase(this WebApplication app)
    {
        // Initialise and seed database
        using (var scope = app.Services.CreateScope())
        {
            var initialiser = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitialiser>();
            await initialiser.InitialiseAsync(); //<<--Chequea si se requieren migraciones y las ejecuta
            await initialiser.SeedAsync(); //<--Ejecuta el seeder.
        }
    }
    public static void UseRedirectFromWWW(this WebApplication app)
    {

        //If the use enter www.domain.com, will be redirected automatically to domain.com (whithou www.)
        app.UseRewriter(new RewriteOptions().AddRedirectToNonWwwPermanent());

    }
}
