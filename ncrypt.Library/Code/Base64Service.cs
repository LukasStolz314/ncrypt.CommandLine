namespace ncrypt.Library.Code;

[Service("b64")]
public class Base64Service
{
    [ActionCommand]
    public String Encode(String input, String a, String b)
    {
        var bytes = Convert.FromHexString(input);
        var result = Convert.ToBase64String(bytes);
        return Converter.ToHex(result, ConvertType.BASE64);
    }

    [ActionCommand]
    public String Decode(String input)
    {
        var bytes = Converter.FromHex(input, ConvertType.BASE64);
        var result = Convert.FromBase64String(bytes);
        return Convert.ToHexString(result);
    }
}
