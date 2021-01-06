using System.Collections.Generic;

public class AssetBundleNode
{
    /// <summary>
    /// asset名字
    /// </summary>
    public string AssetName { get; set; }

    /// <summary>
    /// asset路径
    /// </summary>
    public string AssetPath { get; set; }

    /// <summary>
    /// assetMD5值
    /// </summary>
    public string AssetMD5 { get; set; }

    /// <summary>
    /// asset大小
    /// </summary>
    public int AssetSize { get; set; }

    /// <summary>
    /// 是否默认缓存
    /// </summary>
    public bool IsDefaultLoad { get; set; }

    /// <summary>
    /// resource路径
    /// </summary>
    public string ResourcePath { get; set; }

    /// <summary>
    /// 包含的资源路径
    /// </summary>
    public List<string> resourcePaths = new List<string>();

    /// <summary>
    /// 依赖的其他asset路径
    /// </summary>
    public List<string> depence = new List<string>();

    /// <summary>
    /// 依赖的上层其他asset路径
    /// </summary>
    public List<string> parents = new List<string>();

    /// <summary>
    /// asset类型,决定资源下载时间
    /// </summary>
    public enAssetType assetType = enAssetType.DEFAULT;

    public AssetBundleNode Clone()
    {
        AssetBundleNode assetBundleNode = new AssetBundleNode
        {
            AssetMD5 = this.AssetMD5,

            AssetName = this.AssetName,

            AssetPath = this.AssetPath,

            AssetSize = this.AssetSize,

            assetType = this.assetType,

            ResourcePath = this.ResourcePath,

            resourcePaths = new List<string>()
        };

        for (int i = 0; i < this.resourcePaths.Count; i++)
        {
            assetBundleNode.resourcePaths.Add(this.resourcePaths[i]);
        }

        assetBundleNode.parents = new List<string>();

        for (int i = 0; i < this.parents.Count; i++)
        {
            assetBundleNode.parents.Add(this.parents[i]);
        }

        return assetBundleNode;
    }
}

public enum enAssetType
{
    /// <summary>
    /// 默认类型,游戏开始时加载
    /// </summary>
    DEFAULT = 0,

    /// <summary>
    /// 小红帽
    /// </summary>
    REDHAT,

    /// <summary>
    /// 居家安全
    /// </summary>
    HOMESAFE,

    /// <summary>
    /// 龟兔赛跑
    /// </summary>
    HARETORTOISE,

    /// <summary>
    /// 白雪公主
    /// </summary>
    STOYA,
}