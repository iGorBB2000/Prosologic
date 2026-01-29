using Prosologic.Runtime;
using Prosologic.Runtime.Services;

Console.WriteLine("Prosologic PLC Runtime");
Console.WriteLine("=====================");
        
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
    Console.WriteLine($"   or: dotnet run -- <project-path>");
    return;
}

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(new RuntimeOptions { ProjectPath = projectPath });
builder.Services.AddSingleton<ScriptExecutor>();
builder.Services.AddSingleton<IMqttPublisher, MqttPublisher>();
builder.Services.AddHostedService<RuntimeService>();
builder.Services.AddSingleton<IProjectLoader, ProjectLoader>();
builder.Services.AddSingleton<ITagEngine, TagEngine>();

var host = builder.Build();
host.Run();