using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 资源基本数据结构,里面包含资源的内容、名字、
/// 路径、类型信息;并且提供加载资源方法
/// </summary>
public class ResourceBase 
{
    /// <summary>
    /// 键索引
    /// </summary>
    private int m_key;

    /// <summary>
    /// asset索引值
    /// </summary>
    private int m_bundleKey;

    /// <summary>
    /// 资源名字
    /// </summary>
    private string m_name;

    /// <summary>
    /// 资源在Resource文件夹下完整路径
    /// </summary>
    private string m_fullPathInResources;

    /// <summary>
    /// 资源在Resource文件夹下完整路径,不包含资源后缀名
    /// </summary>
    private string m_fullPathInResourcesWithoutExtension;

    /// <summary>
    /// 文件在Resource文件夹下完整路径
    /// </summary>
    private string m_fileFullPathInResources;

    /// <summary>
    /// 数据类型
    /// </summary>
    private Type m_contentType;

    /// <summary>
    /// 资源类型
    /// </summary>
    private enResourceType m_resourceType;

    /// <summary>
    /// 加载状态
    /// </summary>
    private enResourceState m_state;

    /// <summary>
    /// 加载资源后是否卸载资源所在的AB包
    /// </summary>
    private bool m_unloadBelongedAssetBundleAfterLoaded;

    /// <summary>
    /// 资源对象
    /// </summary>
    private UnityEngine.Object m_content;

    /// <summary>
    /// 是否弃用此资源
    /// </summary>
    private bool m_isAbandon;

    public int key
    {
        get
        {
            return m_key;
        }
    }

    public int bundleKey
    {
        get
        {
            return m_bundleKey;
        }
    }

    public enResourceType resourceType
    {
        get
        {
            return m_resourceType;
        }
        set
        {
            m_resourceType = value;
        }
    }

    public bool unloadBelongedAssetBundleAfterLoaded
    {
        get
        {
            return m_unloadBelongedAssetBundleAfterLoaded;
        }
    }

    public UnityEngine.Object content
    {
        get
        {
            return m_content;
        }
    }

    public string Name
    {
        get
        {
            return m_name;
        }
    }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="keyHash">键索引</param>
    /// <param name="fullPathInResources">资源在Resource文件夹下完整路径</param>
    /// <param name="contentType">数据类型</param>
    /// <param name="resourceType">资源类型</param>
    /// <param name="unloadBelongedAssetBundleAfterLoaded"></param>
    public ResourceBase(int keyHash, int keyBundle, string fullPathInResources, Type contentType, enResourceType resourceType, bool unloadBelongedAssetBundleAfterLoaded)
    {
        this.m_key = keyHash;
        this.m_bundleKey = keyBundle;
        this.m_fullPathInResources = fullPathInResources;
        this.m_fullPathInResourcesWithoutExtension = FileManager.EraseExtension(this.m_fullPathInResources);
        this.m_name = FileManager.EraseExtension(FileManager.GetFullName(fullPathInResources));
        this.m_resourceType = resourceType;
        this.m_state = enResourceState.Unload;
        this.m_contentType = contentType;
        this.m_unloadBelongedAssetBundleAfterLoaded = unloadBelongedAssetBundleAfterLoaded;
        this.m_content = null;
        this.m_isAbandon = false;
    }

    /// <summary>
    /// 用Unity的API加载Resouces中的资源
    /// </summary>
    public void Load()
    {
        if (m_isAbandon)
        {
            m_state = enResourceState.Unload;
            return;
        }
        if (m_contentType == null)
        {
            m_content = Resources.Load(m_fullPathInResourcesWithoutExtension);
        }
        else
        {
            m_content = Resources.Load(m_fullPathInResourcesWithoutExtension, m_contentType);
        }
        m_state = enResourceState.Loaded;
        if (m_content != null && m_content.GetType() == typeof(TextAsset))
        {
            BinaryObject binaryObject = ScriptableObject.CreateInstance<BinaryObject>();
            binaryObject.data = (m_content as TextAsset).bytes;
            m_content = binaryObject;
        }
    }

    /// <summary>
    /// 用C#IO流读取文件资源
    /// </summary>
    /// <param name="ifsExtractPath">文件路径,ifsExtractPath + m_fileFullPathInResources等于文件完整路径</param>
    public void Load(string ifsExtractPath)
    {
        if (m_isAbandon)
        {
            m_state = enResourceState.Unload;
            return;
        }
        byte[] array = FileManager.ReadFile(FileManager.CombinePath(ifsExtractPath, m_fileFullPathInResources));
        m_state = enResourceState.Loaded;
        if (array != null)
        {
            BinaryObject binaryObject = ScriptableObject.CreateInstance<BinaryObject>();
            binaryObject.data = array;
            binaryObject.name = m_name;
            m_content = binaryObject;
        }
    }

    /// <summary>
    /// 从AssetBundle中加载资源
    /// </summary>
    /// <param name="resourcePackerInfo">AB资源包信息</param>
    public void LoadFromAssetBundle(ResourcePackerInfo resourcePackerInfo)
    {
        if (m_isAbandon)
        {
            m_state = enResourceState.Unload;
            return;
        }
        string name = FileManager.EraseExtension(m_name);

        if (m_contentType == null)
        {
            m_content = resourcePackerInfo.assetBundle.LoadAsset(name);    
        }
        else
        {
            try
            {
                AssetBundle dfsd = resourcePackerInfo.assetBundle;
                string[] sdf =  dfsd.GetAllAssetNames();
                m_content = resourcePackerInfo.assetBundle.LoadAsset(name, m_contentType);
            }
            catch
            {
                Debug.Log(name);
            }
        }

        m_state = enResourceState.Loaded;

        if (m_content != null && m_content.GetType() == typeof(TextAsset))
        {
            BinaryObject binaryObject = ScriptableObject.CreateInstance<BinaryObject>();
            binaryObject.data = (m_content as TextAsset).bytes;
            m_content = binaryObject;
        }
    }

    /// <summary>
    /// 卸载资源
    /// </summary>
    public void Unload()
    {
        if (m_state == enResourceState.Loaded)
        {
            m_content = null;
            m_state = enResourceState.Unload;
        }
        else if (m_state == enResourceState.Loading)
        {
            m_isAbandon = true;
        }
    }
}
