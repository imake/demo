using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IPlatformSDK
{
    public abstract void Init();


    public abstract void RefreshWithBytes(byte[] bytes, int length);

    public abstract void ShowHostMainWindow();

    public abstract void SendPlatformStartSenseComplete();
}
