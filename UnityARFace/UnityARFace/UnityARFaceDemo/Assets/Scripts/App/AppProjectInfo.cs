
/// <summary>
/// 应用信息
/// </summary>
public static class AppProjectInfo
{
    /// <summary>
    /// 应用代号
    /// </summary>
    public static string AppName;

    /// <summary>
    /// 包名
    /// </summary>
    public static string PackageName;

    /// <summary>
    /// 密钥Key
    /// </summary>
    public static string AESKey;

    /// <summary>
    /// 密钥IVector
    /// </summary>
    public static string AESIVector;

    /// <summary>
    /// 服务器标签
    /// </summary>
    public static string ServerTag;

    /// <summary>
    /// 正服连接 (结尾要有/, 否则服务器报错)
    /// </summary>
    public static string WebSocketUrl;

    /// <summary>
    /// 测服连接 (结尾要有/, 否则服务器报错)
    /// </summary>
    public static string WebSocketTestUrl;

    /// <summary>
    /// 域名
    /// </summary>
    public static string Domain;

    /// <summary>
    /// SDK接口前缀
    /// 根据产品发布的账号来填写: solitaire / slidey
    /// </summary>
    public static string SDKApiPrefix;

    /// <summary>
    /// BuglyAndroidAppID
    /// </summary>
    public static string BuglyAppIDForAndroid;

    /// <summary>
    /// BuglyAndroidAppID
    /// </summary>
    public static string BuglyAppIDForiOS;
}
