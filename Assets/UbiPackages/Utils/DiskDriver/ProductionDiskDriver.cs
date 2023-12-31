﻿using System.IO;
using System.Collections.Generic;

public class ProductionDiskDriver : DiskDriver
{    
    public bool Directory_Exists(string path)
    {
        return Directory.Exists(path);
    }

    public DirectoryInfo Directory_CreateDirectory(string path)
    {
        return Directory.CreateDirectory(path);
    }

    public void Directory_Delete(string path)
    {
        Directory.Delete(path);
    }

    public List<string> Directory_GetFiles(string path)
    {
        return new List<string>(Directory.GetFiles(path));        
    }  
    
    public string File_ReadAllText(string path)
    {     
        return File.ReadAllText(path);
    }

    public byte[] File_ReadAllBytes(string path)
    {
        return File.ReadAllBytes(path);
    }

    public void File_WriteAllText(string path, string content)
    {
        File.WriteAllText(path, content);       
    }

    public void File_Write(FileStream fileStream, byte[] arr, int offset, int count)
    {
        fileStream.Write(arr, offset, count);
    }

    public bool File_Exists(string path)
    {
        return File.Exists(path);
    }

    public FileInfo File_GetInfo(string path)
    {
        return new FileInfo(path);
    }

    public FileStream File_Open(FileInfo info, FileMode mode, FileAccess access, FileShare share)
    {
        return info.Open(mode, access, share);
    }    

    public void File_Delete(string path)
    {
        File.Delete(path);
    }    
}
