using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ResourcePackerInfo 
{
    /// <summary>
    /// 是否是AssetBundle资源
    /// </summary>
    public bool isAssetBundle;

    public string pathInIFS;

    public string resourcesIFS;

    public bool resident;

    public bool useAsyncLoadingData;

    private enAssetBundleState assetBundleState;

    private AssetBundle m_assetBundle;

    private ResourcePackerInfo m_parent;

    private List<int> m_resourceInfos = new List<int>();

    public List<ResourcePackerInfo> childrens = new List<ResourcePackerInfo>();

    public ResourcePackerInfo dependency
    {
        get
        {
            return m_parent;
        }
        set
        {
            m_parent = value;

            childrens.Add(this);
        }
    }

    public AssetBundle assetBundle
    {
        get { return m_assetBundle; }
    }

    public ResourcePackerInfo()
    {

    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="isAssetBundle">是否是AssetBundle资源</param>
    public ResourcePackerInfo(bool isAssetBundle)
    {
        this.isAssetBundle = isAssetBundle;

        this.assetBundleState = enAssetBundleState.Unload;

        this.useAsyncLoadingData = false;

        this.childrens = new List<ResourcePackerInfo>();
    }

    public bool IsResident()
    {
        return dependency == null && resident;
    }

    public bool IsAssetBundleLoaded()
    {
        return isAssetBundle && assetBundleState == enAssetBundleState.Loaded;
    }

    public void AddToResourceMap(Dictionary<int, ResourcePackerInfo> map)
    {
        for (int i = 0; i < m_resourceInfos.Count; ++i)
        {
            if (!map.ContainsKey(m_resourceInfos[i]))
            {
                map.Add(m_resourceInfos[i], this);
            }
        }
    }

    public void LoadAssetBundle(string ifsExtractPath)
    {
        if (!isAssetBundle)
        {
            return;
        }
        if (dependency != null && dependency.isAssetBundle && !dependency.IsAssetBundleLoaded())
        {
            dependency.LoadAssetBundle(ifsExtractPath);
        }
        if (assetBundleState != enAssetBundleState.Unload)
        {
            return;
        }
        useAsyncLoadingData = false;

        string filePath = FileManager.CombinePath(ifsExtractPath, pathInIFS);
        
        if (FileManager.IsFileExist(filePath))
		{
		    int num = 0;
			while (true)
			{
				try
				{
				    m_assetBundle = AssetBundle.LoadFromFile(filePath);
				}
				catch (Exception)
				{
					m_assetBundle = null;
				}
				if (this.m_assetBundle != null)
				{
					break;
				}
				num++;
				if (num >= 3)
				{
					break;
				}
			}

            if (m_assetBundle == null) 
            {
				Debug.LogError("Load AssetBundle " + filePath + " Error!!!");
			}
		}
		else
		{
			Debug.LogError("File " + filePath + " can not be found!!!");
		}

        Debug.Log("Load AssetBundle " + filePath + " Success!!!");
        assetBundleState = enAssetBundleState.Loaded;
    }

    public IEnumerator AsyncLoadAssetBundle(string ifsExtractPath)
    {
        //if (isAssetBundle)
        //{
        //    yield break;
        //}

        useAsyncLoadingData = true;

        if (assetBundleState != enAssetBundleState.Loaded)
        {
            assetBundleState = enAssetBundleState.Loading;

            AssetBundleCreateRequest assetBundleLoader = AssetBundle.LoadFromFileAsync(FileManager.CombinePath(ifsExtractPath, pathInIFS));

            yield return assetBundleLoader;

            if (useAsyncLoadingData)
            {
                m_assetBundle = assetBundleLoader.assetBundle;
            }

            assetBundleState = enAssetBundleState.Loaded;
        }

        yield return 0;
    }

    public void UnloadAssetBundle(bool force = false)
    {
        if (isAssetBundle && (!IsResident() || force))
        {
            if (assetBundleState == enAssetBundleState.Loaded)
            {
                if (m_assetBundle != null)
                {
                    m_assetBundle.Unload(false);
                    m_assetBundle = null;
                }
                assetBundleState = enAssetBundleState.Unload;
            }
            else if (assetBundleState == enAssetBundleState.Loading)
            {
                useAsyncLoadingData = false;
            }
            if (dependency != null)
            {
                dependency.UnloadAssetBundle(force);
            }
        }
    }
}
