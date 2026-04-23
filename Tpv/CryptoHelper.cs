using System;
using System.Security.Cryptography;
using System.Text;

public static class CryptoHelper
{
    
    private static readonly byte[] SHARED_KEY =
        Encoding.UTF8.GetBytes("euskal jatetxe gako sekretua bat");

    static CryptoHelper()
    {
        if (SHARED_KEY.Length != 32)
            throw new Exception($"Gakoak 32 byte izan behar ditu. Egungoa: {SHARED_KEY.Length}");
    }

    public static string Cifrar(string plainText)
    {
        if (string.IsNullOrWhiteSpace(plainText))
            return "";

        try
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = SHARED_KEY;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                aes.GenerateIV();
                byte[] iv = aes.IV;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, iv);

                byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
                byte[] cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

                
                byte[] result = new byte[iv.Length + cipherBytes.Length];
                Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                Buffer.BlockCopy(cipherBytes, 0, result, iv.Length, cipherBytes.Length);

                return Convert.ToBase64String(result);
            }
        }
        catch (Exception e)
        {
            throw new Exception("Errorea zifratzean: " + e.Message, e);
        }
    }

    public static string Descifrar(string cipherText)
    {
        if (string.IsNullOrWhiteSpace(cipherText))
            return "";

        try
        {
            byte[] buffer = Convert.FromBase64String(cipherText);

            using (Aes aes = Aes.Create())
            {
                aes.Key = SHARED_KEY;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                
                byte[] iv = new byte[16];
                Buffer.BlockCopy(buffer, 0, iv, 0, iv.Length);

                
                int cipherLen = buffer.Length - iv.Length;
                byte[] cipherBytes = new byte[cipherLen];
                Buffer.BlockCopy(buffer, iv.Length, cipherBytes, 0, cipherLen);

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, iv);
                byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

                return Encoding.UTF8.GetString(plainBytes);
            }
        }
        catch (Exception e)
        {
            throw new Exception("Errorea deszifratzean: " + e.Message, e);
        }
    }
}
