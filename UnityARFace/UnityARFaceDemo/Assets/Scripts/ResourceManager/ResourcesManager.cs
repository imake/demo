using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum enResourceType
{
    Numeric,
    UIForm,
    UIPrefan,
    Sound,
    ScenePrefab,
    UISprite,
    Scene,
    Material,
    Effect,
    AssetText,
}

public enum enResourceState
{
    Unload,
    Loading,
    Loaded
}

public enum enAssetBundleState
{
    Unload,
    Loading,
    Loaded
}



public class ResourcesManager : Singleton<ResourcesManager>
{
    public delegate void OnResourceLoaded(ResourceBase resource);

    public static bool isBattleState;

    private static int s_frameCounter;

    private ResourcePackerInfoSet m_resourcePackerInfoSet;

    private Dictionary<int, ResourceBase> m_cachedResourceMap;

    private bool m_clearUnusedAssets;

    private int m_clearUnusedAssetsExecuteFrame;

    /* 忽略类型 */
    private static readonly List<string> IgnoredAssetTypeExtension = new List<string>
        {
            string.Empty,
            ".manifest",
            ".meta",
            ".assetbundle",
            ".sample",
            //".unitypackage",
            ".cs",
            ".sh",
            ".js",
            ".zip",
            ".tar",
            ".tgz",
        };

    public static bool IsLoadingAsset(string fileExtension)
    {
        return !IgnoredAssetTypeExtension.Contains(fileExtension);
    }

    public override void Init()
    {
        m_resourcePackerInfoSet = null;
        m_cachedResourceMap = new Dictionary<int, ResourceBase>();
    }

    public override void Dispose()
    {
        base.Dispose();
        m_cachedResourceMap.Clear();
    }

    public Dictionary<int, ResourceBase> GetCachedResourceMap()
    {
        return m_cachedResourceMap;
    }

    public bool CheckCachedResource(string fullPathInResources)
    {
        string s = FileManager.EraseExtension(fullPathInResources);
        ResourceBase resourceInfo = null;
        return m_cachedResourceMap.TryGetValue(s.JavaHashCodeIgnoreCase(), out resourceInfo);
    }

    public void CustomUpdate()
    {
        ResourcesManager.s_frameCounter++;
        if (m_clearUnusedAssets && m_clearUnusedAssetsExecuteFrame == ResourcesManager.s_frameCounter)
        {
            ExecuteUnloadUnusedAssets();
            m_clearUnusedAssets = false;
        }
    }

    public GameObject LoadProductAB(string path)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(path);
        DirectoryInfo[] directoryInfos = directoryInfo.GetDirectories();

