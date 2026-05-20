using System.Text.Json.Serialization;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;
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
var generatedDataRoot = Path.Combine(AppContext.BaseDirectory, "wwwroot", "data");
var blazorGeneratedWwwroot = ResolveBlazorGeneratedWwwroot(app.Environment.ContentRootPath, AppContext.BaseDirectory);
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
app.Use((context, next) =>
{
    if (ShouldDisableBrowserCache(context.Request.Path))
    {
        context.Response.OnStarting(() =>
        {
            context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate";
            context.Response.Headers.Pragma = "no-cache";
            context.Response.Headers.Expires = "0";
            return Task.CompletedTask;
        });
    }

    return next();
});
if (blazorGeneratedWwwroot is not null)
{
    app.UseStaticFiles(CreateBlazorStaticFileOptions(blazorGeneratedWwwroot));
}
app.UseBlazorFrameworkFiles();
app.UseDefaultFiles();
if (Directory.Exists(generatedDataRoot))
{
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(generatedDataRoot),
        RequestPath = "/data"
    });
}
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

static string? ResolveBlazorGeneratedWwwroot(string contentRoot, string baseDirectory)
{
    var targetFramework = Path.GetFileName(Path.GetFullPath(baseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)));
    var configuration = Directory.GetParent(Path.GetFullPath(baseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)))?.Name;
    if (string.IsNullOrWhiteSpace(configuration) || string.IsNullOrWhiteSpace(targetFramework))
    {
        return null;
    }

    var candidate = Path.GetFullPath(Path.Combine(
        contentRoot,
        "..",
        "SchoolGrowth.Blazor",
        "bin",
        configuration,
        targetFramework,
        "wwwroot"));

    return Directory.Exists(candidate) ? candidate : null;
}

static StaticFileOptions CreateBlazorStaticFileOptions(string root)
{
    var provider = new FileExtensionContentTypeProvider();
    provider.Mappings[".pdb"] = "application/octet-stream";

    return new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(root),
        ContentTypeProvider = provider,
        ServeUnknownFileTypes = true,
        DefaultContentType = "application/octet-stream"
    };
}

static bool ShouldDisableBrowserCache(PathString path)
{
    return path.StartsWithSegments("/_framework")
        || path.StartsWithSegments("/css")
        || path.StartsWithSegments("/js")
        || path.StartsWithSegments("/lib")
        || path == "/"
        || path == "/index.html"
        || path == "/coi-serviceworker.js"
        || path == "/service-worker-assets.js"
        || path.Value?.EndsWith(".styles.css", StringComparison.OrdinalIgnoreCase) == true;
}
