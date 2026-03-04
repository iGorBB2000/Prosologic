using System.Text.Json;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Prosologic.Runtime.Services;

public class ScriptExecutor
{
    private readonly ILogger<ScriptExecutor> _logger;
    private readonly Dictionary<string, Script<object>> _compiledScripts = new();

    public ScriptExecutor(ILogger<ScriptExecutor> logger)
    {
        _logger = logger;
    }

    public async Task<bool> CompileScriptAsync(string tagPath, string scriptContent)
    {
        try
        {
            _logger.LogInformation("Compiling script for {TagPath}...", tagPath);

            var options = ScriptOptions.Default
                .AddImports("System")
                .AddImports("System.Math")
                .AddImports("System.Linq")
                .AddReferences(
                    typeof(object).Assembly,
                    typeof(Enumerable).Assembly
                );

            var script = CSharpScript.Create<object>(
                scriptContent,
                options,
                globalsType: typeof(ScriptContext)
            );

            var diagnostics = script.Compile();

            if (diagnostics.Any(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error))
            {
                foreach (var diagnostic in diagnostics)
                {
                    _logger.LogError("Script compilation error in {TagPath}: {Error}",
                        tagPath, diagnostic.GetMessage());
                }

                return false;
            }

            _compiledScripts[tagPath] = script;
            _logger.LogInformation("Script compiled successfully for {TagPath}", tagPath);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to compile script for {TagPath}", tagPath);
            return false;
        }
    }
    
    public async Task<object?> ExecuteScriptAsync(string tagPath, object? currentValue)
    {
        if (!_compiledScripts.TryGetValue(tagPath, out var script))
        {
            _logger.LogWarning("No compiled script found for {TagPath}", tagPath);
            return currentValue;
        }
        
        try
        {
            var actualValue = ConvertFromJsonElement(currentValue);
            
            var context = new ScriptContext
            {
                value = actualValue
            };
            
            var result = await script.RunAsync(context);
            
            return result.ReturnValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Script execution error for {TagPath}", tagPath);
            return currentValue;
        }
    }
    
    private object? ConvertFromJsonElement(object? value)
    {
        if (value is not JsonElement jsonElement)
            return value;
        
        return jsonElement.ValueKind switch
        {
            JsonValueKind.Number when jsonElement.TryGetInt32(out var intValue) => intValue,
            JsonValueKind.Number when jsonElement.TryGetInt64(out var longValue) => longValue,
            JsonValueKind.Number when jsonElement.TryGetDouble(out var doubleValue) => doubleValue,
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.String => jsonElement.GetString(),
            JsonValueKind.Null => null,
            _ => value
        };
    }
}