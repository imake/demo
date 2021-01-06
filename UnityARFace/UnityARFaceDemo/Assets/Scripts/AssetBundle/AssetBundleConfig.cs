using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AssetBundleConfig
{
    /// <summary>
    /// Asset后缀名
    /// </summary>
    public const string FILE_EXTENSION = ".unity3d";

    /// <summary>
    /// Asset需要打包路径-文件夹形式打包
    /// </summary>
    public static string[] BUILD_ASSET_PATH_FOLDER
    {
        get
        {
            return new string[]
            {
                //Application.dataPath.CombinePath("Resources/Databin/Text"),
            };
        }
    }

    /// <summary>
    /// Asset需要打包路径-单个文件形式打包
    /// </summary>
    public static string[] BUILD_ASSET_PATH_FILE
    {
        get
        {
            return new string[]
            {
                Application.dataPath.CombinePath("Resources/Prefabs/Product"),
                //Application.dataPath.CombinePath("Resources/Prefabs/Reporter"),
                //Application.dataPath.CombinePath("Resources/Effect/HomeSafe"),
                //Application.dataPath.CombinePath("Resources/Effect/RedHat"),
                //Application.dataPath.CombinePath("Resources/Effect/Lobby"),
                //Application.dataPath.CombinePath("Resources/Scenes/HomeSafe"),
                //Application.dataPath.CombinePath("Resources/Scenes/Lobby"),
                //Application.dataPath.CombinePath("Resources/Scenes/RedHat"),
                //Application.dataPath.CombinePath("Resources/Scenes/HareTortoise"),
                //Application.dataPath.CombinePath("Resources/Sound/SE/HomeSafe"),
                //Application.dataPath.CombinePath("Resources/Sound/SE/Lobby"),
                //Application.dataPath.CombinePath("Resources/Sound/SE/RedHat"),
                //Application.dataPath.CombinePath("Resources/Sound/SE/HareTortoise"),
                //Application.dataPath.CombinePath("Resources/UGUI/Atlas/Common"),
                //Application.dataPath.CombinePath("Resources/UGUI/Form/System/HomeSafe"),
                //Application.dataPath.CombinePath("Resources/UGUI/Form/System/Lobby"),
                //Application.dataPath.CombinePath("Resources/UGUI/Form/System/RedHat"),
                //Application.dataPath.CombinePath("Resources/UGUI/Form/System/HareTortoise"),
            };
        }
    }

    /// <summary>
    /// Asset打包保存路径
    /// </summary>
    public static string BUILD_ASSETS_SAVE_PATH = Application.dataPath.CombinePath("AssetBundles");

    /// <summary>
    /// Asset打包资源版本号路径
    /// </summary>
    public static string BUILD_ASSET_VERSION_NUMBER_PATH
    {
        get
        {
#if UNITY_STANDALONE_WIN

            string path = Application.dataPath.CombinePath("Resources/ResourceVersion/Window");

            FileManager.CreateDirectory(path);

            return path.CombinePath("ResourceVersion.bytes");

#elif UNITY_ANDROID

            string path = Application.dataPath.CombinePath("Resources/ResourceVersion/Android");

            FileManager.CreateDirectory(path);

            return path.CombinePath("ResourceVersion.bytes");

#elif UNITY_IPHONE

            string path = Application.dataPath.CombinePath("Resources/ResourceVersion/IOS");

            FileManager.CreateDirectory(path);

            return path.CombinePath("ResourceVersion.bytes");
#endif

            return Application.dataPath.CombinePath("Resources/ResourceVersion").CombinePath("ResourceVersion.bytes");
        }
    }

    /// <summary>
    /// Asset打包保存版本信息路径
    /// </summary>
    public static string BUILD_ASSET_VERSION_INFO_PATH
    {
        get
        {
#if UNITY_STANDALONE_WIN

            string path = Application.dataPath.CombinePath("Resources/ResourceVersion/Window");

            FileManager.CreateDirectory(path);

            return path;

#elif UNITY_ANDROID

            string path = Application.dataPath.CombinePath("Resources/ResourceVersion/Android");

            FileManager.CreateDirectory(path);

            return path;
#elif UNITY_IPHONE

            string path = Application.dataPath.CombinePath("Resources/ResourceVersion/IOS");

            FileManager.CreateDirectory(path);

            return path;
#endif

            return Application.dataPath.CombinePath("Resources/ResourceVersion");
        }
    }

    /// <summary>
    /// 打包之后删除的文件夹
    /// </summary>
    public static List<string> BUILD_DELETE_FOLDER = new List<string>
    {
        "Resources/Databin/TableRes",
        "Resources/Effect",
        "Resources/Scenes",
        "Resources/Sound/SE/HomeSafe",
        "Resources/Sound/SE/Lobby",
        "Resources/Sound/SE/RedHat",
        "Resources/Sound/SE/HareTortoise",
        "Resources/UGUI/Form/System/HomeSafe",
        "Resources/UGUI/Form/System/Lobby",
        "Resources/UGUI/Form/System/RedHat",
        "Resources/UGUI/Form/System/HareTortoise",
    };
}
