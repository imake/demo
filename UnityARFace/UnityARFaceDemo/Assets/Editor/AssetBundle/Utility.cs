using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Assets.Editor.AssetBundleBuilder
{
    public class Utility
    {
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

        public static string GetRelativeAssetsPath(string path)
        {
            return "Assets" + Path.GetFullPath(path).Replace(Path.GetFullPath(Application.dataPath), "").Replace('\\', '/');
        }

        public static string GetBundleNames(string path)
        {
            return Path.GetFileNameWithoutExtension(path) + BundleConfig.FILE_EXTENSION;
        }

        public static string GetFileNameWithOutExtension(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        public static string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }

        public static string GetFullPath(string path)
        {
            return Path.GetFullPath(path);
        }
    }
}