using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);
/********************************************************************************************/
//      Los siguientes extension methods ayudan Concetar todos los servicios requeridos
//      Mediante DI. Se agrega un Extension method por cada capa.
//      Application
//      Infrastructure
//      UserInterface
/********************************************************************************************/
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddWebUIServices();


// Luego de configurar todos los servicios se procede a crear la aplicación y configurar algunos de los
// servicios Agregados (si requiren configuracion.
var app = builder.Build();

// Se inicializa la base de datos, se corren las migraciones y el seeder.
await app.InitDatabase();

/*******************************************************************************************/
//  Las siguientes instrucciones configuran el pipeline de AspNet.Core, esto es un concepto 
//  muy importante que debe ser estudiado: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-6.0
//  la pipeline podrian entenderse como una "Chain of responsability", donde cada eslabon (aqui llamado Middleware)
//  Realiza su trabajo y pasa el control al siguiente.
/*******************************************************************************************/
if (!app.Environment.IsDevelopment())
{
    /*
     * HTTP Strict-Transport-Security (a menudo abreviado como HSTS (en-US)) es una característica de seguridad 
     * que permite a un sitio web indicar a los navegadores que sólo se debe comunicar con HTTPS en lugar de usar 
     * HTTP.
     * The default HSTS value is 30 days. You may want to change this for production scenarios, 
     * see https://aka.ms/aspnetcore-hsts.
     */
    app.UseHsts();
    //app.UseExceptionHandler("/Home/Error"); Chequear si lo necesitamos, porque ya tenemos un filtro de excepciones propio
}
else
{
    //app.UseDeveloperExceptionPage(); Chequear si lo necesitamos, porque ya tenemos un filtro de excepciones propio
    
    //en produccion, si la aplicación corre Kestrel atras de un Proxy http,
    //debemos configurar dicho proxy para forwardear estas cabeceras http.
    //y aqui debemo indicar que las cabeceras recibidas son en realidad las forwardeadas.
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });
    //Permite que el usuaroio ingrese www.<nuestro_dominio>, ver comentario interno.
    app.UseRedirectFromWWW();
}

//si el usuario usa http, se redirecciona a https, dado que la conexion entre el proxy y kestrel no requiere https,
//podemos obviarlo.
//app.UseHttpsRedirection();

//Habilita el uso de imagenes, librarias, css y todo contenido statico, por defecto, lo almacena en wwwroot.
app.UseStaticFiles();

//Agrega el middleware de rute que permite identificar controlador y accion en base a la url solicitada
app.UseRouting();
//Agrega el middleware de autorizacion que requiere el logeo de usuarios a las paginas marcadas con Authorize.
app.UseAuthentication();
//Una vez logueado aplica los controles de seguridad pertinentes si existen juntos con las anotaciones Authorize.
app.UseAuthorization();



//Configura el Ruteador, determina como se identifican controladores a partir de Urls, es usado en UseRouting.
app.UseEndpoints(endpoints =>
{
    endpoints.MapRazorPages(); // Requerido por las RazorPages de Identity.

    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

});

app.Run();
