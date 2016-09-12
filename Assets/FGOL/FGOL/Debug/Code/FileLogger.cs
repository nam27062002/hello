using System;
using System.IO;
using UnityEngine;

public class FileLogger
{
    private static StreamWriter s_streamWriter = null;

    public static void Init()
    {
        LogCallbackHandler.RegisterLogCallbackThreaded(HandleLog);

        OpenStream();
    }

    public static void DeInit()
    {
        try
        {
            if (s_streamWriter != null)
            {
                s_streamWriter.Close();
            }
        }
        catch (Exception) { }
    }

    private static void HandleLog(string log, string stackTrace, LogType type)
    {
        try
        {
            if (s_streamWriter != null)
            {
                if (s_streamWriter.BaseStream == null)
                {
                    OpenStream();
                }

                if (s_streamWriter != null && s_streamWriter.BaseStream != null)
                {
                    s_streamWriter.WriteLine(string.Format("{0} {1} -- {2}", DateTime.Now, type.ToString().ToUpper(), log));
                    s_streamWriter.WriteLine(stackTrace);
                    s_streamWriter.Flush();
                }
            }
        }
        catch (Exception) { }
    }

    private static void OpenStream()
    {                
        string path = FGOL.Plugins.Native.NativeBinding.Instance.GetPersistentDataPath() + "/game.log";

        UnityEngine.Debug.Log("FileLogger (OpenStream) :: Creating log in location - " + path);

        try
        {
            s_streamWriter = new StreamWriter(path);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("FileLogger (OpenStream) :: Failed to create log file with error - " + e.Message);
        }        
    }
}