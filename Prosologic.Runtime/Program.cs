using Prosologic.Runtime;
using Prosologic.Runtime.Services;

Console.WriteLine("Prosologic PLC Runtime");
Console.WriteLine("=====================");
        
// Get project path from args, env var, or default
var projectPath = args.Length > 0 
    ? args[0] 
    : Environment.GetEnvironmentVariable("PROJECT_PATH") 
      ?? "/app/project";
        
if (!Path.IsPathRooted(projectPath))
{
    projectPath = Path.GetFullPath(projectPath);
}
        
Console.WriteLine($"Project path: {projectPath}");
        
if (!Directory.Exists(projectPath))
{
    Console.WriteLine($"ERROR: Project directory not found: {projectPath}");
    Console.WriteLine($"Usage: dotnet run <project-path>");
    Console.WriteLine($"   or: Set PROJECT_PATH environment variable");
    Console.WriteLine($"   or: Mount project to /app/project in Docker");
    return;
}

var builder = Host.CreateApplicationBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(
    builder.Environment.IsDevelopment() 
        ? LogLevel.Debug 
        : LogLevel.Information
);

// Register services
builder.Services.AddSingleton(new RuntimeOptions { ProjectPath = projectPath });
builder.Services.AddSingleton<ScriptExecutor>();
builder.Services.AddSingleton<IMqttPublisher, MqttPublisher>();
builder.Services.AddHostedService<RuntimeService>();
builder.Services.AddSingleton<IProjectLoader, ProjectLoader>();
builder.Services.AddSingleton<ITagEngine, TagEngine>();

var host = builder.Build();
host.Run();