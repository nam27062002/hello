using System.IO;
using UnityEditor;
using UnityEngine;

public class FileEditorTools
{
    private static bool VERBOSE = false;

    public static bool Exists(string path)
    {
        return Directory.Exists(path);
    }

    public static void CreateDirectory(string path)
    {
        if (!string.IsNullOrEmpty(path) && !Exists(path))
        {            
            //string[] tokens = path.Split(Path.DirectorySeparatorChar);
            string[] tokens = path.Split('/');
            int count = tokens.Length;
            string pathSoFar = "";
            string newPath;
            for (int i = 0; i < count; i++)
            {
                newPath = PathCombine(pathSoFar, tokens[i]);
                if (!Exists(newPath))
                {
                    AssetDatabase.CreateFolder(pathSoFar,tokens[i]);
                }

                pathSoFar = newPath;
            }
        }            
    }

    public static void DeleteFileOrDirectory(string path)
    {
        if (VERBOSE)
        {
            Debug.Log("Deleting " + path);
        }

        if (AssetDatabase.IsValidFolder(path))
        {            
            AssetDatabase.DeleteAsset(path);
        }
        else
        {
            FileUtil.DeleteFileOrDirectory(path);
        }
    }

    public static string PathCombine(string path1, string path2)
    {              
        if (string.IsNullOrEmpty(path1))
        {
            return path2;
        }
        else if (string.IsNullOrEmpty(path2))
        {
            return path1;
        }
        else
        {
            return path1 + "/" + path2;
        }
            
        //return Path.Combine(path1, path2);        
    }

    public static string[] SeparatePathInTokens(string path)
    {
        //return (string.IsNullOrEmpty(path)) ? null : path.Split(Path.DirectorySeparatorChar);
        return (string.IsNullOrEmpty(path)) ? null : path.Split('/');
    }

    public static string GetFileName(string path, bool includeExtension)
    {
        string[] tokens = SeparatePathInTokens(path);
        string returnValue;
        if (tokens == null)
        {
            returnValue = null;
        }
        else
        {
            returnValue = tokens[tokens.Length - 1];
            if (!includeExtension)
            {
                tokens = returnValue.Split('.');
                if (tokens.Length > 1)
                {
                    returnValue = tokens[0];
                }
            }
        }

        return returnValue;
    }

    public static void CopyDirectory(string srcPath, string dstPath)
    {
        if (VERBOSE)
        {
            Debug.Log("Copy " + srcPath + " into " + dstPath);
        }
      
        if (Exists(srcPath))
        {
            if (!Exists(dstPath))
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
                dstFile = PathCombine(dstPath, fileName);
                File.Copy(s, dstFile, true);
            }

            // Copies the subdirectories            
            string dstSubdirectoryPath;
            string[] subdirectories = Directory.GetDirectories(srcPath);
            foreach (string s in subdirectories)
            {
                // Uses static Path methods to extract only the file name from the path.
                fileName = Path.GetFileName(s);                
                dstSubdirectoryPath = PathCombine(dstPath, fileName);
                CopyDirectory(s, dstSubdirectoryPath);
            }
        }        
    }

    public static void RenameFile(string oldPath, string newPath)
    {
        if (VERBOSE)
        {
            Debug.Log("Rename " + oldPath + " to " + newPath);
        }

        FileUtil.ReplaceFile(oldPath, newPath);
    }
    
    public static void WriteToFile(string path, string content, bool append = false)
    {
        StreamWriter writer = new StreamWriter(path, append);
        writer.WriteLine(content);
        writer.Close();
    }    
}
