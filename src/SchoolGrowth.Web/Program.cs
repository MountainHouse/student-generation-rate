using System.Text.Json.Serialization;
using SchoolGrowth.Core;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalBlazor", policy => policy
        .WithOrigins(
            "http://localhost:5003",
            "https://localhost:7275",
            "http://localhost:5133",
            "https://localhost:5133")
        .AllowAnyHeader()
        .AllowAnyMethod());
});

var app = builder.Build();
var dataRoot = ResolveDataRoot(app.Environment.ContentRootPath);
var store = EnrollmentData.Load(dataRoot);
var model = EnrollmentModel.Calibrate(store);
var monteCarloModel = new MonteCarloEnrollmentModel(store);
var enableCrossOriginIsolationHeaders = builder.Configuration.GetValue("SchoolGrowth:EnableCrossOriginIsolationHeaders", true);

app.UseCors("LocalBlazor");
if (enableCrossOriginIsolationHeaders)
{
    app.Use(async (context, next) =>
    {
        context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
        context.Response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp";
        context.Response.Headers["Cross-Origin-Resource-Policy"] = "same-origin";
        await next();
    });
}
app.UseBlazorFrameworkFiles();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/summary", () => Results.Ok(new
{
    years = store.AugustYears,
    terms = store.Terms,
    neighborhoods = store.Neighborhoods,
    densities = store.Densities,
    latestTerm = store.Terms.LastOrDefault(),
    latestAugustYear = store.AugustYears.LastOrDefault(),
    gradeHistory = store.GradeRows,
    gridHistory = store.GridRows,
    schoolHistory = store.SchoolRows,
    homes = store.HomeRows,
    calibration = model.Calibration
}));

app.MapPost("/api/simulate", (SimulationRequest request) =>
{
    var result = model.Project(request);
    return Results.Ok(result);
});

app.MapGet("/api/simulation/status", () => Results.Ok(new
{
    available = true,
    mode = "server",
    maxDegreeOfParallelism = Environment.ProcessorCount
}));

app.MapPost("/api/simulation/validate", (MonteCarloValidationRequest request) =>
{
    var result = monteCarloModel.Validate(request);
    return Results.Ok(result);
});

app.MapPost("/api/simulation/search", (MonteCarloSearchRequest request) =>
{
    var result = monteCarloModel.FindBest(request);
    return Results.Ok(result);
});

app.MapFallbackToFile("index.html");

app.Run();

static string ResolveDataRoot(string contentRoot)
{
    var candidates = new[]
    {
        Path.Combine(contentRoot, "data"),
        Path.Combine(contentRoot, "..", "..", "data"),
        Path.Combine(contentRoot, "..", "..", "..", "data")
    };

    return candidates
        .Select(Path.GetFullPath)
        .First(path => File.Exists(Path.Combine(path, "homes.csv")));
}
