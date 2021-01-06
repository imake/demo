using UnityEngine;
using System.Collections;
using System.IO;

public static class StringExtension
{
    public static int JavaHashCode(this string s)
    {
        int num = 0;
        int length = s.Length;
        if (length > 0)
        {
            int num2 = 0;
            for (int i = 0; i < length; i++)
            {
                char c = s[num2++];
                num = 31 * num + (int)c;
            }
        }
        return num;
    }

    public static int JavaHashCodeIgnoreCase(this string s)
    {
        int num = 0;
        int length = s.Length;
        if (length > 0)
        {
            int num2 = 0;
            for (int i = 0; i < length; i++)
            {
                char c = s[num2++];
                if (c >= 'A' && c <= 'Z')
                {
                    c += ' ';
                }
                num = 31 * num + (int)c;
            }
        }
        return num;
    }

    /// <summary>
    /// 合并两个文件路径字符串
    /// </summary>
    /// <param name="s1">第一个</param>
    /// <param name="s2">第二个</param>
    /// <returns></returns>
    public static string CombinePath(this string s1, string s2)
    {
        return s1 + "/" + s2;
    }

    /// <summary>
    /// 获取文件名字(不含后缀名)
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns></returns>
    public static string GetFileNameWithoutExtension(this string path)
    {
        return Path.GetFileNameWithoutExtension(path);
    }

    /// <summary>
    /// 获取文件名字(带后缀名)
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns></returns>
    public static string GetFileNameWithExtension(this string path)
    {
        return Path.GetFileName(path);
    }

    /// <summary>
    /// 获取后缀
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns></returns>
    public static string GetFileExtension(this string path)
    {
        return Path.GetExtension(path);
    }

    /// <summary>
    /// 从文件字符串中得到包括文件名和扩展名的全路径名
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns></returns>
    public static string GetFullPath(this string path)
    {
        return Path.GetFullPath(path);
    }

    /// <summary>
    /// 获取路径(不包含扩展名)
    /// </summary>
    /// <param name="s">路径</param>
    /// <returns></returns>
    public static string GetFilePathWithoutExtension(this string s)
    {
        if (s.LastIndexOf('.') == -1)
        {
            return s;
        }

        return s.Substring(0, s.LastIndexOf('.'));
    }

    /// <summary>
    /// 得到文件的文件夹路径
    /// </summary>
    /// <param name="path">路径</param>
    /// <returns></returns>
    public static string GetDirectoryPath(this string path)
    {
        return Path.GetDirectoryName(path);
    }

    /// <summary>
    /// 获取路径删除Application.dataPath
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static string GetAssetsDirectoryPath(this string path)
    {
        return path.Substring(Application.dataPath.Length + 1);
    }
}
