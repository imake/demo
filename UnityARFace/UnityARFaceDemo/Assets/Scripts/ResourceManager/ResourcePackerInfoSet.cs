using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcePackerInfoSet 
{
    /// <summary>
    /// 打包编号
    /// </summary>
    public string buildNumber;

    /// <summary>
    /// 资源路径
    /// </summary>
    public string ifsPath;

    /// <summary>
    /// publish
    /// </summary>
    public string publish;

    /// <summary>
    /// 版本号
    /// </summary>
    public string version;

    public List<ResourcePackerInfo> resourcePackerInfos = new List<ResourcePackerInfo>();

    private Dictionary<int, ResourcePackerInfo> resourcePackerInfoMap = new Dictionary<int, ResourcePackerInfo>();

    public ResourcePackerInfoSet()
    {
        this.buildNumber = "";

        this.ifsPath = "";

        this.publish = "";

        this.version = "";

        this.resourcePackerInfos = new List<ResourcePackerInfo>();
    }

    public void AddResourcePackerInfo(ResourcePackerInfo resourceInfo)
    {
        resourcePackerInfos.Add(resourceInfo);

        resourcePackerInfoMap[FileManager.EraseExtension(resourceInfo.resourcesIFS).JavaHashCodeIgnoreCase()] = resourceInfo;

        for (int i = 0; i < resourceInfo.childrens.Count; ++i)
        {
            AddResourcePackerInfoAll(resourceInfo.childrens[i]);
        }
    }

    private void AddResourcePackerInfoAll(ResourcePackerInfo resourceInfo)
    {
        resourcePackerInfos.Add(resourceInfo);

        for (int i = 0; i < resourceInfo.childrens.Count; i++)
        {
            AddResourcePackerInfoAll(resourceInfo.childrens[i]);
        }
    }

    public ResourcePackerInfo GetResourceBelongedPackerInfo(int resourceKeyHash)
    {
        ResourcePackerInfo info = null;

        if (resourcePackerInfoMap.TryGetValue(resourceKeyHash, out info))
        {
            return info;
        }

        return null;
    }

    public void CreateResourceMap()
    {
        for (int i = 0; i < resourcePackerInfos.Count; ++i)
        {
            resourcePackerInfos[i].AddToResourceMap(resourcePackerInfoMap);
        }
    }

    public void Dispose()
    {
        for (int i = 0; i < resourcePackerInfos.Count; ++i)
        {
            if (resourcePackerInfos[i].IsAssetBundleLoaded())
            {
                resourcePackerInfos[i].UnloadAssetBundle(false);
            }
            resourcePackerInfos[i] = null;
        }

        resourcePackerInfos.Clear();

        resourcePackerInfoMap.Clear();
    }
}
