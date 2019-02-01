using UnityEditor;
using UnityEngine;

public class FileEditorTools
{
    private static bool VERBOSE = false;

    public static void DeleteFileOrDirectory(string path)
    {
        if (VERBOSE)
        {
            Debug.Log("Deleting " + path);
        }

        //if (File.Exists(path))
        {
            FileUtil.DeleteFileOrDirectory(path);
        }
    }

    public static string PathCombine(string path1, string path2)
    {
        return path1 + "/" + path2;
        //return Path.Combine(path1, path2);
    }

    public static void CopyFileOrDirectory(string srcPath, string dstPath)
    {
        if (VERBOSE)
        {
            Debug.Log("Copy " + srcPath + " into " + dstPath);
        }

        FileUtil.CopyFileOrDirectory(srcPath, dstPath);
    }

    public static void RenameFile(string oldPath, string newPath)
    {
        if (VERBOSE)
        {
            Debug.Log("Rename " + oldPath + " to " + newPath);
        }

        FileUtil.ReplaceFile(oldPath, newPath);
    }
}
