using ncrypt.CommandLine;
using System.CommandLine;
using System.CommandLine.Invocation;

var _service = new ApplicationService();
var models = _service.Load();

var rootCommand = new RootCommand(
    "Application to solve complex problems with modern cryptographic alogrithms");

// Set input argument
var arg = new Argument<String>("Input", "Eingabefeld");
arg.SetDefaultValue(String.Empty);
rootCommand.AddArgument(arg);

// Set output flag
var outputAliases = new String[] { "-o", "--output" };
var outputOption = new Option<FileInfo?>(outputAliases, "Define output file");
rootCommand.AddGlobalOption(outputOption);

var importAliases = new String[] { "-i", "--import" };
var importOption = new Option<FileInfo?>(importAliases, "Import custom service");
rootCommand.AddGlobalOption(importOption);

var options = _service.ModelsToOptions(models);

foreach(var option in options)
{
    rootCommand.AddOption(option);
}

rootCommand.SetHandler((InvocationContext context) => new ApplicationService().Solve(context, models));

await rootCommand.InvokeAsync(args);