using ncrypt.Library;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Reflection;
using System.Runtime.InteropServices;

namespace ncrypt.CommandLine;

internal class ApplicationService
{
    private String _homePath = String.Empty;
    private String _pluginPath = String.Empty;

    public ApplicationService()
    {
        _homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if(RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _pluginPath = Path.Combine(_homePath, "AppData", "Roaming",
                "ncrypt.CommandLine", "PluginServices");
        }
        else if(RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            _pluginPath = Path.Combine(_homePath, "Library", "Application Support",
                "ncrypt.CommandLine", "PluginServices");
        }
        else
        {
            throw new NotSupportedException("OS not supported yet");
        }
    }

    internal List<GenericModel> Load()
    {
        List<Type> services = new();

        Assembly lib = Assembly.Load("ncrypt.Library");
        services = lib.GetTypes()
            .Where(t => t.CustomAttributes
                .Any(a => a.AttributeType == typeof(Service))
            ).ToList();

        Directory.CreateDirectory(_pluginPath);

        foreach (String dll in Directory.GetFiles(_pluginPath, "*.dll"))
        {
            var types = Assembly.LoadFile(dll).GetTypes();
            var selected = types.Where(t => t.GetCustomAttributes().Any(
                a => a.GetType() == typeof(Service)));

            services.AddRange(selected);
        }
        
        List<GenericModel> result = services.Select(t => new GenericModel(t)).ToList();
        return result;
    }

    internal List<Option> ModelsToOptions(List<GenericModel> models)
    {
        List<Option> result = new();

        foreach (var model in models)
        {
            foreach (var action in model.ServiceActions)
            {
                var actionOptionName = model.GetActionOptionName(action) ?? action.Name.ToLower();
                var aliases = new String[] { $"--{model.ServiceOptionName}-{actionOptionName}" };

                var serviceName = model.ServiceType.Name.Replace("Service", "");                
                var description = model.GetActionOptionDescription(action) ??
                    $"Implementation of {serviceName} {action.Name}";

                var parameters = action.GetParameters().Select(p => p.Name ?? String.Empty);
                var parameterNames = " <";

                foreach(var parameter in parameters)
                {
                    if (parameter.ToLower().Equals("input"))
                        continue;

                    parameterNames += $"{parameter} | ";
                }
                
                if(parameterNames.Length > 2)
                    parameterNames = parameterNames.Substring(0, parameterNames.Length - 3);
                parameterNames += ">";

                description += parameterNames;

                var option = new Option<Boolean>(aliases, description);

                result.Add(option);
            }
        }

        return result;
    }

    internal void Solve(InvocationContext context, List<GenericModel> models)
    {
        // Get context childrens and input string
        var children = context.ParseResult.CommandResult.Children;
        var inputArgument = children.OfType<ArgumentResult>()
            .First(a => a.Symbol.Name.Equals("Input"));

        // Get option sequence
        var tokens = context.ParseResult.Tokens;
        var optionTokens = tokens.Where(c => c.Type == TokenType.Option);
        

        // Set input string as first result
        var result = inputArgument.Tokens.FirstOrDefault()?.Value ?? String.Empty;

        foreach(var optionToken in optionTokens)
        {
            GenericModel model;
            MethodInfo? action;
            try
            {
                // Reproduce service and action name from option flag
                String serviceOptionName = optionToken.Value.Remove(0, 2).Split('-').First();
                String actionOptionName = optionToken.Value.Remove(0, 2).Split('-').Last();

                // Get reflection models
                model = models.Single(m => m.ServiceOptionName.Equals(serviceOptionName));
                action = model.GetActionByOptionName(actionOptionName);
            }
            catch (InvalidOperationException e) { continue; }

            // Ask user for parameters values
            var actionParameters = action.GetParameters();
            List<Object> paramList = new(); 
            foreach(var param in actionParameters)
            {
                if (param.Name!.Equals("input"))
                {
                    paramList.Add(result ?? String.Empty);
                    continue;
                }

                Console.Write($"{param.Name}: ");
                var input = Console.ReadLine();

                paramList.Add(input ?? String.Empty);
            }

            // Create service instance and invoke action
            var instance = Activator.CreateInstance(model.ServiceType);
            result = (String) (action.Invoke(instance, paramList.ToArray()) ?? String.Empty);
        }

        // Get options
        var options = children.OfType<OptionResult>();

        CheckForGlobalOptionsAndPrintResult(options, result);
    }



    internal void CheckForGlobalOptionsAndPrintResult(
        IEnumerable<OptionResult> options, String result)
    {
        Func<OptionResult, Boolean> outputFunc = (o =>
            o.Symbol.Name.Equals("output") || o.Symbol.Name.Equals("o"));

        Func<OptionResult, Boolean> importFunc = (o =>
            o.Symbol.Name.Equals("import") || o.Symbol.Name.Equals("i"));

        if (options.Any(outputFunc))
        {
            var fi = new FileInfo(options.First(outputFunc).Tokens.First().Value);
            try
            {
                using (StreamWriter sw = fi.AppendText())
                {
                    sw.WriteLine(result);
                }
            }
            catch (DirectoryNotFoundException e)
            {
                Console.WriteLine("Not able to create file");
            }
        }
        else if (options.Any(importFunc))
        {
            Directory.CreateDirectory(_pluginPath);
            var file = new FileInfo(options.First(importFunc).Tokens.First().Value);
            File.Copy(file.FullName, Path.Combine(_pluginPath, file.Name));
        }
        else
        {
            Console.WriteLine($"Ergebnis: ");
            Console.WriteLine(result);
        }
    }
}
