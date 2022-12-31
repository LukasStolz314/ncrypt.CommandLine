using ncrypt.Library;
using System.Reflection;

namespace ncrypt.CommandLine;

internal class GenericModel
{
    public Type ServiceType { get; set; }
    public GenericModel(Type serviceType)
    {
        ServiceType = serviceType;
    }

    private List<MethodInfo>? _serviceActions;
    public List<MethodInfo> ServiceActions
    {
        get
        {
            if (_serviceActions is null || _serviceActions.Count == 0)
            {
                _serviceActions = ServiceType.GetMethods()
                    .Where(m => m.GetCustomAttributes()
                        .Any(a => a.GetType() == typeof(ActionCommand))
                    ).ToList();
            }

            return _serviceActions;
        }
    }

    public String ServiceOptionName => ServiceType
        .GetCustomAttribute<Service>()?.OptionName ?? String.Empty;

    public String? GetActionOptionName(MethodInfo action) =>
        action.GetCustomAttribute<ActionCommand>()?.OptionName;

    public String? GetActionOptionDescription(MethodInfo action) =>
        action.GetCustomAttribute<ActionCommand>()?.OptionDescription;

    public MethodInfo? GetActionByOptionName(String optionName) =>
        ServiceActions.Single(a => a.Name.ToLower().Equals(optionName) ||
        (a.GetCustomAttribute<ActionCommand>()!.OptionName ?? String.Empty).Equals(optionName));
}
