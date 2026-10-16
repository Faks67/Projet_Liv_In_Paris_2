using System.Text.Json;
using LivInParis.Web.Api;
using LivInParis.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(new Db(builder.Configuration.GetConnectionString("LivInParis")!));
builder.Services.AddSingleton(new Metro(Path.Combine(AppContext.BaseDirectory, "Metro_GareFinal.xlsx")));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "livinparis";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        // API : on renvoie 401/403 au lieu de rediriger vers une page de connexion
        options.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = 401; return Task.CompletedTask; };
        options.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = 403; return Task.CompletedTask; };
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("client", p => p.RequireClaim(Session.Client));
    options.AddPolicy("cuisinier", p => p.RequireClaim(Session.Cuisinier));
    options.AddPolicy("admin", p => p.RequireClaim(Session.Admin));
});
builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase);

var app = builder.Build();

try
{
    await app.Services.GetRequiredService<Db>().MettreAJourSchemaAsync();
}
catch (MySqlException ex)
{
    app.Logger.LogError("Base MySQL inaccessible ({Message}). Lancez demarrer_mysql.cmd puis relancez le site.", ex.Message);
}

// Les erreurs SQL inattendues deviennent un message lisible côté interface
app.Use(async (ctx, next) =>
{
    try
    {
        await next();
    }
    catch (MySqlException ex) when (!ctx.Response.HasStarted)
    {
        app.Logger.LogError(ex, "Erreur MySQL");
        ctx.Response.StatusCode = 503;
        await ctx.Response.WriteAsJsonAsync(new { erreur = "La base de données ne répond pas. Vérifiez que demarrer_mysql.cmd est lancé." });
    }
});

app.UseDefaultFiles();
// Le navigateur revalide les fichiers du site à chaque visite (ETag) : une mise à jour est vue tout de suite
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "no-cache"
});
app.UseAuthentication();
app.UseAuthorization();

app.MapSessionApi();
app.MapMetroApi();
app.MapPlatsApi();
app.MapCommandesApi();
app.MapCuisinierApi();
app.MapAdminApi();

app.Run();
