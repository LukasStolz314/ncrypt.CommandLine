using System.Text;

namespace ncrypt.Library;

public enum ConvertType
{
    ASCII,
    HEX,
    BASE64,
}

public class Converter
{
    internal static String ToStringWithFixedLineLength(String text, Int32 lineLength)
    {
        String remainedText = text;
        StringBuilder sb = new();

        while (remainedText.Count() > lineLength)
        {
            sb.AppendLine(new(remainedText.Take(lineLength).ToArray()));
            remainedText = remainedText.Remove(0, lineLength);
        }

        sb.Append(remainedText);

        return sb.ToString();
    }

    #region String-Byte and Byte-String

    internal static String ToString(Byte[] key, ConvertType type) => type switch
    {
        ConvertType.BASE64 => Encoding.UTF8.GetString(key),
        ConvertType.ASCII => Encoding.ASCII.GetString(key),
        ConvertType.HEX => Convert.ToHexString(key),
        _ => throw new NotImplementedException()
    };

    internal static Byte[] ToByteArray(String key, ConvertType type) => type switch
    {
        ConvertType.BASE64 => Encoding.UTF8.GetBytes(key),
        ConvertType.ASCII => Encoding.ASCII.GetBytes(key),
        ConvertType.HEX => Convert.FromHexString(key),
        _ => throw new NotImplementedException()
    };

    #endregion  

    #region ToHex

    private static String FromAsciiToHex(String text)
    {
        // Based on https://stackoverflow.com/questions/15920741/convert-from-string-ascii-to-string-hex
        StringBuilder sb = new StringBuilder();

        foreach (Char c in text)
        {
            sb.AppendFormat("{0:X2}", (Int32)c);
        }

        var result = sb.ToString();
        return result;
    }

    private static String FromBase64ToHex(String text)
    {
        var bytes = Convert.FromBase64String(text);
        var ascii = Encoding.ASCII.GetString(bytes);

        var result = FromAsciiToHex(ascii);

        return result;
    }

    public static String ToHex(String text, ConvertType current) => current switch
    {
        ConvertType.BASE64 => FromBase64ToHex(text),
        ConvertType.ASCII => FromAsciiToHex(text),
        ConvertType.HEX => text,
        _ => throw new NotImplementedException(),
    };

    #endregion

    #region FromHex

    private static String FromHexToAscii(String text)
    {
        // Used from https://stackoverflow.com/questions/5613279/c-sharp-hex-to-ascii
        StringBuilder sb = new StringBuilder();

        for (Int32 i = 0; i < text.Length; i += 2)
        {
            String hs = text.Substring(i, 2);
            sb.Append(Convert.ToChar(Convert.ToUInt32(hs, 16)));
        }

        var result = sb.ToString();
        return result;
    }

    private static String FromHexToBase64(String text)
    {
        var result = FromHexToAscii(text);

        var bytes = Encoding.ASCII.GetBytes(result);
        result = Convert.ToBase64String(bytes);

        return result;
    }

    public static String FromHex(String text, ConvertType newType) => newType switch
    {
        ConvertType.BASE64 => FromHexToBase64(text),
        ConvertType.ASCII => FromHexToAscii(text),
        ConvertType.HEX => text,
        _ => throw new NotImplementedException(),
    };

    #endregion    
}
