using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using test.Application.Common.Interfaces;
using test.Infrastructure.Common;
using test.Infrastructure.Identity;
using test.Infrastructure.Persistence;
using test.Infrastructure.Persistence.Interceptors;
using test.Infrastructure.Services;

namespace Microsoft.Extensions.DependencyInjection;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        //Registra en el DI, la clase que gestiona el salvado automatico de campos de Entidades Auditables.
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        if (configuration.GetValue<bool>("UseInMemoryDatabase"))
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("testDb"));
        }
        else
        {
            //Configura MySQL para EF
            var serverVersion = ServerVersion.Parse(configuration.ServerInfo());

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(configuration.GetConnectionString("DefaultConnection"),
                serverVersion,
                    builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName))
                .UseLazyLoadingProxies());
        }
        // Registra el Contexto de EF.
        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        //Registra el Seeder
        services.AddScoped<ApplicationDbContextInitialiser>();

        //Configura Identity Services para su uso con nuestro contexto.
        services
            .AddDefaultIdentity<ApplicationUser>() //Identifica la clase que describe los usuarios de la aplicacion.
                                                   //Notar que esta clase no esta en nuestro contexto, Analizar esto detenidamente.
            .AddRoles<IdentityRole>()   //Identifica la clase que describe los roles de la aplicacion
            .AddEntityFrameworkStores<ApplicationDbContext>()   //Permite almacenar lso datos de Identity en nuestra base
            .AddDefaultTokenProviders(); //Adds the default token providers used to generate tokens for reset passwords, change email and change telephone number operations, and for two factor authentication token generation.
            //.AddClaimsPrincipalFactory //Habilitar en caso de necesitar customizar los claims incluidos en el token jwt.


        //Configura las cookies utilizadas en el sistema, recordar que el token jwt es almacenado en el cliente
        //en una cookie.
        services.ConfigureApplicationCookie(options =>
                        {
                            options.Cookie.Name = "junkode_cookie";
                            options.LoginPath = "/Identity/Account/Login";
                            options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                            options.LogoutPath = "/Identity/Account/Logout";
                            options.ReturnUrlParameter = "returnUrl";
                        }); ;

        //Este sevicio, obtiene fecha y hora actual y es util solo si se lo requiere mockear en tests.
        services.AddTransient<IDateTime, DateTimeService>();

        //Registra servicio general de consulta para seguridad
        services.AddTransient<IIdentityService, IdentityService>();

        return services;
    }
}
