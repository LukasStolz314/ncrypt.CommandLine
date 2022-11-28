using System.Security.Cryptography;
using System.Text;

namespace ncrypt.Library.Cipher;

[Service("rsa")]
public class RSAService
{
    [ActionCommand(OptionName = "keypair",
        OptionDescription = "Generate rsa key pair with given size")]
    public String GenerateKeyPair(String keySize)
    {
        Int32 keySizeParsed = Convert.ToInt32(keySize);
        // Create key pair and export to Base64 String
        String privateKey;
        String publicKey;
        using (RSA rsa = RSA.Create(keySizeParsed))
        {
            privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
            publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
        }

        // Write public key to pem format
        StringBuilder publicSB = new();
        publicSB.AppendLine("-----BEGIN RSA PUBLIC KEY-----");
        publicSB.AppendLine(Converter.ToStringWithFixedLineLength(publicKey, 64));
        publicSB.AppendLine("-----END RSA PUBLIC KEY-----");

        // Write private key to pem format
        StringBuilder privateSB = new();
        privateSB.AppendLine("-----BEGIN RSA PRIVATE KEY-----");
        privateSB.AppendLine(Converter.ToStringWithFixedLineLength(privateKey, 64));
        privateSB.AppendLine("-----END RSA PRIVATE KEY-----");

        // Return result object
        StringBuilder result = new();
        result.AppendLine(publicSB.ToString());
        result.AppendLine("");
        result.AppendLine(privateSB.ToString());

        return result.ToString();
    }
}
