using System;
using System.Collections.Generic;
using System.IO;

public class MockDiskDriver : DiskDriver
{
    public enum EExceptionType
    {
        None,
        UnauthorizedAccess,
        IOException
    };

    public enum EOp
    {
        Directory_Exists,
        Directory_CreateDirectory,
        Directory_GetFiles,
        File_ReadAllText,
        File_ReadAllBytes,
        File_WriteAllText,
        File_Exists,
        File_GetInfo,
        File_Delete
    }

    public class ExceptionConf
    {
        public EOp Op;
        public string Path;
        public EExceptionType ExceptionType;

        public ExceptionConf(EOp op, string path, EExceptionType exceptionType)
        {
            Op = op;
            Path = path;
            ExceptionType = exceptionType;
        }
    }

    private Dictionary<EOp, Dictionary<string, EExceptionType>> m_expectionByOp = new Dictionary<EOp, Dictionary<string, EExceptionType>>();

    public void ClearAllExceptionTypeToThrow()
    {
        m_expectionByOp.Clear();
    }

    public void SetExceptionTypeToThrow(EExceptionType exceptionType)
    {
        int count = Enum.GetValues(typeof(EOp)).Length;
        for (int i = 0; i < count; i++)
        {
            SetExceptionTypeToThrow((EOp)i, "*", exceptionType);
        }        
    }

    public void SetExceptionTypeToThrow(ExceptionConf conf)
    {
        SetExceptionTypeToThrow(conf.Op, conf.Path, conf.ExceptionType);
    }

    public void SetExceptionTypeToThrow(EOp op, string path, EExceptionType exceptionType)
    {
        if (!m_expectionByOp.ContainsKey(op))
        {
            m_expectionByOp.Add(op, new Dictionary<string, EExceptionType>());
        }

        if (m_expectionByOp[op].ContainsKey(path))
        {
            m_expectionByOp[op][path] = exceptionType;
        }
        else
        {
            m_expectionByOp[op].Add(path, exceptionType);
        }
    }

    public EExceptionType GetExceptionTypeToThrow(EOp op, string path)
    {
        EExceptionType returnValue = EExceptionType.None;
        if (m_expectionByOp.ContainsKey(op))
        {
            if (m_expectionByOp[op].ContainsKey(path))
            {
                returnValue = m_expectionByOp[op][path];
            }
            else if (m_expectionByOp[op].ContainsKey("*"))
            {
                returnValue = m_expectionByOp[op]["*"];
            }
        }

        return returnValue;
    }

    private void ThrowException(EExceptionType exceptionType)
    {
        switch (exceptionType)
        {
            case EExceptionType.UnauthorizedAccess:
                throw new UnauthorizedAccessException();

            case EExceptionType.IOException:
                throw new IOException();                

        }        
    }

    public override bool Directory_Exists(string path)
    {
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.Directory_Exists, path);
        if (exceptionType == EExceptionType.None)
        {
            return base.Directory_Exists(path);
        }
        else
        {
            ThrowException(exceptionType);
            return false;
        }        
    }

    public override DirectoryInfo Directory_CreateDirectory(string path)
    {
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.Directory_CreateDirectory, path);
        if (exceptionType == EExceptionType.None)
        {
            return base.Directory_CreateDirectory(path);
        }
        else
        {
            ThrowException(exceptionType);
            return null;
        }        
    }

    public override List<string> Directory_GetFiles(string path)
    {
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.Directory_GetFiles, path);
        if (exceptionType == EExceptionType.None)
        {
            return base.Directory_GetFiles(path);
        }
        else
        {
            ThrowException(exceptionType);
            return null;
        }
    }

    public override string File_ReadAllText(string path)
    {
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.File_ReadAllText, path);
        if (exceptionType == EExceptionType.None)
        {
            return base.File_ReadAllText(path);
        }
        else
        {
            ThrowException(exceptionType);
            return null;
        }
    }

    public override byte[] File_ReadAllBytes(string path)
    {
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.File_ReadAllBytes, path);
        if (exceptionType == EExceptionType.None)
        {
            return base.File_ReadAllBytes(path);
        }
        else
        {
            ThrowException(exceptionType);
            return null;
        }
    }

    public override void File_WriteAllText(string path, string content)
    {
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.File_WriteAllText, path);
        if (exceptionType == EExceptionType.None)
        {
            base.File_WriteAllText(path, content);
        }
        else
        {
            ThrowException(exceptionType);            
        }
    }    

    public override bool File_Exists(string path)
    {
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.File_Exists, path);
        if (exceptionType == EExceptionType.None)
        {
            return base.File_Exists(path);
        }
        else
        {
            ThrowException(exceptionType);
            return false;
        }       
    }
    
    public override FileInfo File_GetInfo(string path)
    {
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.File_GetInfo, path);
        if (exceptionType == EExceptionType.None)
        {
            return base.File_GetInfo(path);
        }
        else
        {
            ThrowException(exceptionType);
            return null;
        }
    }

    public override void File_Delete(string path)
    {
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.File_Delete, path);
        if (exceptionType == EExceptionType.None)
        {
            base.File_Delete(path);
        }
        else
        {
            ThrowException(exceptionType);     
        }
    }    
}
