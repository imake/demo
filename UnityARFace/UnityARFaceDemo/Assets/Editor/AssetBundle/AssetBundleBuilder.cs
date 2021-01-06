using Assets.Editor.AssetBundleBuilder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class AssetBundleBuilder
{
    public static string bundlePath;

    public static List<AssetBundleNode> BundleAssetBundle(BuildTarget buildTarget)
    {
        List<AssetBundleNode> nodes = new List<AssetBundleNode>();

        List<AssetBundleBuild> assetBundleBuilds = new List<AssetBundleBuild>();

        //以文件夹形式打包
        //for (int i = 0; i < AssetBundleConfig.BUILD_ASSET_PATH_FOLDER.Length; i++)
        //{
        //    bundlePath = AssetBundleConfig.BUILD_ASSET_PATH_FOLDER[i];

        //    List<string> rootFiles = LoadRootAssets_Folder(bundlePath);

        //    List<AssetBundleNode> nodeList = CreateAssetBundleNode_Folder(rootFiles);

        //    for (int j = 0; j < nodeList.Count; j++)
        //    {
        //        nodes.Add(nodeList[j]);
        //    }

        //    AssetBundleBuild[] bundleBuildArray = ConvertToBundleBulid_Folder(nodeList);

        //    for (int j = 0; j < bundleBuildArray.Length; j++)
        //    {
        //        assetBundleBuilds.Add(bundleBuildArray[j]);
        //    }
        //}

        //以单个文件形式打包
        for (int i = 0; i < AssetBundleConfig.BUILD_ASSET_PATH_FILE.Length; i++)
        {
            bundlePath = AssetBundleConfig.BUILD_ASSET_PATH_FILE[i];

            //获取每个文件的路径
            List<string> rootFiles = LoadRootAssets_File(bundlePath);

            //创建每个文件的AssetBundleNode
            List<AssetBundleNode> nodeList = CreateAssetBundleNode_File(rootFiles);

            for (int j = 0; j < nodeList.Count; j++)
            {
                //Debug.Log("nodeList " + nodeList[i].ResourcePath);
                nodes.Add(nodeList[j]);
            }

            AssetBundleBuild[] bundleBuildArray = ConvertToBundleBulid_File(nodeList);

            for (int j = 0; j < bundleBuildArray.Length; j++)
            {
                assetBundleBuilds.Add(bundleBuildArray[j]);
            }
        }

        DoBuild(AssetBundleConfig.BUILD_ASSETS_SAVE_PATH, assetBundleBuilds.ToArray(), BuildAssetBundleOptions.None, buildTarget);

        return nodes;
    }

    private static List<string> LoadRootAssets_Folder(string path)
    {
        List<string> list = new List<string>();

        Stack<DirectoryInfo> floderStack = new Stack<DirectoryInfo>();

        floderStack.Push(new DirectoryInfo(Utility.GetFullPath(path)));

        while (floderStack.Count > 0)
        {
            DirectoryInfo floder = floderStack.Pop();

            list.Add(floder.FullName.Replace('\\', '/'));

            foreach (DirectoryInfo directoryInfo in floder.GetDirectories())
            {
                floderStack.Push(directoryInfo);
            }
        }

        return list;
    }

    private static List<AssetBundleNode> CreateAssetBundleNode_Folder(List<string> rootList)
    {
        List<AssetBundleNode> nodes = new List<AssetBundleNode>();

        for (int i = 0; i < rootList.Count; i++)
        {
            AssetBundleNode assetBundleNode = new AssetBundleNode()
            {
                AssetName = FileManager.GetNameFile(rootList[i]).ToLower(),

                ResourcePath = rootList[i].Substring(Application.dataPath.Length + 1),

                AssetPath = rootList[i].Substring(Application.dataPath.Length + 1).ToLower(),

                AssetMD5 = FileManager.GetMd5(rootList[i]),

                AssetSize = FileManager.GetFileSize(rootList[i]),

                resourcePaths = new List<string>(),

                depence = new List<string>(),

                parents = new List<string>(),
            };

            nodes.Add(assetBundleNode);
        }

        return nodes;
    }

    private static AssetBundleBuild[] ConvertToBundleBulid_Folder(List<AssetBundleNode> nodeList)
    {
        List<AssetBundleBuild> list = new List<AssetBundleBuild>();
        /* 文件夹形式打包 */
        foreach (AssetBundleNode node in nodeList)
        {
            AssetBundleBuild assetBundleBuild;

            string path = node.ResourcePath;

            string name = node.AssetName;

            List<string> assetNames = new List<string>();

            string[] files = Directory.GetFiles(Application.dataPath.CombinePath(path));

            for (int i = 0; i < files.Length; i++)
            {
                files[i] = files[i].Replace('\\', '/');

                if (Utility.IsLoadingAsset(Path.GetExtension(files[i])))
                {
                    node.resourcePaths.Add(files[i].Substring(bundlePath.Length + 1));

                    files[i] = "Assets/" + files[i].Substring(Application.dataPath.Length + 1);

                    assetNames.Add(files[i]);
                }
            }

            assetBundleBuild = new AssetBundleBuild()
            {
                assetNames = assetNames.ToArray(),

                assetBundleName = (path + AssetBundleConfig.FILE_EXTENSION).ToLower(),
            };

            list.Add(assetBundleBuild);
        }

        return list.ToArray();
    }

    private static List<string> LoadRootAssets_File(string path)
    {
        List<string> list = new List<string>();

        Stack<DirectoryInfo> floderStack = new Stack<DirectoryInfo>();

        floderStack.Push(new DirectoryInfo(Utility.GetFullPath(path)));

        while (floderStack.Count > 0)
        {
            DirectoryInfo floder = floderStack.Pop();

            Debug.Log(floder);

            FileInfo[] files = floder.GetFiles();

            for (int i = 0; i < files.Length; i++)
            {
                Debug.Log(files[i].FullName.Replace('\\', '/'));
                list.Add(files[i].FullName.Replace('\\', '/'));
            }

            foreach (DirectoryInfo directoryInfo in floder.GetDirectories())
            {
                floderStack.Push(directoryInfo);
            }
        }

        return list;
    }

    private static List<AssetBundleNode> CreateAssetBundleNode_File(List<string> rootList)
    {
        List<AssetBundleNode> nodes = new List<AssetBundleNode>();

        for (int i = 0; i < rootList.Count; i++)
        {
            string extension = Path.GetExtension(rootList[i]);

            //Debug.Log("rootList "+ rootList[i]+" extension " + extension);

            if (!Utility.IsLoadingAsset(extension))
            {
                continue;
            }

            AssetBundleNode assetBundleNode = new AssetBundleNode()
            {
                AssetName = FileManager.GetNameFile(rootList[i]).ToLower(),

                ResourcePath = rootList[i].Substring(Application.dataPath.Length + 1),

                AssetPath = rootList[i].Substring(Application.dataPath.Length + 1).ToLower().GetFilePathWithoutExtension(),

                AssetMD5 = FileManager.GetMd5(rootList[i]),

                AssetSize = FileManager.GetFileSize(rootList[i]),

                resourcePaths = new List<string>(),

                depence = new List<string>(),

                parents = new List<string>(),
            };

            //Debug.Log("assetBundleNode "+ assetBundleNode.ResourcePath);

            nodes.Add(assetBundleNode);
        }

        return nodes;
    }

    private static AssetBundleBuild[] ConvertToBundleBulid_File(List<AssetBundleNode> nodeList)
    {
        List<AssetBundleBuild> list = new List<AssetBundleBuild>();

        foreach (AssetBundleNode node in nodeList)
        {
            AssetBundleBuild assetBundleBuild;

            string path = node.ResourcePath;

            Debug.Log(path);

            string name = node.AssetName;

            string filesPath = Application.dataPath.CombinePath(path);

            //string[] files = Directory.GetFiles(filesPath);

            string[] files = new string[] { filesPath };

            node.ResourcePath = node.ResourcePath.GetFilePathWithoutExtension();

            Debug.Log("files "+ files.Length);

            //注意
            for (int i = 0; i < files.Length; i++)
            {
                files[i] = files[i].Replace('\\', '/');

                if (Utility.IsLoadingAsset(Path.GetExtension(files[i])))
                {
                    node.resourcePaths.Add(files[i].Substring(Application.dataPath.Length + 1));

                    files[i] = "Assets/" + files[i].Substring(Application.dataPath.Length + 1);

                    //Debug.Log("files[i] "+i+" "+files[i]);

                    assetBundleBuild = new AssetBundleBuild()
                    {
                        assetNames = new string[] { files[i] },

                        assetBundleName = (path.GetFilePathWithoutExtension() + AssetBundleConfig.FILE_EXTENSION).ToLower(),
                    };

                    //Debug.Log("assetBundleBuild.assetNames=" + assetBundleBuild.assetNames[0]+ " assetBundleBuild.assetBundleName="+ assetBundleBuild.assetBundleName);
                    list.Add(assetBundleBuild);
                }
            }
        }

        return list.ToArray();
    }

    private static void DoBuild(string ouputPath, AssetBundleBuild[] builds, BuildAssetBundleOptions assetBundleOptions, BuildTarget targetPlatform)
    {
        FileManager.CreateDirectory(ouputPath);

        BuildPipeline.BuildAssetBundles(ouputPath, builds, assetBundleOptions, targetPlatform);
    }
}
