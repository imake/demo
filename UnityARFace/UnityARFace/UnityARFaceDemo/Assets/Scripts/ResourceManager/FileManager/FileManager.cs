using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public enum enFileOperation
{
    ReadFile,
    WriteFile,
    DeleteFile,
    CreateDirectory,
    DeleteDirectory
}

public class FileManager
{
    public delegate void DelegateOnOperateFileFail(string fullPath, enFileOperation fileOperation);

    private static string s_cachePath = null;

    public static string s_ifsExtractFolder = "Resources";

    private static string s_ifsExtractPath = null;

    private static MD5CryptoServiceProvider s_md5Provider = new MD5CryptoServiceProvider();

    public static FileManager.DelegateOnOperateFileFail s_delegateOnOperateFileFail = delegate
    {

    };

    public static string CombinePaths(params string[] values)
    {
        if (values.Length <= 0)
        {
            return string.Empty;
        }
        if (values.Length == 1)
        {
            return FileManager.CombinePath(values[0], string.Empty);
        }
        if (values.Length > 1)
        {
            string text = FileManager.CombinePath(values[0], values[1]);
            for (int i = 2; i < values.Length; i++)
            {
                text = FileManager.CombinePath(text, values[i]);
            }
            return text;
        }
        return string.Empty;
    }

    /// <summary>
    /// 去除后缀名
    /// </summary>
    /// <param name="fullName"></param>
    /// <returns></returns>
    public static string EraseExtension(string fullName)
    {
        if (fullName == null)
        {
            return null;
        }
        int num = fullName.LastIndexOf('.');
        if (num > 0)
        {
            return fullName.Substring(0, num);
        }
        return fullName;
    }

    public static string GetExtension(string fullName)
    {
        int num = fullName.LastIndexOf('.');
        if (num > 0 && num + 1 < fullName.Length)
        {
            return fullName.Substring(num);
        }
        return string.Empty;
    }

    public static string GetFullName(string fullPath)
    {
        if (fullPath == null)
        {
            return null;
        }
        int num = fullPath.LastIndexOf("/");
        if (num > 0)
        {
            return fullPath.Substring(num + 1, fullPath.Length - num - 1);
        }
        return fullPath;
    }

    /// <summary>
    /// 本地资源存放路径
    /// </summary>
    /// <returns></returns>
    public static string GetIFSExtractPath()
    {
        return GetResourcePath();
    }

    private static string GetLocalPathHeader()
    {
        return "file://";
    }

    public static string GetMd5(byte[] data)
    {
        return BitConverter.ToString(FileManager.s_md5Provider.ComputeHash(data)).Replace("-", string.Empty);
    }

    public static string GetMd5(string str)
    {
        return BitConverter.ToString(FileManager.s_md5Provider.ComputeHash(Encoding.UTF8.GetBytes(str))).Replace("-", string.Empty);
    }

    public static string GetStreamingAssetsPathWithHeader(string fileName)
    {
        return Path.Combine(Application.streamingAssetsPath, fileName);
    }

    #region Path

    public static string GetCachePath() /* Persistent Data Path (可读可写路径) */
    {
        if (s_cachePath == null)
        {
            s_cachePath = Application.persistentDataPath;//沙盒目录
        }

        return s_cachePath;
    }

    public static string GetStreamingAssetsPath() /* Streaming Assets Path (可读路径) */
    {
        return Application.streamingAssetsPath;
    }

    /// <summary>
    /// 从服务器下载资源存放路径
    /// </summary>
    /// <returns></returns>
    public static string GetCacheResourcePath()
    {
        string _serverPath = CombinePath(GetCachePath(), "CacheResource");

        CreateDirectory(_serverPath);

        return _serverPath;
    }

    /// <summary>
    /// 本地资源版本号文件路径
    /// </summary>
    /// <returns></returns>
    public static string GetResourceVersionFilePath()
    {
        string directory = GetResourcePath();

        return directory.CombinePath("ResourceVersion.bytes");
    }

