
using UnityEngine;

public class SDKManager : Singleton<SDKManager>
{
    private IPlatformSDK platformSDK;

    public override void Init()
    {
#if UNITY_IOS
        platformSDK = new NativeAPI();
        Debug.Log("IosSDK");
#elif UNITY_EDITOR
        platformSDK = new EditorSDK();
        Debug.Log("EditorSDK");
#elif UNITY_ANDROID
        platformSDK = new AndroidSDK();
        Debug.Log("AndroidSDK");
#endif
        platformSDK.Init();
        Debug.Log("_____> platformSDK=" + platformSDK);
    }

    /// <summary>
    /// 发送图片文件
    /// </summary>
    public void RefreshWithBytes(byte[] bytes)
    {
        platformSDK.RefreshWithBytes(bytes, bytes.Length);
    }

    /// <summary>
    /// 通知原生展示原生界面
    /// </summary>
    public void ShowHostMainWindow()
    {
        platformSDK.ShowHostMainWindow();
    }

    /// <summary>
    /// 通知原生平台初始化场景结束，切换其他功能场景
    /// </summary>
    public void SendPlatformStartSenseComplete()
    {
        platformSDK.SendPlatformStartSenseComplete();
    }

    /// <summary>
    /// 设置unity显示在ios界面上
    /// </summary>
    public void SendSetUnityViewUpToIosView()
    {
        platformSDK.SendSetUnityViewUpToIosView();
    }

    /// <summary>
    /// 发送潜水艇场景的图片
    /// </summary>
    /// <param name="bytes"></param>
    /// <param name="length"></param>
    public void RefreshSubmarineWithBytes(byte[] bytes)
    {
        platformSDK.RefreshSubmarineWithBytes(bytes, bytes.Length);
    }

    public override void Dispose()
    {
        base.Dispose();
    }
}
