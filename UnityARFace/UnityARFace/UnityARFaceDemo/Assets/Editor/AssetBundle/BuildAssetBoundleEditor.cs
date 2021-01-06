using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class BuildAssetBoundleEditor
{
    [MenuItem("[FC Project]/资源热更/Build AssetBundle Windows", false, 1)]
    public static void BuildAssetBundle_Window()
    {
        //UnityEditor.BuildPipeline.BuildAssetBundles(AssetBundleConfig.BUILD_ASSETS_SAVE_PATH, BuildAssetBundleOptions.None, BuildTarget.Android);

        //return;

#if UNITY_EDITOR || UNITY_EDITOR_WIN

        Init();

        //导出AB资源包
        List<AssetBundleNode> list = AssetBundleBuilder. BundleAssetBundle(BuildTarget.StandaloneWindows);

        //导出AB资源配置文件
        ExplortAssetBundleConfig(list, AssetBundleConfig.BUILD_ASSET_VERSION_INFO_PATH);

#else
            Debug.LogError("当前平台不是Windows,请切换!");
#endif

        End();

        //刷新目录
        AssetDatabase.Refresh();
    }

    [MenuItem("[FC Project]/资源热更/Build AssetBundle Android", false, 2)]
    public static void BuildAssetBundle_Android()
    {

#if UNITY_ANDROID

            Init();

            List<AssetBundleNode> list = AssetBundleBuilder.BundleAssetBundle(BuildTarget.Android);

            ExplortAssetBundleConfig(list, AssetBundleConfig.BUILD_ASSET_VERSION_INFO_PATH);

#else

        Debug.LogError("当前平台不是Android,请切换!");

#endif

        End();

        //Debug.LogWarning("!!!!!!!!!!!!!!");

        AssetDatabase.Refresh();
    }

    [MenuItem("[FC Project]/资源热更/Build AssetBundle IOS", false, 3)]
    public static void BuildAssetBundle_IOS()
    {

#if UNITY_IOS

            Init();

            List<AssetBundleNode> list = AssetBundleBuilder.BundleAssetBundle(BuildTarget.iOS);

            ExplortAssetBundleConfig(list, AssetBundleConfig.BUILD_ASSET_VERSION_INFO_PATH);

#else

        Debug.LogError("当前平台不是IOS,请切换!");

#endif

        End();

        AssetDatabase.Refresh();
    }

    private static void Init()
    {
        GetLastVersionNumber();
    }

    private static string GetLastVersionNumber()
    {
        string versionNumber = ProtobufSerializer<string>.Deserialize(AssetBundleConfig.BUILD_ASSET_VERSION_NUMBER_PATH);

        if (string.IsNullOrEmpty(versionNumber))
        {
            versionNumber = "0.0.0";
        }

        return versionNumber;
    }

    private static void ExplortAssetBundleConfig(List<AssetBundleNode> list, string exportPath)
    {
        Dictionary<string, AssetBundleNode> map = new Dictionary<string, AssetBundleNode>();

        foreach (AssetBundleNode assetRefrenceNode in list)
        {
            string filePath = AssetBundleConfig.BUILD_ASSETS_SAVE_PATH.CombinePath(assetRefrenceNode.AssetPath + AssetBundleConfig.FILE_EXTENSION);

            Debug.Log(filePath);

            if (!FileManager.IsFileExist(filePath))
            {
                Debug.LogError(filePath);

                continue;
            }

            assetRefrenceNode.AssetMD5 = FileManager.GetFileMd5(filePath);

            assetRefrenceNode.AssetSize = FileManager.GetFileSize(filePath);

            assetRefrenceNode.IsDefaultLoad = assetRefrenceNode.AssetSize >= (50 * 1024); // 10KB

            if (assetRefrenceNode.resourcePaths.Count > 0)
            {
                map.Add(assetRefrenceNode.AssetPath, assetRefrenceNode);
            }

            if (filePath.Contains(("HomeSafe").ToLower()))
            {
                assetRefrenceNode.assetType = enAssetType.HOMESAFE;
            }

            if (filePath.Contains(("RedHat").ToLower()))
            {
                assetRefrenceNode.assetType = enAssetType.REDHAT;
            }

            if (filePath.Contains(("HareTortoise").ToLower()))
            {
                assetRefrenceNode.assetType = enAssetType.HARETORTOISE;
            }
        }

        string dataStr = LitJson.JsonMapper.ToJson(map);

        string configPath = exportPath.CombinePath(SetVersionNumber() + ".bytes");

        if (File.Exists(configPath))
        {
            File.Delete(configPath);
        }

        ProtobufSerializer<string>.Serialize(dataStr, configPath);

        //Debug.LogWarning("~~~~~~~~~~~~~~~~~~~~~~~");
    }

    private static void End()
    {
        //FileManager.DeleteDirectory(FileManager.GetResourcePath());

        //FileManager.MoveDirectory(AssetBundleConfig.BUILD_ASSETS_SAVE_PATH, FileManager.GetResourcePath());

        //FileManager.CopyDirectory(AssetBundleConfig.BUILD_ASSETS_SAVE_PATH, FileManager.GetResourcePath());

        //FileManager.CopyDirectory(AssetBundleConfig.BUILD_ASSET_VERSION_INFO_PATH, FileManager.GetResourcePath());

        //FileManager.CopyDirectory(AssetBundleConfig.BUILD_ASSET_VERSION_INFO_PATH, FileManager.GetResourcePath());
    }

    private static string SetVersionNumber()
    {
        string versionNumber = ProtobufSerializer<string>.Deserialize(AssetBundleConfig.BUILD_ASSET_VERSION_NUMBER_PATH);

        if (string.IsNullOrEmpty(versionNumber))
        {
            ProtobufSerializer<string>.Serialize("0.0.0", AssetBundleConfig.BUILD_ASSET_VERSION_NUMBER_PATH);

            versionNumber = "0.0.0";
        }
        else
        {
            int i = int.Parse(versionNumber[0].ToString());

            int j = int.Parse(versionNumber[2].ToString());

            int k = int.Parse(versionNumber[4].ToString());

            k++;

            versionNumber = string.Format("{0}.{1}.{2}", i, j, k);

            ProtobufSerializer<string>.Serialize(versionNumber, AssetBundleConfig.BUILD_ASSET_VERSION_NUMBER_PATH);
        }

        return versionNumber;
    }
}