    /// <summary>
    /// 本地资源存放路径
    /// </summary>
    /// <returns></returns>
    public static string GetResourcePath()
    {
        string _serverPath = CombinePath(GetCachePath(), "Assets");

        CreateDirectory(_serverPath);

        return _serverPath;

        //CreateDirectory(GetStreamingAssetsPath());

        //return GetStreamingAssetsPath();
    }

    /// <summary>
    /// 合并路径
    /// </summary>
    /// <param name="path1">路径1</param>
    /// <param name="path2">路径2</param>
    /// <returns></returns>
    public static string CombinePath(string path1, string path2)
    {
        // 最后一个字符不为'/',添加'/'
        if (path1.LastIndexOf('/') != (path1.Length - 1))
        {
            path1 = path1 + '/';
        }

        // 第一个字符为'/',删除'/'
        if (path2.IndexOf('/') == 0)
        {
            path2 = path2.Substring(1);
        }

        return (path1 + path2);
    }

    #endregion

    #region File

    public static FileStream CreateFile(string filePath)
    {
        if (IsFileExist(filePath))
        {
            DeleteFile(filePath);
        }

        if (!IsDirectoryExist(filePath.GetDirectoryPath()))
        {
            CreateDirectory(filePath.GetDirectoryPath());
        }

        return File.Create(filePath);
    }

    public static bool IsFileExist(string filePath)    /* 文件是否存在 */
    {
        //if (!MonoSingleton<GameFramework>.GetInstance().UseStreamingAssets)
        //{
            return File.Exists(filePath);
        //}
        //else
        //{
            //return true;
        //}
    }

    public static byte[] ReadFile(string filePath)
    {
        if (IsFileExist(filePath))
        {
            byte[] _buffer = null;

            int _num = 0;

            do
            {
                try
                {
                    _buffer = File.ReadAllBytes(filePath);
                }
                catch (Exception exception)
                {
#if UNITY_EDITOR
                    Debug.Log(string.Concat(new object[] { "Read File ", filePath, " Error! Exception = ", exception.ToString(), ", TryCount = ", _num }));
#endif
                    _buffer = null;
                }

                if (_buffer != null && _buffer.Length > 0)
                {
                    return _buffer;
                }

                _num++;
            }
            while (_num < 3);

#if UNITY_EDITOR
            Debug.Log(string.Concat(new object[] { "Read File ", filePath, " Fail!, TryCount = ", _num }));
#endif

            // 文件操作失败委托回调
            s_delegateOnOperateFileFail(filePath, enFileOperation.ReadFile);
        }

        return null;
    }

    public static bool WriteFile(string filePath, byte[] data)
    {
        int num = 0;

        while (true)
        {
            try
            {
                File.WriteAllBytes(filePath, data);

                return true;
            }
            catch (Exception exception)
            {
                num++;

                if (num >= 3)
                {
                    Debug.Log("Write File " + filePath + " Error! Exception = " + exception.ToString());

                    DeleteFile(filePath);

                    s_delegateOnOperateFileFail(filePath, enFileOperation.WriteFile);

                    return false;
                }
            }
        }
    }

