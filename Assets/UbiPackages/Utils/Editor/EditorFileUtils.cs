using System.IO;
using UnityEditor;
using UnityEngine;

public class EditorFileUtils
{
    private static bool VERBOSE = false;

    public static string GetPlatformDirectory(string directory)
    {
        return directory + "/" + EditorUserBuildSettings.activeBuildTarget.ToString();
    }

    public static bool Exists(string path)
    {
        return Directory.Exists(path);
    }

    public static void CreateDirectory(string path)
    {
        if (!string.IsNullOrEmpty(path) && !Exists(path))
        {
            Directory.CreateDirectory(path);           
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

    public static string GetPathWithoutExtension(string path)
    {
        string returnValue = path;
        if (!string.IsNullOrEmpty(path))
        {
            string[] tokens = path.Split('.');
            if (tokens.Length > 1)
            {
                returnValue = "";

                for (int i = 0; i < tokens.Length - 1; i++)
                {
                    if (i > 0)
                    {
                        returnValue += ".";
                    }

                    returnValue += tokens[i];
                }
            }
        }

        return returnValue;
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
                returnValue = GetPathWithoutExtension(returnValue);
            }
        }

        return returnValue;
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
    
    public static int GetFilesAmount(string path)
    {
        string[] files = Directory.GetFiles(path);
        return (files == null) ? 0 : files.Length;
    } 

    /// <summary>
    /// Copy all files in <c>sourceDirectory</c> to <c>destDirectory</c>
    /// </summary>
    /// <param name="sourceDirectory">Directory where the files to move are stored in</param>
    /// <param name="destDirectory">Directory to move the files to</param>
    public static void CopyFilesInDirectory(string sourceDirectory, string destDirectory)
    {
        if (Directory.Exists(sourceDirectory) && Directory.Exists(destDirectory))
        {
            string[] files = Directory.GetFiles(sourceDirectory);
            int count = files.Length;
            string destFileName;
            for (int i = 0; i < count; i++)
            {
                destFileName = EditorFileUtils.PathCombine(destDirectory, Path.GetFileName(files[i]));     

                // If the destDirectory already contains a file with the same name then this file is deleted before copying the one in sourceDirectory
                if (File.Exists(destFileName))
                {
                    File.Delete(destFileName);
                }           

                File.Copy(files[i], destFileName);
            }
        }
    }

    public static void Move(string sourcePath, string destPath)
    {
        if (File.Exists(sourcePath))
        {
            CreateDirectory(Path.GetDirectoryName(destPath));

            if (File.Exists(destPath))
            {
                File.Delete(destPath);
            }

            File.Move(sourcePath, destPath);
        }
    }
}
