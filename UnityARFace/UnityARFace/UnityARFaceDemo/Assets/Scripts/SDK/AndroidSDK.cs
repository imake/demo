using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AndroidSDK : IPlatformSDK
{
    private AndroidJavaClass javaClass;
    private AndroidJavaObject javaObject;

    public override void Init()
    {
        javaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        javaObject = javaClass.GetStatic<AndroidJavaObject>("currentActivity");
    }

    public override void RefreshWithBytes(byte[] bytes, int length)
    {
        javaObject.Call("refreshWithBytes", bytes, length);

    }

    public override void ShowHostMainWindow()
    {
        javaObject.Call("showHostMainWindow"); 
    }

    public override void SendPlatformStartSenseComplete()
    {

    }
}