        for (int j = 0; j < directoryInfos.Length; j++)
        {
            FileInfo[] fileInfos = directoryInfos[j].GetFiles();
            for (int i = 0; i < fileInfos.Length; i++)
            {
                string extension = Path.GetExtension(fileInfos[i].Name);
                Debug.Log("LoadProductAB__ extension=" + extension);
                if (!IsLoadingAsset(extension))
                {
                    continue;
                }
                string name = fileInfos[i].Name.GetFileNameWithoutExtension();

                Debug.Log("name="+ name);

                GameObject go;

                string abName = "resources/prefabs/product/" + fileInfos[i].Name;

                IEnumerable<AssetBundle> assetBundles = AssetBundle.GetAllLoadedAssetBundles();

                foreach (var item in assetBundles)
                {
                    Debug.Log(item.name);

                    if (abName.Equals(item.name))
                    {
                        go = item.LoadAsset<GameObject>(name);
                        return go;
                    }
                }
                
                AssetBundle assetBundle = AssetBundle.LoadFromFile(fileInfos[i].FullName);
                if (assetBundle != null)
                {
                    go = assetBundle.LoadAsset<GameObject>(name);
                    if (go != null)
                    {
                        Debug.Log(fileInfos[i].FullName + " ...assetBundle.Load is Successed!!");
                        return go;
                    }
                    else
                    {
                        Debug.Log(fileInfos[i].FullName + " ...assetBundle.Load is Error!!");
                    }

                    //卸载AB资源
                    AssetBundle.UnloadAllAssetBundles(false);
                    GC.Collect();
                }
                else
                {
                    Debug.Log(fileInfos[i].FullName + " ...AssetBundle.LoadFromFile is Error!!");
                }
            }
        }
        return null;
    }

    /// <summary>
    ///  获取资源基本数据结构对象
    /// </summary>
    /// <param name="fullPathInResources">资源在Resource文件夹下完整路径</param>
    /// <param name="resourceContentType">数据类型</param>
    /// <param name="resourceType">资源类型</param>
    /// <param name="needCached">是否需要缓存</param>
    /// <param name="unloadBelongedAssetBundleAfterLoaded">加载资源后是否卸载资源所在的AB包</param>
    /// <returns>资源基本数据</returns>
    public ResourceBase GetResource(string fullPathInResources, Type resourceContentType, enResourceType resourceType, bool needCached = false, bool unloadBelongedAssetBundleAfterLoaded = false)
    {
        if (string.IsNullOrEmpty(fullPathInResources))
        {
            return new ResourceBase(0, 0, string.Empty, null, resourceType, unloadBelongedAssetBundleAfterLoaded);
        }

        string s = FileManager.EraseExtension(fullPathInResources);

        int key = s.JavaHashCodeIgnoreCase();

        int bundleKey = s.JavaHashCodeIgnoreCase();

        ResourceBase resourceBase = null;

        //缓存取出
        if (m_cachedResourceMap.TryGetValue(key, out resourceBase))
        {
            if (resourceBase.resourceType != resourceType)
            {
                resourceBase.resourceType = resourceType;
            }
            return resourceBase;
        }

        resourceBase = new ResourceBase(key, bundleKey, fullPathInResources, resourceContentType, resourceType, unloadBelongedAssetBundleAfterLoaded);
        try
        {
            LoadResource(resourceBase);
        }
        catch (Exception exception)
        {
            object[] inParameters = new object[] { s };

            Debug.AssertFormat(false, "Failed Load Resource {0}", inParameters);

            throw exception;
        }

        if (needCached)
        {
            m_cachedResourceMap.Add(key, resourceBase);
        }

        return resourceBase;
    }

    public ResourcePackerInfo GetResourceBelongedPackerInfo(string fullPathInResources)
    {
        if (string.IsNullOrEmpty(fullPathInResources))
        {
            return null;
        }
        if (m_resourcePackerInfoSet != null)
        {
            return m_resourcePackerInfoSet.GetResourceBelongedPackerInfo(FileManager.EraseExtension(fullPathInResources).JavaHashCodeIgnoreCase());
        }
        return null;
    }

    private ResourcePackerInfo GetResourceBelongedPackerInfo(ResourceBase resourceBase)
    {
        if (m_resourcePackerInfoSet != null)
        {
            ResourcePackerInfo resourceBelongedPackerInfo = m_resourcePackerInfoSet.GetResourceBelongedPackerInfo(resourceBase.bundleKey);

            return resourceBelongedPackerInfo;
        }
        return null;
    }

    public Type GetResourceContentType(string extension)
    {
        Type result = null;
        if (string.Equals(extension, ".prefab", StringComparison.OrdinalIgnoreCase))
        {
            result = typeof(GameObject);
        }
        else
        {
            if (string.Equals(extension, ".bytes", StringComparison.OrdinalIgnoreCase) || string.Equals(extension, ".xml", StringComparison.OrdinalIgnoreCase))
            {
                result = typeof(TextAsset);
            }
            else
            {
                if (string.Equals(extension, ".asset", StringComparison.OrdinalIgnoreCase))
                {
                    result = typeof(ScriptableObject);
                }
            }
        }
        return result;
    }

    /// <summary>
    /// 加载资源,优先保证从AB包里面(Resources文件夹外部),其次是通过C#IO流加
    /// 载(Resources文件夹外部),最后通过Resources加载((Resources文件夹内存)).
    /// </summary>
    /// <param name="resourceBase">资源基本数据</param>
    private void LoadResource(ResourceBase resourceBase)
    {
        ResourcePackerInfo resourceBelongedPackerInfo = GetResourceBelongedPackerInfo(resourceBase);
   
        if (resourceBelongedPackerInfo != null)
        {
            if (resourceBelongedPackerInfo.isAssetBundle)
            {
                if (!resourceBelongedPackerInfo.IsAssetBundleLoaded())
                {
                    resourceBelongedPackerInfo.LoadAssetBundle(FileManager.GetIFSExtractPath());
                }

                resourceBase.LoadFromAssetBundle(resourceBelongedPackerInfo);

                if (resourceBase.unloadBelongedAssetBundleAfterLoaded)
                {
                    resourceBelongedPackerInfo.UnloadAssetBundle(false);
                }
            }
            else
            {
                resourceBase.Load(FileManager.GetIFSExtractPath());
            }
        }
        else
        {
            Debug.Log(resourceBase.Name + " ...isResourceLoad!!");
            resourceBase.Load();
        }
    }

    public void LoadResourcePackerInfoSet()
    {
        if (this.m_resourcePackerInfoSet != null)
        {
            this.m_resourcePackerInfoSet.Dispose();

            this.m_resourcePackerInfoSet = null;
        }

        string filePath = FileManager.CombinePath(FileManager.GetResourcePath(), VersionUpdateSystem.ResourceVersionNumberFile);

        if (FileManager.IsFileExist(filePath))
        {
            string strData = ProtobufSerializer<string>.Deserialize(filePath);

            filePath = FileManager.CombinePath(FileManager.GetResourcePath(), strData + ".bytes");

            strData = ProtobufSerializer<string>.Deserialize(filePath);

            Dictionary<string, AssetBundleNode> assetBundleNodeMap = JsonMapper.ToObject<Dictionary<string, AssetBundleNode>>(strData);

            this.m_resourcePackerInfoSet = new ResourcePackerInfoSet()
            {
                ifsPath = FileManager.GetResourcePath(),
            };

            foreach (KeyValuePair<string, AssetBundleNode> node in assetBundleNodeMap)
            {
                ResourcePackerInfo resourceInfo = new ResourcePackerInfo()
                {
                    isAssetBundle = true,

                    pathInIFS = node.Value.AssetPath + AssetBundleConfig.FILE_EXTENSION,

                    resourcesIFS = node.Value.ResourcePath + AssetBundleConfig.FILE_EXTENSION,

                    resident = false,

                    useAsyncLoadingData = true,
                };

                if (resourceInfo.resourcesIFS.StartsWith("Resources/"))
                {
                    resourceInfo.resourcesIFS = resourceInfo.resourcesIFS.Substring(("Resources/").Length);
                }

                resourceInfo.resourcesIFS = resourceInfo.resourcesIFS.GetFilePathWithoutExtension();

                this.m_resourcePackerInfoSet.AddResourcePackerInfo(resourceInfo);
            }
        }
        else
        {
#if UNITY_EDITOR
            Debug.Log("LoadResourcePackerInfoSet   " + filePath);
#endif
        }
    }

    public void LoadResourcePackerInfoSet(List<AssetBundleNode> assetBundleNodes)
    {
        for (int i = 0; i < assetBundleNodes.Count; i++)
        {
            ResourcePackerInfo resourceInfo = new ResourcePackerInfo()
            {
                isAssetBundle = true,

                pathInIFS = assetBundleNodes[i].AssetPath + AssetBundleConfig.FILE_EXTENSION,

                resourcesIFS = assetBundleNodes[i].ResourcePath + AssetBundleConfig.FILE_EXTENSION,

                resident = false,

                useAsyncLoadingData = true,
            };

            if (resourceInfo.resourcesIFS.StartsWith("Resources/"))
            {
                resourceInfo.resourcesIFS = resourceInfo.resourcesIFS.Substring(("Resources/").Length).GetFilePathWithoutExtension();
            }

            this.m_resourcePackerInfoSet.AddResourcePackerInfo(resourceInfo);
        }
    }

    public void RemoveAllCachedResources()
    {
        this.RemoveCachedResources((enResourceType[])Enum.GetValues(typeof(enResourceType)));
    }

    public void RemoveCachedResource(string fullPathInResources)
    {
        string s = FileManager.EraseExtension(fullPathInResources);
        int key = s.JavaHashCodeIgnoreCase();
        ResourceBase resourceBase = null;
        if (m_cachedResourceMap.TryGetValue(key, out resourceBase))
        {
            resourceBase.Unload();
            this.m_cachedResourceMap.Remove(key);
        }
    }

    public void RemoveCachedResources(enResourceType[] resourceTypes)
    {
        for (int i = 0; i < resourceTypes.Length; i++)
        {
            RemoveCachedResources(resourceTypes[i], false);
        }
        UnloadAllAssetBundles();
        UnloadUnusedAssets();
    }

    public void RemoveCachedResources(enResourceType resourceType, bool clearImmediately = true)
    {
        List<int> list = new List<int>();
        Dictionary<int, ResourceBase>.Enumerator enumerator = m_cachedResourceMap.GetEnumerator();
        while (enumerator.MoveNext())
        {
            KeyValuePair<int, ResourceBase> current = enumerator.Current;
            ResourceBase value = current.Value;
            if (value.resourceType == resourceType)
            {
                value.Unload();
                list.Add(value.key);
            }
        }
        for (int i = 0; i < list.Count; i++)
        {
            m_cachedResourceMap.Remove(list[i]);
        }
        if (clearImmediately)
        {
            UnloadAllAssetBundles();
            UnloadUnusedAssets();
        }
    }

    private void ExecuteUnloadUnusedAssets()
    {
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    private void UnloadAllAssetBundles()
    {
        if (m_resourcePackerInfoSet == null)
        {
            return;
        }
        for (int i = 0; i < m_resourcePackerInfoSet.resourcePackerInfos.Count; i++)
        {
            ResourcePackerInfo resourcePackerInfo = m_resourcePackerInfoSet.resourcePackerInfos[i];
            if (resourcePackerInfo.IsAssetBundleLoaded())
            {
                resourcePackerInfo.UnloadAssetBundle(false);
            }
        }
    }

    public void UnloadBelongedAssetbundle(string fullPathInResources)
    {
        ResourcePackerInfo resourceBelongedPackerInfo = GetResourceBelongedPackerInfo(fullPathInResources);
        if (resourceBelongedPackerInfo != null && resourceBelongedPackerInfo.IsAssetBundleLoaded())
        {
            resourceBelongedPackerInfo.UnloadAssetBundle(false);
        }
    }

    public void UnloadUnusedAssets()
    {
        m_clearUnusedAssets = true;
        m_clearUnusedAssetsExecuteFrame = ResourcesManager.s_frameCounter + 1;
    }

    public IEnumerator LoadResidentAssetBundles()
    {
        if (m_resourcePackerInfoSet != null)
        {
            int i = 0;

            while (i < m_resourcePackerInfoSet.resourcePackerInfos.Count)
            {
                ResourcePackerInfo resourcePackerInfo = m_resourcePackerInfoSet.resourcePackerInfos[i];

                if ((resourcePackerInfo.isAssetBundle && resourcePackerInfo.IsResident()) && !resourcePackerInfo.IsAssetBundleLoaded())
                {
                    yield return resourcePackerInfo.AsyncLoadAssetBundle(FileManager.GetResourcePath());

                }
                i++;
            }
        }
        yield return new WaitForEndOfFrame();
        //return new LoadResidentAssetBundlesEnumertor { _this = this };
    }


    /* 加载内存AB */
    private sealed class LoadResidentAssetBundlesEnumertor : IDisposable, IEnumerator, IEnumerator<object>
    {
        internal object current;
        internal int PC;
        internal ResourcesManager _this;
        internal int i;
        internal ResourcePackerInfo resourcePackerInfo;

        public void Dispose()
        {
            this.PC = -1;
        }

        public bool MoveNext()
        {
            uint num = (uint)this.PC;

            this.PC = -1;

            switch (num)
            {
                case 0:
                    if (this._this.m_resourcePackerInfoSet != null)
                    {
                        this.i = 0;

                        while (this.i < this._this.m_resourcePackerInfoSet.resourcePackerInfos.Count)
                        {
                            this.resourcePackerInfo = this._this.m_resourcePackerInfoSet.resourcePackerInfos[this.i];

                            if ((this.resourcePackerInfo.isAssetBundle && this.resourcePackerInfo.IsResident()) && !this.resourcePackerInfo.IsAssetBundleLoaded())
                            {
                                this.resourcePackerInfo.LoadAssetBundle(FileManager.GetResourcePath());

                                this.current = null;

                                this.PC = 1;

                                return true;
                            }

                            this.i++;
                        }

                        this.PC = -1;

                        break;
                    }
                    break;

                case 1:
                    goto Label_00B6;
            }

        Label_00B6:
            this.i++;
            return false;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        object IEnumerator<object>.Current
        {
            get
            {
                return this.current;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.current;
            }
        }
    }
}

public class AssetBundleInfoNode
{
    public string assetName;

    public string assetPath;

    public List<string> depences = new List<string>();
}

public static class BundleConfig
{
    public const string CONFIG_FILE_NAME = "AssetsConfig" + FILE_EXTENSION;

    public const string FILE_EXTENSION = ".asset";

    public static string BUNDLE_REMOTE_PATH = Application.dataPath.Replace("Assets", "") + "Bundles";

    public static string LOCALE_BUNDLE_FLODER_PATH = Application.streamingAssetsPath;

    public static string LOADPATH = Application.dataPath + "/AssetBundles";
}

public static class BundleTool
{
    public static string GetAssetName(string path)
    {
        int index = path.LastIndexOf('/');

        return index < 0 ? path : path.Substring(index + 1);
    }

    public static string GetBundleFileName(string assetName)
    {
        return assetName + BundleConfig.FILE_EXTENSION;
    }

    public static string GetBundleFilePath(string assetName)
    {
        return BundleConfig.LOCALE_BUNDLE_FLODER_PATH + "/" + GetBundleFileName(assetName);
    }
}

public class ResourceVersionInfo
{
    /* 版本号 */
    public string versionNumber = "";
    /* 下载URL */
    public string versionUrl = "";
    /* 需要下载的文件 */
    public List<string> files = new List<string>();
    /* 文件MD5 */
    public Dictionary<string, string> fileMd5Map = new Dictionary<string, string>();
    /* 单个文件大小 */
    public Dictionary<string, int> fileSizeMap = new Dictionary<string, int>();

    public Dictionary<string, AssetBundleInfoNode> mainAssetBundles = new Dictionary<string, AssetBundleInfoNode>();

    public Dictionary<string, string> resourceToAssetBundles = new Dictionary<string, string>();

    public ResourceVersionInfo()
    {
        versionNumber = "";

        versionUrl = "";

        files = new List<string>();

        fileMd5Map = new Dictionary<string, string>();

        fileSizeMap = new Dictionary<string, int>();

        mainAssetBundles = new Dictionary<string, AssetBundleInfoNode>();

        resourceToAssetBundles = new Dictionary<string, string>();
    }
}
