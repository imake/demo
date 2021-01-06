
using System.Security.Cryptography;

/// <summary>
/// 对称加密：客户端和服务端 使用相同的密钥，加密速度很快。
/// </summary>
public static class DESEncryptUtil
{
    /// <summary>
    /// DES加密
    /// </summary>
    public static byte[] Encrypt(byte[] data, byte[] desrgbKey, byte[] desrgbIV)
    {
        DESCryptoServiceProvider des = new DESCryptoServiceProvider();
        System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
        CryptoStream cryptoStream = new CryptoStream(memoryStream, des.CreateEncryptor(desrgbKey, desrgbIV), CryptoStreamMode.Write);
        cryptoStream.Write(data, 0, data.Length);
        cryptoStream.FlushFinalBlock();
        return memoryStream.ToArray();
    }

    /// <summary>
    /// DES解密
    /// </summary>
    public static byte[] Decrypt(byte[] data, byte[] desrgbKey, byte[] desrgbIV)
    {
        DESCryptoServiceProvider des = new DESCryptoServiceProvider();
        System.IO.MemoryStream memoryStream = new System.IO.MemoryStream();
        CryptoStream cryptoStream = new CryptoStream(memoryStream, des.CreateDecryptor(desrgbKey, desrgbIV), CryptoStreamMode.Write);
        cryptoStream.Write(data, 0, data.Length);
        cryptoStream.FlushFinalBlock();
        return memoryStream.ToArray();
    }
}
