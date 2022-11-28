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
                    .Where(m => m.CustomAttributes
                         .Any(a => a.AttributeType == typeof(ActionCommand))
                    ).ToList();
            }

            return _serviceActions;
        }
    }

    public String ServiceOptionName => ServiceType
        .GetCustomAttribute<Service>()?.OptionName ?? String.Empty;
}
