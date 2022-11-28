namespace ncrypt.Library;

public class Service : Attribute
{
    public String OptionName { get; set; }

    public Service(String optionName)
    {
        OptionName = optionName;
    }
}

public class ActionCommand : Attribute
{
    public String OptionName { get; set; }
    public String OptionDescription { get; set; }
}