using System.Security.Cryptography;
using System.Text;

namespace FarmaciaSistema.Helpers;

// RNF03: criptografia simétrica (AES) dos dados de receituário/paciente armazenados
// no banco, em conformidade com a LGPD. Chave fixa apenas para fins didáticos -
// em produção viria de um cofre de segredos (ex: Azure Key Vault, variáveis de ambiente).
public static class CryptoHelper
{
    private static readonly byte[] Key = SHA256.HashData(Encoding.UTF8.GetBytes("farmacia-sngpc-chave-secreta"));

    public static string Criptografar(string texto)
    {
        using var aes = Aes.Create();
        aes.Key = Key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var bytes = Encoding.UTF8.GetBytes(texto);
        var cipher = encryptor.TransformFinalBlock(bytes, 0, bytes.Length);

        // Concatena IV + dados cifrados, em Base64, para armazenar em uma única coluna
        var resultado = new byte[aes.IV.Length + cipher.Length];
        Buffer.BlockCopy(aes.IV, 0, resultado, 0, aes.IV.Length);
        Buffer.BlockCopy(cipher, 0, resultado, aes.IV.Length, cipher.Length);

        return Convert.ToBase64String(resultado);
    }

    public static string Descriptografar(string textoCriptografado)
    {
        var dados = Convert.FromBase64String(textoCriptografado);

        using var aes = Aes.Create();
        aes.Key = Key;

        var iv = new byte[aes.IV.Length];
        var cipher = new byte[dados.Length - iv.Length];
        Buffer.BlockCopy(dados, 0, iv, 0, iv.Length);
        Buffer.BlockCopy(dados, iv.Length, cipher, 0, cipher.Length);
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var bytes = decryptor.TransformFinalBlock(cipher, 0, cipher.Length);
        return Encoding.UTF8.GetString(bytes);
    }

    // RF03: hash da "senha/assinatura digital" do farmacêutico
    public static string HashSenha(string senha)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(senha));
        return Convert.ToHexString(bytes);
    }
}
