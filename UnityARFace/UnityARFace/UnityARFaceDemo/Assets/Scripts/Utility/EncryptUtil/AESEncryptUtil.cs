using System;
using System.Security.Cryptography;
using System.Text;

/// <summary>
/// 对称加密：客户端和服务端 使用相同的密钥，加密速度很快。
/// </summary>
public static class AESEncryptUtil
{
    /// <summary>
    /// 运算字节矩阵
    /// </summary>
    private const int byteMatrixSize = 4 * 4;
    /// <summary>
    /// 补零
    /// </summary>
    private const string zeroString = "\0";

    /// <summary>
    /// AES加密
    /// </summary>
    public static byte[] Encrypt(string text, string key, string iVector)
    {
        int totalLen = text.Length;
        int maxLength = (int)Math.Ceiling((double)(totalLen / byteMatrixSize)) * byteMatrixSize;
        for (int i = totalLen; i < maxLength; i++)
        {
            text += zeroString;
        }

        byte[] toEncryptArray = Encoding.UTF8.GetBytes(text);
        byte[] keyArray = Encoding.UTF8.GetBytes(key);
        byte[] ivArray = Encoding.UTF8.GetBytes(iVector);

        RijndaelManaged aes = new RijndaelManaged();
        aes.Key = keyArray;
        aes.IV = ivArray;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.Zeros;

        ICryptoTransform cTransform = aes.CreateEncryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
        return resultArray;
    }

    public static byte[] Encrypt(string text)
    {
        return Encrypt(text, EncryptConst.AES_Key, EncryptConst.AES_IVector);
    }

    /// <summary>
    /// AES解密
    /// </summary>>
    public static string Decrypt(byte[] bytes, string key, string iVector)
    {
        byte[] toEncryptArray = bytes;
        byte[] keyArray = Encoding.UTF8.GetBytes(key);
        byte[] ivArray = Encoding.UTF8.GetBytes(iVector);

        RijndaelManaged aes = new RijndaelManaged();
        aes.Key = keyArray;
        aes.IV = ivArray;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.Zeros;

        ICryptoTransform cTransform = aes.CreateDecryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

        string text = Encoding.UTF8.GetString(resultArray);
        int zeroIdx = text.IndexOf(zeroString);
        if (zeroIdx > 0)
        {
            text = text.Substring(0, zeroIdx);
        }
        return text;
    }

    public static string Decrypt(byte[] bytes)
    {
        return Decrypt(bytes, EncryptConst.AES_Key, EncryptConst.AES_IVector);
    }
}