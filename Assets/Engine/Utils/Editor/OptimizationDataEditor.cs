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

    public  void DeleteADir(string path)
    {
        path = StringUtil.FormatPathToProjectRoot(path);
        AssetDatabase.DeleteAsset(path);       
    }

    public  void DeleteAndCreateADir(string path)
    {
        path = StringUtil.FormatPathToProjectRoot(path);        
        DeleteADir(path);
        CreateADir(path);
    }
    
    public  void CreateADir(string path)
    {
        path = StringUtil.FormatPathToProjectRoot(path);
        string[] tokens = path.Split('/');
        string parent = "";
        int count = tokens.Length;
        for (int i = 0; i < count - 1; i++)
        {
            if (i > 0)
                parent += "/";

            parent += tokens[i];
        }

        AssetDatabase.CreateFolder(parent, tokens[count - 1]);
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

    public  List<String> GetSceneMaterialFolders()
    {
        List<string> folders = new List<string>();
        folders.Add(OptimizationDataInfo.MaterialResourceSaveFolderPath);
        folders.Add(OptimizationDataInfo.MaterialResourceSaveFolderPath + OptimizationDataInfo.DirectorySeparator + OptimizationDataInfo.HDSufix);
        return folders;
    }   

    /*public  void ResetSceneMaterialFolder()
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
    }*/   

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