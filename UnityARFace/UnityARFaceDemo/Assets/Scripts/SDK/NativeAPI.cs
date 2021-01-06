using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class NativeAPI : IPlatformSDK
{
    /// <summary>
    /// 由于U3D无法直接调用Objc或者Swift语言声明的接口，
    /// U3D的主要语言是C#，因此可以利用C#的特性来访问C语言所定义的接口，
    /// 然后再通过C接口再调用ObjC的代码（对于Swift代码则还需要使用OC桥接）。
    /// 
    /// 其中DllImport为一个Attribute，目的是通过非托管方式将库中的方法导出到C#中进行使用。
    /// 而传入"__Internal"则是表示这个是一个静态库或者是一个内部方法。通过上面的声明，这个方法就可以在C#里面进行调用了。
    /// </summary>

#if UNITY_IOS
    [DllImport("__Internal")]
    public static extern void showHostMainWindow(string lastStringColor);

    [DllImport("__Internal")]
    private static extern void refreshWithBytes(byte[] bytes, int length);

    [DllImport("__Internal")]
    private static extern void sendPlatformStartSenseComplete();
#endif

    public override void Init()
    {
        
    }


    public override void RefreshWithBytes(byte[] bytes, int length)
    {
#if UNITY_IOS
        refreshWithBytes(bytes, length);
#endif

    }

    public override void ShowHostMainWindow()
    {
#if UNITY_IOS
        showHostMainWindow("red");
#endif
    }

    public override void SendPlatformStartSenseComplete()
    {
#if UNITY_IOS
        sendPlatformStartSenseComplete();
#endif
    }
}
