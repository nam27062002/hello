using System.Collections.Generic;
using System.IO;

public class MockDiskDriver : MockDriver, DiskDriver
{
#if UNITY_EDITOR
    private static string MOCK_NO_FREE_SPACE_ENABLED = "MockDiskNoFreeSpaceEnabled";
    private static string MOCK_NO_ACCESS_PERMISSION_ENABLED = "MockDiskNoAccessPermissionEnabled";

    private static bool sm_isNoFreeSpaceEnabled = UnityEditor.EditorPrefs.GetBool(MOCK_NO_FREE_SPACE_ENABLED, false);
    public static bool IsNoFreeSpaceEnabled
    {        
        get
        {
            return sm_isNoFreeSpaceEnabled;
        }

        set
        {
            sm_isNoFreeSpaceEnabled = value;
            UnityEditor.EditorPrefs.SetBool(MOCK_NO_FREE_SPACE_ENABLED, sm_isNoFreeSpaceEnabled);            
        }
    }

    private static bool sm_isNoAccessPermissionEnabled = UnityEditor.EditorPrefs.GetBool(MOCK_NO_ACCESS_PERMISSION_ENABLED, false);
    public static bool IsNoAccessPermissionEnabled
    {
        get
        {
            return sm_isNoAccessPermissionEnabled;
        }

        set
        {
            sm_isNoAccessPermissionEnabled = value;
            UnityEditor.EditorPrefs.SetBool(MOCK_NO_ACCESS_PERMISSION_ENABLED, sm_isNoAccessPermissionEnabled);
        }
    }
#endif

    private ProductionDiskDriver m_prodDriver = new ProductionDiskDriver();    

    public MockDiskDriver(GetExceptionTypeToThrowDelegate getExceptionToThrowDelegate) : base(getExceptionToThrowDelegate)
    {
    }

    private bool RequiresDiskAccess(EOp op)
    {        
        return RequiresDiskWriteAccess(op) || op == EOp.Directory_Exists || op == EOp.Directory_GetFiles || op == EOp.File_ReadAllText ||
               op == EOp.File_ReadAllBytes || op == EOp.File_Exists || op == EOp.File_GetInfo || op == EOp.File_Open ||
               op == EOp.File_Delete;
    }

    private bool RequiresDiskWriteAccess(EOp op)
    {
        return op == EOp.Directory_CreateDirectory || op == EOp.File_Write || op == EOp.File_WriteAllText;
    }

    public override EExceptionType GetExceptionTypeToThrow(EOp op, string path)
    {
#if UNITY_EDITOR        
        if (IsNoAccessPermissionEnabled && RequiresDiskAccess(op))
        {
            return EExceptionType.UnauthorizedAccess;
        }
        else
#endif
        if (GetFreeSpaceBytes() == 0 && RequiresDiskWriteAccess(op))
        {
            return EExceptionType.IOException;
        }
        else
        {
            return base.GetExceptionTypeToThrow(op, path);
        }
    }

    public bool Directory_Exists(string path)
    {
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.Directory_Exists, path);
        if (exceptionType == EExceptionType.None)
        {
            return m_prodDriver.Directory_Exists(path);
        }
        else
        {
            ThrowException(exceptionType);
            return false;
        }        
    }

    public DirectoryInfo Directory_CreateDirectory(string path)
    {
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.Directory_CreateDirectory, path);
        if (exceptionType == EExceptionType.None)
        {
            return m_prodDriver.Directory_CreateDirectory(path);
        }
        else
        {
            ThrowException(exceptionType);
            return null;
        }        
    }

    public List<string> Directory_GetFiles(string path)
    {
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.Directory_GetFiles, path);
        if (exceptionType == EExceptionType.None)
        {
            return m_prodDriver.Directory_GetFiles(path);
        }
        else
        {
            ThrowException(exceptionType);
            return null;
        }
    }

    public string File_ReadAllText(string path)
    {
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.File_ReadAllText, path);
        if (exceptionType == EExceptionType.None)
        {
            return m_prodDriver.File_ReadAllText(path);
        }
        else
        {
            ThrowException(exceptionType);
            return null;
        }
    }

    public byte[] File_ReadAllBytes(string path)
    {
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.File_ReadAllBytes, path);
        if (exceptionType == EExceptionType.None)
        {
            return m_prodDriver.File_ReadAllBytes(path);
        }
        else
        {
            ThrowException(exceptionType);
            return null;
        }
    }

    public void File_WriteAllText(string path, string content)
    {        
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.File_WriteAllText, path);
        if (exceptionType == EExceptionType.None)
        {
            m_prodDriver.File_WriteAllText(path, content);
        }
        else
        {
            ThrowException(exceptionType);
        }        
    }

    public void File_Write(FileStream fileStream, byte[] arr, int offset, int count)
    {
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.File_Write, "*");
        if (exceptionType == EExceptionType.None)
        {
            m_prodDriver.File_Write(fileStream, arr, offset, count);
        }
        else
        {
            ThrowException(exceptionType);
        }        
    }

    public bool File_Exists(string path)
    {
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.File_Exists, path);
        if (exceptionType == EExceptionType.None)
        {
            return m_prodDriver.File_Exists(path);
        }
        else
        {
            ThrowException(exceptionType);
            return false;
        }       
    }
    
    public FileInfo File_GetInfo(string path)
    {
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.File_GetInfo, path);
        if (exceptionType == EExceptionType.None)
        {
            return m_prodDriver.File_GetInfo(path);
        }
        else
        {
            ThrowException(exceptionType);
            return null;
        }
    }

    public FileStream File_Open(FileInfo info, FileMode mode, FileAccess access, FileShare share)
    {
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.File_Open, info.Name);
        if (exceptionType == EExceptionType.None)
        {
            return m_prodDriver.File_Open(info, mode, access, share);
        }
        else
        {
            ThrowException(exceptionType);
            return null;
        }
    }

    public void File_Delete(string path)
    {
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.File_Delete, path);
        if (exceptionType == EExceptionType.None)
        {
            m_prodDriver.File_Delete(path);
        }
        else
        {
            ThrowException(exceptionType);     
        }
    }

    public long GetFreeSpaceBytes()
    {
#if UNITY_EDITOR
        return (IsNoFreeSpaceEnabled) ? 0 : m_prodDriver.GetFreeSpaceBytes();
#else
        return m_prodDriver.GetFreeSpaceBytes();
#endif
    }
}