    public static bool WriteFile(string filePath, byte[] data, int offset, int length)
    {
        FileStream stream = null;

        int num = 0;

        while (true)
        {
            try
            {
                stream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);

                stream.Write(data, offset, length);

                stream.Close();

                return true;
            }
            catch (Exception exception)
            {
                if (stream != null)
                {
                    stream.Close();
                }

                num++;

                if (num >= 3)
                {
                    Debug.Log("Write File " + filePath + " Error! Exception = " + exception.ToString());

                    DeleteFile(filePath);

                    s_delegateOnOperateFileFail(filePath, enFileOperation.WriteFile);

                    return false;
                }
            }
        }
    }

    public static void CopyFile(string srcFile, string dstFile)
    {
        string path = srcFile.Substring(0, srcFile.LastIndexOf('/'));

        if (!IsDirectoryExist(path))
        {
            CreateDirectory(path);
        }

        path = dstFile.Substring(0, dstFile.LastIndexOf('/'));

        if (!IsDirectoryExist(path))
        {
            CreateDirectory(path);
        }

        File.Copy(srcFile, dstFile, true);
    }

    public static void MoveFile(string srcFile, string dstFile, bool deleteFolder = false) /* deleteFolder 当源文件夹下没有文件时，删除文件夹 */
    {
        try
        {
            if (!IsFileExist(srcFile))
            {
                return;
            }

            if (IsFileExist(dstFile))
            {
                DeleteFile(dstFile);
            }

            if (!IsDirectoryExist(dstFile.GetDirectoryPath()))
            {
                CreateDirectory(dstFile.GetDirectoryPath());
            }

            File.Move(srcFile, dstFile);
        }
        catch
        {
            Debug.LogError("MoveFile"); 
        }
    }

    public static bool DeleteFile(string filePath)
    {
        if (!IsFileExist(filePath))
        {
            return true;
        }

        int num = 0;

        while (true)
        {
            try
            {
                File.Delete(filePath);

                return true;
            }
            catch (Exception exception)
            {
                num++;

                if (num >= 3)
                {
                    Debug.Log("Delete File " + filePath + " Error! Exception = " + exception.ToString());

                    s_delegateOnOperateFileFail(filePath, enFileOperation.DeleteFile);

                    return false;
                }
            }
        }
    }

    public static int GetFileLength(string filePath)
    {
        if (!IsFileExist(filePath))
        {
            return 0;
        }

        int num = 0;

        while (true)
        {
            try
            {
                FileInfo info = new FileInfo(filePath);

                return (int)info.Length;
            }
            catch (Exception exception)
            {
                num++;

                if (num >= 3)
                {
                    Debug.Log("Get FileLength of " + filePath + " Error! Exception = " + exception.ToString());

                    return 0;
                }
            }
        }
    }

    public static string GetFileMd5(string filePath)
    {
        if (!IsFileExist(filePath))
        {
            return string.Empty;
        }

        return BitConverter.ToString(s_md5Provider.ComputeHash(ReadFile(filePath))).Replace("-", string.Empty);
    }

    public static int GetFileSize(string filePath)
    {
        int _value = 0;

        if (IsFileExist(filePath))
        {
            FileStream _fs = new FileStream(filePath, FileMode.Open);

            if (_fs != null)
            {
                _value = (int)(_fs.Length);

                _fs.Close();
            }
        }

        return _value;
    }

    /// <summary>
    /// 获取文件的名字
    /// </summary>
    /// <param name="filePath">文件完全路径</param>
    /// <returns></returns>
    public static string GetNameFile(string filePath)
    {
        filePath = EraseExtension(filePath);

        int index = filePath.LastIndexOf('/');

        return index < 0 ? filePath : filePath.Substring(index + 1);
    }

    #endregion

    #region Directory

    public static bool CreateDirectory(string directory)
    {
        if (IsDirectoryExist(directory))
        {
            return true;
        }

        int num = 0;

        while (true)
        {
            try
            {
                Directory.CreateDirectory(directory);

                return true;
            }
            catch (Exception exception)
            {
                num++;

                if (num >= 3)
                {
                    Debug.Log("Create Directory " + directory + " Error! Exception = " + exception.ToString());

                    s_delegateOnOperateFileFail(directory, enFileOperation.CreateDirectory);

                    return false;
                }
            }
        }
    }

    public static bool ClearDirectory(string fullPath)
    {
        try
        {
            string[] files = Directory.GetFiles(fullPath);
            for (int i = 0; i < files.Length; i++)
            {
                File.Delete(files[i]);
            }
            string[] directories = Directory.GetDirectories(fullPath);
            for (int j = 0; j < directories.Length; j++)
            {
                Directory.Delete(directories[j], true);
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool ClearDirectory(string fullPath, string[] fileExtensionFilter, string[] folderFilter)
    {
        try
        {
            if (fileExtensionFilter != null)
            {
                string[] _files = Directory.GetFiles(fullPath);

                for (int i = 0; i < _files.Length; i++)
                {
                    if ((fileExtensionFilter != null) && (fileExtensionFilter.Length > 0))
                    {
                        for (int j = 0; j < fileExtensionFilter.Length; j++)
                        {
                            if (_files[i].Contains(fileExtensionFilter[j]))
                            {
                                DeleteFile(_files[i]);

                                break;
                            }
                        }
                    }
                }
            }

            if (folderFilter != null)
            {
                string[] _directories = Directory.GetDirectories(fullPath);

                for (int k = 0; k < _directories.Length; k++)
                {
                    if ((folderFilter != null) && (folderFilter.Length > 0))
                    {
                        for (int m = 0; m < folderFilter.Length; m++)
                        {
                            if (_directories[k].Contains(folderFilter[m]))
                            {
                                DeleteDirectory(_directories[k]);

                                break;
                            }
                        }
                    }
                }
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public static bool DeleteDirectory(string directory)
    {
        if (!IsDirectoryExist(directory))
        {
            return true;
        }

        int _num = 0;

        while (true)
        {
            try
            {
                Directory.Delete(directory, true);

                return true;
            }
            catch (Exception exception)
            {
                _num++;

                if (_num >= 3)
                {
                    Debug.Log("Delete Directory " + directory + " Error! Exception = " + exception.ToString());

                    s_delegateOnOperateFileFail(directory, enFileOperation.DeleteDirectory);

                    return false;
                }
            }
        }
    }

    public static bool IsDirectoryExist(string directory)
    {
        return Directory.Exists(directory);
    }

    public static string GetFullDirectory(string fullPath)
    {
        return Path.GetDirectoryName(fullPath);
    }

    public static string[] GetTotalFileFromDirectory(string folderPath)
    {
        List<string> _fileList = new List<string>();

        string[] _files = Directory.GetFiles(folderPath);

        for (int i = 0; i < _files.Length; i++)
        {
            _fileList.Add(_files[i]);
        }

        string[] _folders = Directory.GetDirectories(folderPath);

        for (int i = 0; i < _folders.Length; i++)
        {
            _files = GetTotalFileFromDirectory(_folders[i]);

            for (int j = 0; j < _files.Length; j++)
            {
                _fileList.Add(_files[j]);
            }
        }

        return _fileList.ToArray();
    }

    public static void CopyDirectory(string srcPath, string destPath)
    {
        try
        {
            DirectoryInfo dir = new DirectoryInfo(srcPath);

            FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //获取目录下（不包含子目录）的文件和子目录

            foreach (FileSystemInfo i in fileinfo)
            {
                if (i is DirectoryInfo)     //判断是否文件夹
                {
                    if (!Directory.Exists(destPath + "\\" + i.Name))
                    {
                        Directory.CreateDirectory(destPath + "\\" + i.Name);   //目标目录下不存在此文件夹即创建子文件夹
                    }

                    CopyDirectory(i.FullName, destPath + "\\" + i.Name);    //递归调用复制子文件夹
                }
                else
                {
                    File.Copy(i.FullName, destPath + "\\" + i.Name, true);      //不是文件夹即复制文件，true表示可以覆盖同名文件
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    public static void MoveDirectory(string srcPath, string destPath)
    {
        DirectoryInfo dir = new DirectoryInfo(srcPath);

        FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //获取目录下（不包含子目录）的文件和子目录

        foreach (FileSystemInfo i in fileinfo)
        {
            if (i is DirectoryInfo)     //判断是否文件夹
            {
                try
                {
                    CreateDirectory(destPath.CombinePath(i.Name));

                    Directory.Move(i.FullName, destPath.CombinePath(i.Name));
                }
                catch
                {
                    Debug.LogError(destPath.CombinePath(i.Name));
                }
            }
            else
            {
                File.Move(i.FullName, destPath + "\\" + i.Name);      //不是文件夹即复制文件，true表示可以覆盖同名文件
            }
        }
    }

    #endregion

    #region Zip

    /// <summary>   
    /// 递归压缩文件夹的内部方法   
    /// </summary>   
    /// <param name="folderRootZip">要压缩的文件夹路径</param>   
    /// <param name="streamZip">压缩输出流</param>   
    /// <param name="folderParent">此文件夹的上级文件夹</param>   
    /// <returns>是否压缩成功</returns>   
    private static Dictionary<string, int> ZipDirectory(string folderRootZip, ZipOutputStream streamZip, string folderParent, string[] maskFileTypes = null)
    {
        folderRootZip = folderRootZip.Replace('\\', '/');

        Dictionary<string, int> result = new Dictionary<string, int>();

        string[] folders, files;

        ZipEntry entry = null;

        FileStream fs = null;

        Crc32 crc = new Crc32();

        string _directory;

        _directory = folderRootZip.Substring(folderParent.Length);

        try
        {
            //ent = new ZipEntry(_directory + "/");

            //streamZip.PutNextEntry(ent);

            //streamZip.Flush();

            files = Directory.GetFiles(folderRootZip);

            int _count = 0;

            foreach (string file in files)
            {
                if (maskFileTypes != null)
                {
                    string fileType = file.Substring(file.LastIndexOf('.'));

                    bool isContinue = false;

                    for (int i = 0; i < maskFileTypes.Length; i++)
                    {
                        if (string.Equals(fileType, maskFileTypes[i]))
                        {
                            isContinue = true;

                            break;
                        }
                    }

                    if (isContinue) { continue; }
                }

                fs = File.OpenRead(file);

                byte[] buffer = new byte[fs.Length];

                fs.Read(buffer, 0, buffer.Length);

                entry = new ZipEntry(_directory + "/" + Path.GetFileName(file))
                {
                    DateTime = DateTime.Now,

                    Size = fs.Length
                };

                fs.Close();

                crc.Reset();

                crc.Update(buffer);

                entry.Crc = crc.Value;

                streamZip.PutNextEntry(entry);

                streamZip.Flush();

                Debug.LogError(streamZip.Position);

                long start = streamZip.Position;

                streamZip.Write(buffer, 0, buffer.Length);

                Debug.LogError(streamZip.Position);

                result[file] = (int)(buffer.Length);
                //buffer.Length;

                _count += buffer.Length;

                Debug.Log(_count);
            }

        }
        catch
        {
            //Debug.LogError("1111111111111111111        " + folderToZip);
        }
        finally
        {
            if (fs != null)
            {
                fs.Close();

                fs.Dispose();
            }
            if (entry != null)
            {
                entry = null;
            }

            GC.Collect();

            GC.Collect(1);
        }

        folders = Directory.GetDirectories(folderRootZip);

        foreach (string folder in folders)
        {
            Dictionary<string, int> cacheMap = ZipDirectory(folder, streamZip, folderParent, maskFileTypes);

            foreach (KeyValuePair<string, int> node in cacheMap)
            {
                result[node.Key] = node.Value;
            }
        }

        return result;
    }

    /// <summary>   
    /// 压缩文件夹    
    /// </summary>   
    /// <param name="foldeRootZip">要压缩的文件夹路径</param>   
    /// <param name="fileTargetZip">压缩目标文件完整路径</param>   
    /// <param name="password">密码</param>   
    /// <returns>是否压缩成功</returns>   
    public static Dictionary<string, int> ZipDirectory(string foldeRootZip, string fileTargetZip, string password, string[] maskFileTypes = null)
    {
        if (!Directory.Exists(foldeRootZip))
        {
            return null;
        }

        ZipOutputStream zipStream = new ZipOutputStream(File.Create(fileTargetZip));

        zipStream.SetLevel(6);

        if (!string.IsNullOrEmpty(password))
        {
            zipStream.Password = password;
        }

        Dictionary<string, int> result = ZipDirectory(foldeRootZip, zipStream, foldeRootZip, maskFileTypes);

        zipStream.Finish();

        zipStream.Close();

        return result;
    }

    /// <summary>   
    /// 压缩文件夹   
    /// </summary>   
    /// <param name="folderRootZip">要压缩的文件夹路径</param>   
    /// <param name="fileTargetZip">压缩文件完整路径</param>   
    /// <returns>是否压缩成功</returns>   
    public static Dictionary<string, int> ZipDirectory(string folderRootZip, string fileTargetZip, string[] maskFileTypes = null)
    {
        return ZipDirectory(folderRootZip, fileTargetZip, null, maskFileTypes);
    }

    /// <summary>   
    /// 解压功能(解压压缩文件到指定目录)   
    /// </summary>   
    /// <param name="fileToUnZip">待解压的文件</param>   
    /// <param name="zipedFolder">指定解压目标目录</param>   
    /// <param name="password">密码</param>   
    /// <returns>解压结果</returns>   
    public static bool UnZip(string fileToUnZip, string zipedFolder, string password)
    {
        bool result = true;

        FileStream fs = null;

        ZipInputStream zipStream = null;

        ZipEntry ent = null;

        string fileName;

        if (!File.Exists(fileToUnZip))
        {
            return false;
        }

        if (!Directory.Exists(zipedFolder))
        {
            Directory.CreateDirectory(zipedFolder);
        }

        try
        {
            zipStream = new ZipInputStream(File.OpenRead(fileToUnZip));

            if (!string.IsNullOrEmpty(password)) zipStream.Password = password;

            while ((ent = zipStream.GetNextEntry()) != null)
            {
                if (!string.IsNullOrEmpty(ent.Name))
                {
                    fileName = CombinePath(zipedFolder, ent.Name);

                    fileName = fileName.Replace('/', '\\');//change by Mr.HopeGi   

                    if (fileName.EndsWith("\\"))
                    {
                        Directory.CreateDirectory(fileName);

                        continue;
                    }

                    fs = CreateFile(fileName);

                    int size = 2048;

                    byte[] data = new byte[size];

                    int count = 0;

                    while (true)
                    {
                        size = zipStream.Read(data, 0, data.Length);

                        if (size > 0)
                        {
                            fs.Write(data, 0, size);

                            count += size;
                        }
                        else if (size == 0)
                        {
                            break;
                        }
                    }

                    fs.Close();
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);

            result = false;

            Debug.LogError(fileToUnZip);

            if (fs != null) { fs.Close(); }

            if (zipStream != null) { zipStream.Close(); }
        }

        if (zipStream != null) { zipStream.Close(); }

        return result;
    }

    /// <summary>   
    /// 解压功能(解压压缩文件到指定目录)   
    /// </summary>   
    /// <param name="fileToUnZip">待解压的文件</param>   
    /// <param name="zipedFolder">指定解压目标目录</param>   
    /// <returns>解压结果</returns>   
    public static bool UnZip(string fileToUnZip, string zipedFolder)
    {
        bool result = UnZip(fileToUnZip, zipedFolder, null);

        return result;
    }

    /// <summary>
    /// 获取压缩包文件大小
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    public static int GetDecompressedFileSize(string filePath)
    {
        int num = 0;

        if (!string.IsNullOrEmpty(filePath))
        {
            ZipEntry entry;

            ZipInputStream stream = new ZipInputStream(File.OpenRead(filePath));

            if (stream == null)
            {
                return 0;
            }

            while ((entry = stream.GetNextEntry()) != null)
            {
                if (!string.IsNullOrEmpty(Path.GetFileName(entry.Name)))
                {
                    num += (int)(entry.Size);
                }
            }

            stream.Close();

        }
        return num;
    }

    #endregion
}
