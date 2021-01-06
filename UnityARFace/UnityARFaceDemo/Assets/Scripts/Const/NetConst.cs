using System;
using UnityEngine;

public enum ChannelType : int
{
    /// <summary>
    /// 本地测试
    /// </summary>
    LocalDebug = 0,

    /// <summary>
    /// 外网审核
    /// </summary>
    NetCheck = 1,

    /// <summary>
    /// 外网正式
    /// </summary>
    NetRelease = 2,
}

public static class NetConst
{
    public static readonly bool IsLittleEndian = BitConverter.IsLittleEndian;

    public static string WebSocketReleaseUrl = "";
    public static string WebSocketTestUrl = "";

    ///***********************************************
    /// LocalHost = "http://127.0.0.1:80/";
    /// SimulatorLocalHost = "http://10.0.2.2:80/";
    ///***********************************************
    public static string AssetsWebUrl;
    public static string LoginWebUrl;

    ///***********************************************
    /// Host = "127.0.0.1";
    /// Port = 11001;
    ///***********************************************
    public static string AppTcpHost;
    public static int AppTcpPort;
    public static string GamePlayTcpHost;
    public static int GamePlayTcpPort;

    public static string WebSocketUrl;

    public static void AfterInit()
    {
        RedirectionAssetsWebUrl();
        RedirectionAppWebUrl();
        RedirectionWebSocketUrl();
    }

    public static bool IsNetAvailable
    {
        get {
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    private static void RedirectionAssetsWebUrl()
    {
        switch (AppConst.ChannelType)
        {
            case ChannelType.LocalDebug:
                AssetsWebUrl = "http://125.65.83.247/assets/debug/";
                break;
            case ChannelType.NetRelease:
                AssetsWebUrl = "http://125.65.83.247/assets/release/";
                break;
            case ChannelType.NetCheck:
                AssetsWebUrl = "http://125.65.83.247/assets/check/";
                break;
        }
    }

    private static void RedirectionAppWebUrl()
    {
        switch (AppConst.ChannelType)
        {
            case ChannelType.LocalDebug:
                LoginWebUrl = "http://125.65.83.247/Test/server_list.php";
                break;
            case ChannelType.NetRelease:
                LoginWebUrl = "http://125.65.83.247/TapTest/server_list.php";
                break;
            case ChannelType.NetCheck:
                LoginWebUrl = "http://125.65.83.247/LoginServer/server_list.php";
                break;
        }
    }

    private static void RedirectionWebSocketUrl()
    {
        switch (AppConst.ChannelType)
        {
            case ChannelType.LocalDebug:
                WebSocketUrl = WebSocketTestUrl;
                break;
            case ChannelType.NetRelease:
                WebSocketUrl = WebSocketReleaseUrl;
                break;
            case ChannelType.NetCheck:
                WebSocketUrl = WebSocketReleaseUrl;
                break;
        }
    }
}
