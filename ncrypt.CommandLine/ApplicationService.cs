using ncrypt.Library;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Reflection;

namespace ncrypt.CommandLine;

internal class ApplicationService
{
    internal List<GenericModel> Load()
    {
        List<GenericModel> result = new();

        Assembly lib = Assembly.Load("ncrypt.Library");
        result = lib.GetTypes()
            .Where(t => t.CustomAttributes
                .Any(a => a.AttributeType == typeof(Service))
            ).Select(t => new GenericModel(t)).ToList();

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
        // Gain options choosen and input string
        var children = context.ParseResult.CommandResult.Children;
        var options = children.OfType<OptionResult>();
        var inputArgument = children.OfType<ArgumentResult>()
            .First(a => a.Symbol.Name.Equals("Input"));

        // Set input string as first result
        var result = inputArgument.Tokens.FirstOrDefault()?.Value;

        foreach(var option in options)
        {
            GenericModel model;
            MethodInfo? action;
            try
            {
                // Reproduce service and action name from option flag
                String serviceOptionName = option.Token!.Value.Remove(0, 2).Split('-').First();
                String actionOptionName = option.Token!.Value.Remove(0, 2).Split('-').Last();

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

        // Print result
        Func<OptionResult, Boolean> outputFunc = (o => 
            o.Symbol.Name.Equals("output") || o.Symbol.Name.Equals("o"));

        if(options.Any(outputFunc))
        {
            var fi = new FileInfo(options.First(outputFunc).Tokens.First().Value);
            try
            {
                using (StreamWriter sw = fi.AppendText())
                {
                    sw.WriteLine(result);
                }
            }
            catch(DirectoryNotFoundException e) 
            {
                Console.WriteLine("Not able to create file"); 
            }
        }
        else
        {
            Console.WriteLine($"Ergebnis: ");
            Console.WriteLine(result);
        }
    }
}
