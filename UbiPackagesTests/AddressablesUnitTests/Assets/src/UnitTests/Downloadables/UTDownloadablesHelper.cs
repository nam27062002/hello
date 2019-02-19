using System.Collections.Generic;
using System.IO;

public class UTDownloadablesHelper
{
    public static string ROOT_PATH = "Assets/Editor/Downloadables/UnitTests";
    public static string ROOT_CATALOGS_PATH = ROOT_PATH + "/" + "Catalogs";
    public static string ROOT_CACHES_PATH = ROOT_PATH + "/" + "Caches";

    public static void CreateDirectory(string path)
    {
        if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public static void CopyDirectory(string srcPath, string dstPath)
    {
        if (Directory.Exists(srcPath))
        {
            if (!Directory.Exists(dstPath))
            {
                CreateDirectory(dstPath);
            }

            string[] files = Directory.GetFiles(srcPath);

            string fileName;
            string dstFile;

            // Copies the files and overwrite destination files if they already exist.           
            foreach (string s in files)
            {
                // Uses static Path methods to extract only the file name from the path.
                fileName = Path.GetFileName(s);
                dstFile = dstPath + "/" + fileName;

                if (Path.GetExtension(fileName) != ".meta")
                {
                    try
                    {
                        File.Copy(s, dstFile, true);
                    }
                    catch (System.Exception e)
                    {
                        UnityEngine.Debug.LogError(e.Message);
                    }
                }
            }

            // Copies the subdirectories            
            string dstSubdirectoryPath;
            string[] subdirectories = Directory.GetDirectories(srcPath);
            foreach (string s in subdirectories)
            {
                // Uses static Path methods to extract only the file name from the path.
                fileName = Path.GetFileName(s);
                dstSubdirectoryPath = dstPath + "/" + fileName;
                CopyDirectory(s, dstSubdirectoryPath);
            }
        }
    }
    
    public static void PrepareCache(string cacheFolder)
    {
        string downloadablesFolder = Downloadables.Manager.DOWNLOADABLES_FOLDER_NAME;
        string srcCachePath = Directory.GetCurrentDirectory() + "/" + ROOT_CACHES_PATH + "/" + cacheFolder;
        string dstCachePath = Directory.GetCurrentDirectory() + "/" + Downloadables.Manager.DESKTOP_DEVICE_STORAGE_PATH_SIMULATED;
        FileUtils.RemoveDirectoryInDeviceStorage(downloadablesFolder, Downloadables.Manager.DESKTOP_DEVICE_STORAGE_PATH_SIMULATED);
        CopyDirectory(srcCachePath, dstCachePath);
    }
    
    public static string GetDeviceCacheRootPath()
    {
        return Directory.GetCurrentDirectory() + "/" + Downloadables.Manager.DESKTOP_DEVICE_STORAGE_PATH_SIMULATED + 
            "/" + Downloadables.Manager.DOWNLOADABLES_FOLDER_NAME;
    }

    public static bool CheckDisk(string path, List<string> ids)
    {
        int idsCount = (ids == null) ? 0 : ids.Count;
        int count = 0;
        string[] files = null;
        if (Directory.Exists(path))
        {
            files = Directory.GetFiles(path);
            count = files.Length;
        }

        bool returnValue = count == idsCount;

        string fileName;
        for (int i = 0; i < count; i++)
        {
            fileName = System.IO.Path.GetFileNameWithoutExtension(files[i]);
            if (!ids.Contains(fileName))
            {
                returnValue = false;
                break;
            }
        }

        return returnValue;
    }

    public static string GetEntryStatusManifestAsString(long crc, long size, int numDownloads, bool verified)
    {
        return @"{""crc32"":" + crc + @",""size"":" + size + @",""t"":" + numDownloads + @",""v"":" + 
                ((verified) ? 1 : 0) + "}";
    }

    public static SimpleJSON.JSONNode GetEntryStatusManifestAsJSON(long crc, long size, int numDownloads, bool verified)
    {
        string asString = GetEntryStatusManifestAsString(crc, size, numDownloads, verified);
        return SimpleJSON.JSON.Parse(asString);        
    }

    public static string GetManifestFile(string id)
    {
        string returnValue = null;
        string manifestPath = Path.Combine(Downloadables.Manager.MANIFESTS_ROOT_PATH, id);
        if (File.Exists(manifestPath))
        {
            returnValue = File.ReadAllText(manifestPath);
        }

        return returnValue;
    }
}
