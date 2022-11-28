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
                var aliases = new String[] { $"--{model.ServiceOptionName}-{action.Name.ToLower()}" };

                var serviceName = model.ServiceType.Name.Replace("Service", "");
                var description = $"Implementation of {serviceName}";

                var option = new Option<Boolean>(aliases, description);

                result.Add(option);
            }
        }

        return result;
    }

    internal async Task Solve(InvocationContext context, List<GenericModel> models)
    {
        var children = context.ParseResult.CommandResult.Children;
        var options = children.OfType<OptionResult>().ToList();
        var inputArgument = children.OfType<ArgumentResult>().First(a => a.Symbol.Name.Equals("Input"));
        var result = inputArgument.Tokens.First().Value;

        foreach(var option in options)
        {
            String serviceOptionName = option.Token!.Value.Remove(0, 2).Split('-').First();
            String actionName = option.Token!.Value.Remove(0, 2).Split('-').Last();

            var model = models.Single(m => m.ServiceOptionName.Equals(serviceOptionName));
            var action = model.ServiceActions.Single(a => a.Name.ToLower().Equals(actionName));

            var actionParameters = action.GetParameters();
            List<Object> paramList = new(); 
            foreach(var param in actionParameters)
            {
                if (param.Name!.Equals("input"))
                {
                    paramList.Add(result);
                    continue;
                }

                Console.Write($"{param.Name}: ");
                var input = Console.ReadLine();

                paramList.Add(input ?? String.Empty);
            }

            var instance = Activator.CreateInstance(model.ServiceType);
            result = (String) (action.Invoke(instance, paramList.ToArray()) ?? String.Empty);
        }

        Console.WriteLine($"Ergebnis: {result}");
    }
}
