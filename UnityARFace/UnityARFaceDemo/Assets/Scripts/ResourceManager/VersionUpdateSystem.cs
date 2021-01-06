using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VersionUpdateSystem : SingletonMono<VersionUpdateSystem>
{
    /* 资源版本号文件 */
    public readonly static string ResourceVersionNumberFile = "ResourceVersion.bytes";
}
