using ProtoBuf;
using System.IO;
using UnityEngine;

/// <summary>
/// 使用Protobuf序列化的实现类
/// </summary>
/// <typeparam name="T">要序列化的类型</typeparam>
public class ProtobufSerializer<T>
{
    public static void Serialize(T data, string absolutePath)
    {
        try
        {
            if (null == data) return;

            if (File.Exists(absolutePath))
            {
                File.Delete(absolutePath);
            }
            else
            {
                ExistPathDirectory(absolutePath);
            }

            FileStream fileStream = new FileStream(absolutePath, FileMode.CreateNew, FileAccess.Write);

            Serializer.Serialize(fileStream, data);

            fileStream.Close();
        }
        catch (System.Exception ex)
        {

#if UNITY_EDITOR
            Debug.LogError(ex.ToString());
#endif
        }
    }

    private static void ExistPathDirectory(string path)
    {
        string directory = Path.GetDirectoryName(path);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    public static T Deserialize(string absolutePath)
    {
        if (!FileManager.IsFileExist(absolutePath))
        {
            return default(T);
        }
        try
        {
            if (GameFramework.UseStreamingAssets)
            {
                WWW www = new WWW(absolutePath);

                T data = default(T);

                while (!www.isDone)
                { }

                Stream stream = new MemoryStream(www.bytes);

                if (stream == null)
                {
                    Debug.LogError("XXXXXXXXXXXXXXXXXXXXXX");
                }

                data = Serializer.Deserialize<T>(stream);

                stream.Close();

                return data;
            }
            else
            {
                FileStream fileStream = new FileStream(absolutePath, FileMode.Open, FileAccess.Read, FileShare.Read);

                T data = Serializer.Deserialize<T>(fileStream);

                fileStream.Close();

                return data;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError(ex.ToString());

            return default(T);
        }
    }

    public static T DeserializeBinary(byte[] bytes)
    {
        if (null == bytes)
        {
            return default(T);
        }

        try
        {
            Stream steam = new MemoryStream(bytes);

            T data = Serializer.Deserialize<T>(steam);

            steam.Close();

            return data;
        }
        catch (System.Exception ex)
        {

#if UNITY_EDITOR
            Debug.Log(ex.ToString());
#endif

            return default(T);
        }
    }
}
