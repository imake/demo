
/*
消息包：混淆长度(包名+协议名) + 混淆数据 + 协议加密数据(协议名+数据)
*/

using Newtonsoft.Json;
using System;
using System.Text;
using UnityEngine;

public static class JsonEncryptUtil
{
    private static JsonSerializerSettings jsonSettings = new JsonSerializerSettings
    {
        // 无格式
        Formatting = Formatting.None,
        // 时间格式
        DateFormatString = "yyyy/MM/dd hh:mm:ss",
    };

    private static JsonSerializerSettings jsonIndentedSettings = new JsonSerializerSettings
    {
        // 缩进格式
        Formatting = Formatting.Indented,
        // 时间格式
        DateFormatString = "yyyy/MM/dd hh:mm:ss",
    };

    private const int msgFirstLength = 1;
    private static char[] randomKeys = "0123456789qwertyuiopasdfghjklzxcvbnm".ToCharArray();

    public static string NoEncryptJsonProto(BaseJsonProto jsonProtoMsg, Type type)
    {
        string json = JsonConvert.SerializeObject(jsonProtoMsg, type, jsonSettings);
        return json;
    }

    public static byte[] EncryptJsonProto(BaseJsonProto jsonProtoMsg, Type type)
    {
        string json = JsonConvert.SerializeObject(jsonProtoMsg, type, jsonSettings);
        return Encrypt(jsonProtoMsg.type.Length, json);
    }

    public static string DecryptJsonProto(byte[] data)
    {
        return Decrypt(data);
    }

    private static byte[] Encrypt(int protoLength, string json)
    {
        byte[] aesData = AESEncryptUtil.Encrypt(json);

        byte randomLength = (byte)(AppConst.PackageName.Length + protoLength);
        int totalLength = msgFirstLength + randomLength + aesData.Length;
        byte[] finalBytes = new byte[totalLength];
        finalBytes[0] = randomLength;

        char[] randomDataArray = GetRandomChars(randomLength);
        for (int i = 0; i < randomLength; i++)
        {
            finalBytes[msgFirstLength + i] = (byte)randomDataArray[i];
        }
        for (int i = msgFirstLength + randomLength; i < totalLength; i++)
        {
            finalBytes[i] = aesData[i - randomLength - msgFirstLength];
        }

        return finalBytes;
    }

    private static string Decrypt(byte[] data)
    {
        byte randomLength = data[0];
        int totalLength = data.Length;
        int aesDataLength = totalLength - randomLength - msgFirstLength;
        byte[] dataBytes = new byte[aesDataLength];
        for (int i = 0; i < aesDataLength; i++)
        {
            dataBytes[i] = data[msgFirstLength + randomLength + i];
        }
        string json = AESEncryptUtil.Decrypt(dataBytes);
        return json;
    }

    private static char[] GetRandomChars(int length)
    {
        char[] randomChar = new char[length];
        int keysLength = randomKeys.Length;
        for (int i = 0; i < length; i++)
        {
            float randomIdx = UnityEngine.Random.Range(0, 1) * keysLength;
            int index = Mathf.CeilToInt(randomIdx);
            index = Mathf.Clamp(index, 0, keysLength - 1);
            randomChar[i] = randomKeys[index];
        }
        return randomChar;
    }
}