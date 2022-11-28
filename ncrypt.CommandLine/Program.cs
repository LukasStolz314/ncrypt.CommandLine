using ncrypt.CommandLine;
using System.CommandLine;
using System.CommandLine.Invocation;

var _service = new ApplicationService();
var models = _service.Load();

var rootCommand = new RootCommand(
    "Application to solve complex problems with modern cryptographic alogrithms");

var arg = new Argument<String>("Input", "Eingabefeld");
rootCommand.AddArgument(arg);

var options = _service.ModelsToOptions(models);

foreach(var option in options)
{
    rootCommand.AddOption(option);
}

rootCommand.SetHandler((InvocationContext context) => new ApplicationService().Solve(context, models));

await rootCommand.InvokeAsync(args);