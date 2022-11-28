using ncrypt.Library;
using System.CommandLine;
using System.CommandLine.Invocation;
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
                var aliases = new String[] { $"-{model.ServiceOptionName}:{action.Name}" };

                var serviceName = model.ServiceType.Name.Replace("Service", "");
                var description = $"Implementation of {serviceName}";

                var parameters = action.GetParameters().Select(p => p.Name ?? "");
                var filteredParams = parameters.Where(n => !n.Equals("input")).ToArray();

                var option = new Option<String>(aliases, description);

                if(filteredParams.Length > 0)
                    option.Add(filteredParams);

                result.Add(option);
            }
        }

        return result;
    }

    internal async Task Solve(InvocationContext context)
    {
        var results = context.ParseResult.CommandResult.Children;
    }
}
