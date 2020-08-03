using System.IO;
using System.Collections.Generic;

/// <summary>
/// Wrapper for disk operations. It's used to be able to offer an alternative implementation that lets us simulate related to disk access errors.
/// </summary>
public interface DiskDriver
{
    bool Directory_Exists(string path);
    DirectoryInfo Directory_CreateDirectory(string path);
    void Directory_Delete(string path);
    List<string> Directory_GetFiles(string path);

    string File_ReadAllText(string path);
    byte[] File_ReadAllBytes(string path);
    void File_WriteAllText(string path, string content);
    void File_Write(FileStream fileStream, byte[] arr, int offset, int count);
    bool File_Exists(string path);
    FileInfo File_GetInfo(string path);
    FileStream File_Open(FileInfo info, FileMode mode, FileAccess access, FileShare share);
    void File_Delete(string path);        
}
