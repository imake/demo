/****************************************************************************
* ScriptType: 主框架
* 请勿修改!!!
****************************************************************************/

using UnityEngine;
using LitJson;

public static class PrefsConst
{
    public const bool BoolDefault = false;
    public const int IntDefault = 0;
    public const int IntTrue = 1;
    public const int IntFalse = 2;
}

public static class PrefsUtil
{
    public static void WriteInt(string key, int data)
    {
        PlayerPrefs.SetInt(key, data);
        Debug.LogFormat("[PrefsUtil WriteInt]Key: {0}\n{1}", key, data);
    }

    public static void WriteBool(string key, bool data)
    {
        PlayerPrefs.SetInt(key, data ? PrefsConst.IntTrue : PrefsConst.IntFalse);
        Debug.LogFormat("[PrefsUtil WriteBool]Key: {0}\n{1}", key, data);
    }

    public static void WriteString(string key, string data)
    {
        PlayerPrefs.SetString(key, data);
        Debug.LogFormat("[PrefsUtil WriteString]Key: {0}\n{1}", key, data);
    }

    public static void WriteObject(string key, object data)
    {
        string dataStr = JsonMapper.ToJson(data);
        PlayerPrefs.SetString(key, dataStr);
        Debug.LogFormat("[PrefsUtil WriteObject]Key: {0}\n{1}", key, dataStr);
    }

    public static int ReadInt(string key, int defalutValue = PrefsConst.IntDefault)
    {
        int data = PlayerPrefs.GetInt(key, defalutValue);
        Debug.LogFormat("[PrefsUtil ReadInt]Key: {0}\n{1}", key, data);
        return data;
    }

    public static bool ReadBool(string key, bool defalutValue = PrefsConst.BoolDefault)
    {
        if (!HasKey(key))
        {
            return defalutValue;
        }
        bool data = PlayerPrefs.GetInt(key) == PrefsConst.IntTrue ? true : false;
        Debug.LogFormat("[PrefsUtil ReadBool]Key: {0}\n{1}", key, data);
        return data;
    }

    public static string ReadString(string key)
    {
        string data = PlayerPrefs.GetString(key, string.Empty);
        Debug.LogFormat("[PrefsUtil ReadString]Key: {0}\n{1}", key, data);
        return data;
    }

    public static object ReadObject<T>(string key)
    {
        if (!HasKey(key))
        {
            Debug.LogFormat("[PrefsUtil ReadObject]Key: {0}, Not Has Key", key);
            return null;
        }

        string dataStr = PlayerPrefs.GetString(key);
        if (string.IsNullOrEmpty(dataStr))
        {
            Debug.LogFormat("[PrefsUtil ReadObject]Key: {0}, Data Is Null", key);
            return null;
        }

        Debug.LogFormat("[PrefsUtil ReadObject]Key: {0}\n{1}", key, dataStr);
        T data = JsonMapper.ToObject<T>(dataStr);
        return data;
    }

    public static bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }

    public static void DeleteKey(string key)
    {
        if (HasKey(key))
        {
            PlayerPrefs.DeleteKey(key);
        }
    }
}