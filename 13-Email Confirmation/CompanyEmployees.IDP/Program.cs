using CompanyEmployees.IDP;
using CompanyEmployees.IDP.InitialSeed;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, lc) => lc
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}")
        .Enrich.FromLogContext()
        .ReadFrom.Configuration(ctx.Configuration));

    var app = builder
        .ConfigureServices()
        .ConfigurePipeline();

    var config = app.Services.GetRequiredService<IConfiguration>();
    var connectionString = config.GetConnectionString("identitySqlConnection");
    SeedUserData.EnsureSeedData(connectionString);

    app.MigrateDatabase()
        .Run();
}
catch (Exception ex)
{
    if (ex.GetType().Name != "HostAbortedException")
        Log.Fatal(ex, $"Unhandled exception {ex.GetType().Name}");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}