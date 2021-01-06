
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;


public class MD5EncryptUtil
{
    /// <summary>
    /// 获取字符串的MD5值
    /// </summary>
    public static string GetStringMD5(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return null;
        }
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] bytResult = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
        string strResult = BitConverter.ToString(bytResult);
        strResult = strResult.Replace("-", string.Empty);
        return strResult;
    }

    /// <summary>
    /// 获取文件的MD5值
    /// </summary>
    public static string GetFileMD5(string file)
    {
        try
        {
            FileStream fs = new FileStream(file, FileMode.Open);
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(fs);
            fs.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++)
            {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            throw new Exception("GetFileMD5 fail error: " + ex.Message);
        }
    }
}