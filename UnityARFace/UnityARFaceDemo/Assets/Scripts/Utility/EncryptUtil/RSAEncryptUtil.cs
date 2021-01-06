
using System.Security.Cryptography;

/// <summary>
/// 非对称加密：客户端和服务端使用 不相同的密钥，加密速度非常慢。
/// </summary>
public static class RSAEncryptUtil
{
    /// <summary>
    /// 生成RSA私钥和RSA公钥
    /// </summary>
    public static void GenerateKey(ref string privateKey, ref string publicKey)
    {
        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        privateKey = rsa.ToXmlString(true);
        publicKey = rsa.ToXmlString(false);
    }

    /// <summary>
    /// RSA公钥加密
    /// </summary>
    public static byte[] Encrypt(byte[] data, string publicKey)
    {
        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        rsa.FromXmlString(publicKey);
        byte[] encryptData = rsa.Encrypt(data, false);
        return encryptData;
    }

    /// <summary>
    /// RSA私钥解密
    /// </summary>
    public static byte[] Decrypt(byte[] data, string privateKey)
    {
        RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
        rsa.FromXmlString(privateKey);
        byte[] decryptData = rsa.Decrypt(data, false);
        return decryptData;
    }
}
