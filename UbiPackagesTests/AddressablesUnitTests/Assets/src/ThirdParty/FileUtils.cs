using UnityEngine;
using System;
using System.IO;

public class FileUtils
{
    public static bool TryRemoveDirectoryInDeviceStorage(string strDirectoryPath, string strForceProjectFolder = "")
    {
        try
        {
            RemoveDirectoryInDeviceStorage(strDirectoryPath, strForceProjectFolder);
            return true;
        }
        catch (IOException)
        {
            //Debug.TaggedLog("TryDelete", e.StackTrace);
            return false;
        }
    }

    public static void RemoveDirectoryInDeviceStorage(string strDirectoryPath, string strForceProjectFolder = "")
    {
        string strFinalPath = "";
        string strPrepath = "";

#if UNITY_EDITOR
        if (strForceProjectFolder.Length > 0)
        {
            strPrepath = Directory.GetCurrentDirectory() + "/" + strForceProjectFolder;

            strFinalPath = strPrepath + strDirectoryPath;
        }
        else
        {
            strPrepath = Application.persistentDataPath + "/";

            strFinalPath = strPrepath + strDirectoryPath;
        }
#else
        strPrepath = Application.persistentDataPath + "/";
        strFinalPath = strPrepath + strDirectoryPath;
#endif

        if (Directory.Exists(strFinalPath))
        {
            //CaletyUtils.Log.d("removing " + strDirectoryPath + "   with   " + strFinalPath);

            string[] kFiles = Directory.GetFiles(strFinalPath);
            string[] kDirs = Directory.GetDirectories(strFinalPath);

            foreach (string strFile in kFiles)
            {
                try
                {
                    System.IO.File.SetAttributes(strFile, FileAttributes.Normal);
                }
                catch (UnauthorizedAccessException)
                {
                    //CaletyUtils.Log.d("RemoveDirectoryInDeviceStorage: " + e.ToString());
                }

                File.Delete(strFile);
            }

            foreach (string strDir in kDirs)
            {
                string strDirTemp = strDir;
                strDirTemp = strDirTemp.Replace(strPrepath, "");

                RemoveDirectoryInDeviceStorage(strDirTemp, strForceProjectFolder);
            }

            System.IO.Directory.Delete(strFinalPath);
        }
    }

    public static string GetDeviceStoragePath (string strDirectoryPath, string strForceProjectFolder = "")
    {
        string strFinalPath = "";

#if UNITY_EDITOR
        if (strForceProjectFolder.Length > 0)
        {
            strFinalPath = Directory.GetCurrentDirectory () + "/" + strForceProjectFolder + strDirectoryPath;
        }
        else
        {
            strFinalPath = Application.persistentDataPath + "/" + strDirectoryPath;
        }
#else
        strFinalPath = Application.persistentDataPath + "/" + strDirectoryPath;
#endif

        return strFinalPath;
    }   
}
