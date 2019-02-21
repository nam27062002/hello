using System.IO;
using System.Collections.Generic;

/// <summary>
/// Wrapper for disk operations. It's used to be able to offer an alternative implementation that lets us simulate related to disk access errors.
/// </summary>
public class DiskDriver
{    
    public virtual bool Directory_Exists(string path)
    {
        return Directory.Exists(path);
    }

    public virtual DirectoryInfo Directory_CreateDirectory(string path)
    {
        return Directory.CreateDirectory(path);
    }

    public virtual List<string> Directory_GetFiles(string path)
    {
        return new List<string>(Directory.GetFiles(path));        
    }  
    
    public virtual string File_ReadAllText(string path)
    {     
        return File.ReadAllText(path);
    }

    public virtual byte[] File_ReadAllBytes(string path)
    {
        return File.ReadAllBytes(path);
    }

    public virtual void File_WriteAllText(string path, string content)
    {
        File.WriteAllText(path, content);       
    }

    public virtual bool File_Exists(string path)
    {
        return File.Exists(path);
    }

    public virtual FileInfo File_GetInfo(string path)
    {
        return new FileInfo(path);
    }

    public virtual void File_Delete(string path)
    {
        File.Delete(path);
    }
}
