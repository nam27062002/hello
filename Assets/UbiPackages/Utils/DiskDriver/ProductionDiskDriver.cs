using System.IO;
using System.Collections.Generic;
using UnityEngine;

public class ProductionDiskDriver : DiskDriver
{
    private float m_realtimeSinceStartup;
    private long m_latestFreeSpace = -1;
    private float m_latesFreeSpaceAt = 0f;

    public bool Directory_Exists(string path)
    {
        return Directory.Exists(path);
    }

    public DirectoryInfo Directory_CreateDirectory(string path)
    {
        return Directory.CreateDirectory(path);
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

    public long GetFreeSpaceBytes()
    {
        if (m_latestFreeSpace < 0 || m_realtimeSinceStartup - m_latesFreeSpaceAt > 2f)
        {
			m_latestFreeSpace = long.MaxValue; //DeviceUtilsManager.SharedInstance.GetDeviceFreeDiskSpace();
            m_latesFreeSpaceAt = m_realtimeSinceStartup;
        }

		return m_latestFreeSpace;
    }

    public void Update()
    {
        m_realtimeSinceStartup = Time.realtimeSinceStartup;
    }
}
