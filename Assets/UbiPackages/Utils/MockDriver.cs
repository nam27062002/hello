using System;
using System.Collections.Generic;
using System.IO;

public class MockDriver
{
    public enum EExceptionType
    {
        None,
        UnauthorizedAccess,
        IOException,
        UriFormatException
    };

    public enum EOp
    {
        None, 

        // Disk
        Directory_Exists,
        Directory_CreateDirectory,
        Directory_GetFiles,
        File_ReadAllText,
        File_ReadAllBytes,
        File_WriteAllText,
        File_Write,
        File_Exists,
        File_GetInfo,
        File_Open,
        File_Delete,        

        // Network
        CreateHttpWebRequest,
        GetResponse
    }

    public class ExceptionConf
    {
        public EOp Op;
        public string Parameter;
        public EExceptionType ExceptionType;

        public ExceptionConf(EOp op, string parameter, EExceptionType exceptionType)
        {
            Op = op;
            Parameter = parameter;
            ExceptionType = exceptionType;
        }
    }

    private Dictionary<EOp, Dictionary<string, EExceptionType>> m_expectionByOp = new Dictionary<EOp, Dictionary<string, EExceptionType>>();

    public delegate EExceptionType GetExceptionTypeToThrowDelegate(EOp op, string parameter);
    private GetExceptionTypeToThrowDelegate m_getExceptionTypeToThrowDelegate;

    public MockDriver(GetExceptionTypeToThrowDelegate getExceptionToThrowDelegate)
    {
        m_getExceptionTypeToThrowDelegate = getExceptionToThrowDelegate;
    }

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
        SetExceptionTypeToThrow(conf.Op, conf.Parameter, conf.ExceptionType);
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
        if (m_getExceptionTypeToThrowDelegate != null)
        {
            returnValue = m_getExceptionTypeToThrowDelegate(op, path);
        }
        else if (m_expectionByOp.ContainsKey(op))
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

    public void ThrowException(EExceptionType exceptionType)
    {
        switch (exceptionType)
        {
            case EExceptionType.UnauthorizedAccess:
                throw new UnauthorizedAccessException();

            case EExceptionType.IOException:
                throw new IOException();

            case EExceptionType.UriFormatException:
                throw new UriFormatException();
        }
    }
}
