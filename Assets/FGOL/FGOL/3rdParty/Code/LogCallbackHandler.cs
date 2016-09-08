//REF: http://blog.dreasgrech.com/2014/07/a-workaround-for-allowing-multiple.html
//Modified by TC to deal with Threaded log callback as well

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unity only handles a single delegate registered with Application.RegisterLogCallback
/// http://feedback.unity3d.com/suggestions/change-application-dot-registerlogcallback-to-allow-multiple-callbacks
/// 
/// This class is used to work around that by allowing multiple delegates to hook to the log callback.
/// </summary>
public static class LogCallbackHandler
{
    private static readonly List<Application.LogCallback> callbacks;
    private static readonly List<Application.LogCallback> threadedCallbacks;

    static LogCallbackHandler()
    {
        callbacks = new List<Application.LogCallback>();
        threadedCallbacks = new List<Application.LogCallback>();

        Application.logMessageReceived += HandleLog;
        Application.logMessageReceivedThreaded += HandleLogThreaded;
    }

    /// <summary>
    /// Register a delegate to be called on log messages.
    /// </summary>
    /// <param name="logCallback"></param>
    public static void RegisterLogCallback(Application.LogCallback logCallback)
    {
        callbacks.Add(logCallback);
    }

    /// <summary>
    /// Register a delegate to be called on log messages threaded.
    /// </summary>
    /// <param name="logCallback"></param>
    public static void RegisterLogCallbackThreaded(Application.LogCallback logCallback)
    {
        threadedCallbacks.Add(logCallback);
    }

    public static void HandleLog(string log, string stackTrace, LogType type)
    {
        for(var i = 0; i < callbacks.Count; i++)
        {
            callbacks[i](log, stackTrace, type);
        }
    }

    public static void HandleLogThreaded(string log, string stackTrace, LogType type)
    {
        for(var i = 0; i < threadedCallbacks.Count; i++)
        {
            threadedCallbacks[i](log, stackTrace, type);
        }
    }
}