using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class OptimizationDataEditor:Singleton<OptimizationDataEditor>
{
    public  string GetSpawnerLevelFileURLFromName(string name)
    {
		return  "Assets/" + OptimizationDataInfo.SpawnersScenePath + name + ".unity";
    }

    public  string GetLevelFileURLFromName(string name)
    {
		return  "Assets/" + OptimizationDataInfo.ArtScenePath + name + ".unity";
    }

    public  string GetPrefabFile(string prefabFile, OptimizationDataInfo.QualityLevelsType type, bool createFolder=false)
    {
        string suffix = OptimizationDataInfo.instance.GetSufix(type);
        string prefabFileExtension = System.IO.Path.GetExtension(prefabFile);
        string prefabFileName = System.IO.Path.GetFileNameWithoutExtension(prefabFile) + prefabFileExtension;
        string prefabDirectory = System.IO.Path.GetDirectoryName(prefabFile);

        int index = prefabDirectory.IndexOf(OptimizationDataInfo.PrefabFolderName);
        if (index == -1)
        {
            Debug.LogError("Error");
            return null;
        }
        index += OptimizationDataInfo.PrefabFolderName.Length;
        prefabDirectory = prefabDirectory.Insert(index, OptimizationDataInfo.DirectorySeparator + OptimizationDataInfo.MultiSufixFolferName);
        prefabDirectory += OptimizationDataInfo.DirectorySeparator + suffix;
        if (createFolder)
        {
            System.IO.DirectoryInfo resource = new System.IO.DirectoryInfo(prefabDirectory);
            if (!resource.Exists)
            {
                resource.Create();
            }
        }
        string prefabDestinationPath = prefabDirectory + "/" + prefabFileName;

        return prefabDestinationPath;
    }

    public  void DeleteADir(string folder)
    {
        System.IO.DirectoryInfo resource = new System.IO.DirectoryInfo(folder);
        DeleteADir(resource);
        UnityEditor.AssetDatabase.Refresh();
    }

    public  void DeleteAndCreateADir(string folder)
    {
        System.IO.DirectoryInfo resource = new System.IO.DirectoryInfo(folder);
        DeleteAndCreateADir(resource);
        UnityEditor.AssetDatabase.Refresh();
    }

    public  void CreateADir(string folder)
    {
        System.IO.DirectoryInfo resource = new System.IO.DirectoryInfo(folder);
        CreateADir(resource);
        AssetDatabase.Refresh();
    }

    public void CreateADir(System.IO.DirectoryInfo directory)
    {
        if (!directory.Exists)
        {
            directory.Create();
        }
    }

    public void DeleteADir(System.IO.DirectoryInfo directory)
    {
        if (directory.Exists)
        {
            //bool result=UnityEditor.FileUtil.DeleteFileOrDirectory(directory.FullName);
            UnityEngine.Debug.Log("DeleteADir " + directory.FullName);
            Empty(directory, true);
            directory.Delete(true);
            string metaFile = directory.FullName + OptimizationDataInfo.MetaFileExtension;
            UnityEditor.FileUtil.DeleteFileOrDirectory(metaFile);
        }
    }

    public void DeleteAndCreateADir(System.IO.DirectoryInfo directory)
    {
        if (directory.Exists)
        {
            Empty(directory, true);
            directory.Delete(true);
        }
        directory.Create();
    }

    public void EmptyOrCreateADir(string folder)
    {
        System.IO.DirectoryInfo resource = new System.IO.DirectoryInfo(folder);
        EmptyOrCreateADir(resource);
        AssetDatabase.Refresh();
    }

    public void EmptyOrCreateADir(System.IO.DirectoryInfo directory, bool subfolder=false)
    {
        if (directory.Exists)
        {
            UnityEmpty(directory, subfolder);
        } else
        {
            UnityCreateFolder(directory);
        }
    }

    public void UnityEmpty(System.IO.DirectoryInfo directory, bool subfolder=false)
    {
        System.IO.FileInfo[] fileInfos = directory.GetFiles();

        foreach (System.IO.FileInfo fileInfo in fileInfos)
        {
            if (fileInfo.Extension != OptimizationDataInfo.MetaFileExtension)
            {
                UnityEngine.Debug.Log("Delete file " + fileInfo.FullName);
                string projectRelativePath = GetProjectRelativePath(fileInfo.FullName);
                bool deleted = UnityEditor.AssetDatabase.DeleteAsset(projectRelativePath);
                UnityEngine.Debug.Log("Deleted asset " + projectRelativePath + "  result " + deleted);
                if (!deleted)
                {
                    UnityEngine.Debug.Log("Failed to delete file Attributes [" + fileInfo.Attributes + "] IsReadOnly " + fileInfo.IsReadOnly);
                }
            }
        }
        if (subfolder)
        {
            System.IO.DirectoryInfo[] directories = directory.GetDirectories();
            foreach (System.IO.DirectoryInfo directoryInfo in directories)
            {
                UnityEmpty(directoryInfo, subfolder);
            }
        }
    }

    public  String[] SplitPath(string path)
    {
        String[] pathSeparators = new String[] { "\\" };
        return path.Split(pathSeparators, StringSplitOptions.RemoveEmptyEntries);
    }

    public string GetProjectRelativePath(string fileSystemPath)
    {
        string pathRelativeToAssetFolder = null;
        int index = fileSystemPath.IndexOf(OptimizationDataInfo.AssetFolderName);
        if (index != -1)
            pathRelativeToAssetFolder = fileSystemPath.Substring(index);

        return pathRelativeToAssetFolder;
    }

    public void UnityCreateFolder(System.IO.DirectoryInfo directory)
    {
        string path = directory.FullName;
        int index = path.IndexOf(OptimizationDataInfo.AssetFolderName);
        string pathRelativeToAssetFolder = path.Substring(index + OptimizationDataInfo.AssetFolderName.Length);
        string indexPath = path.Substring(0, index - 1);
        string[] pathArray = SplitPath(pathRelativeToAssetFolder);
        string currentFolder = OptimizationDataInfo.AssetFolderPath;
        for (int i=0; i<pathArray.Length; i++)
        {
            string currentFullPath = indexPath + OptimizationDataInfo.DirectorySeparator + currentFolder + OptimizationDataInfo.DirectorySeparator + pathArray [i];
            if (!System.IO.Directory.Exists(currentFullPath))
            {
                string guid = UnityEditor.AssetDatabase.CreateFolder(currentFolder, pathArray [i]);
                string newFolderPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
            }
            currentFolder += OptimizationDataInfo.DirectorySeparator + pathArray [i];
        }

        if (!directory.Exists)
            directory.Create();
    }

    public  List<String> GetSceneMaterialFolders()
    {
        List<string> folders = new List<string>();
        folders.Add(OptimizationDataInfo.MaterialResourceSaveFolderPath);
        folders.Add(OptimizationDataInfo.MaterialResourceSaveFolderPath + OptimizationDataInfo.DirectorySeparator + OptimizationDataInfo.HDSufix);
        return folders;
    }

    public  List<String> GetLODPrefabsFolders()
    {
        List<string> folders = new List<string>();
        string folder = OptimizationDataInfo.PrefabsFolderPath + OptimizationDataInfo.DirectorySeparator + OptimizationDataInfo.MultiSufixFolferName;
        folders.Add(folder);
        folders.Add(OptimizationDataInfo.MaterialPrefabsSavePath);
        return folders;
    }

    public  void ResetSceneMaterialFolder()
    {
        List<String> folders = GetSceneMaterialFolders();
        foreach (string folder in folders)
        {
            EmptyOrCreateADir(folder);
        }
    }

    public  void DeleteSceneMaterialFolder()
    {
        List<String> folders = GetSceneMaterialFolders();
        foreach (string folder in folders)
        {
            DeleteADir(folder);
        }
    }

    public void ResetPrefabsFolder()
    {
        List<String> folders = GetLODPrefabsFolders();
        foreach (string folder in folders)
        {
            DeleteAndCreateADir(folder);
        }
    }

    public string GetResourceLinkFromPath(string path)
    {
        string wordToFind = OptimizationDataInfo.ResourcesFolderName + OptimizationDataInfo.DirectorySeparator;
        int index = path.IndexOf(wordToFind);
        if (index == -1)
        {
            Debug.LogError("Error");
            return null;
        }
        index += wordToFind.Length;
        string rLink = path.Substring(index);
        string extension = System.IO.Path.GetExtension(rLink);
        return rLink.Remove(rLink.Length - (extension.Length));
    }

    public  List<String> GetPrefabsFolderToBeReset()
    {
        List<string> folders = new List<string>();
        string folder = OptimizationDataInfo.PrefabsFolderPath + OptimizationDataInfo.DirectorySeparator + OptimizationDataInfo.MultiSufixFolferName;
        folders.Add(folder);
        folders.Add(OptimizationDataInfo.MaterialPrefabsSavePath);
        folders.Add(OptimizationDataInfo.ResourcePoolsFolderPath);
        return folders;
    }

    protected void Empty(System.IO.DirectoryInfo directory, bool subfolder=false)
    {
        //bool result = false;
        if (subfolder)
        {
            System.IO.DirectoryInfo[] subDirectories = directory.GetDirectories();
            foreach (System.IO.DirectoryInfo subDirectory in subDirectories)
            {
                Empty(subDirectory, true);
            }
        }
        System.IO.FileInfo[] files = directory.GetFiles();
        foreach (System.IO.FileInfo file in files)
        {
            /*result = */UnityEditor.FileUtil.DeleteFileOrDirectory(file.FullName);
            //UnityEngine.Debug.Log("Delete a file " + file.FullName + " result " + result);
        }
    }
}