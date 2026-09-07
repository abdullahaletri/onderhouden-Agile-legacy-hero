using ExcellentTaste.Core.Data;
using ExcellentTaste.Core.Repositories;
using ExcellentTaste.Core.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton(new DatabaseConnection(builder.Configuration.GetConnectionString("ExcellentTaste")));
builder.Services.AddScoped<KlantRepository>();
builder.Services.AddScoped<ReserveringRepository>();
builder.Services.AddScoped<BestellingRepository>();
builder.Services.AddScoped<MenuRepository>();
builder.Services.AddScoped<OverzichtRepository>();
builder.Services.AddScoped<ReserveringService>();
builder.Services.AddScoped<BestellingService>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (DatabaseUnavailableException ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "De database is niet bereikbaar.");
        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        await context.Response.WriteAsJsonAsync(new
        {
            fout = "De database is niet bereikbaar.",
            bericht = "Neem contact op met de administrator."
        });
    }
});

try
{
    app.Services.GetRequiredService<DatabaseConnection>().Initialize();
}
catch (DatabaseUnavailableException ex)
{
    app.Logger.LogWarning(ex, "De API is gestart, maar de database is niet bereikbaar.");
}

app.MapGet("/", () => Results.Redirect("/api/status"));
app.MapGet("/api/status", (DatabaseConnection database) =>
{
    database.Initialize();
    return new
    {
        applicatie = "Excellent Taste API",
        status = "Actief",
        opmerking = "Gebruik de /api routes voor reserveringen, bestellingen, gegevens en overzichten."
    };
});
app.MapControllers();

app.Run();
