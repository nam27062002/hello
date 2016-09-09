//[DGR] Commented out to prevent this class from hidding the one in Calety
/*using UnityEngine;
using System.Diagnostics;

public static class Debug
{
	public static bool isDebugBuild = UnityEngine.Debug.isDebugBuild;

	public static void Log(object message)
    {
        UnityEngine.Debug.Log(message);
    }

    public static void Log(object message, UnityEngine.Object context)
    {
        UnityEngine.Debug.Log(message, context);
    }

	#if !ENABLE_LOG || PRODUCTION
	[Conditional("FALSE")]
	#endif
    public static void LogError(object message)
    {
        UnityEngine.Debug.LogError(message);
    }

	#if !ENABLE_LOG || PRODUCTION
	[Conditional("FALSE")]
	#endif
    public static void LogError(object message, UnityEngine.Object context)
    {
        UnityEngine.Debug.LogError(message, context);
    }

	#if !ENABLE_LOG || PRODUCTION
	[Conditional("FALSE")]
	#endif
    public static void LogWarning(object message)
    {
        UnityEngine.Debug.LogWarning(message.ToString());
    }

	#if !ENABLE_LOG || PRODUCTION
	[Conditional("FALSE")]
	#endif
    public static void LogWarning(object message, UnityEngine.Object context)
    {
        UnityEngine.Debug.LogWarning(message.ToString(), context);
    }

	#if !ENABLE_LOG || PRODUCTION
	[Conditional("FALSE")]
	#endif
    public static void DebugBreak()
    {
        UnityEngine.Debug.DebugBreak();
    }

	#if !ENABLE_LOG || PRODUCTION
	[Conditional("FALSE")]
	#endif
    public static void DrawLine(Vector3 start, Vector3 end)
    {
        DrawLine(start, end, Color.white);
    }

	#if !ENABLE_LOG || PRODUCTION
	[Conditional("FALSE")]
	#endif
    public static void DrawLine(Vector3 start, Vector3 end, Color color)
    {
        UnityEngine.Debug.DrawLine(start, end, color);
    }

	#if !ENABLE_LOG || PRODUCTION
	[Conditional("FALSE")]
	#endif
	public static void DrawLine(Vector3 start, Vector3 end, Color color, float duration, bool depthTest)
	{
		UnityEngine.Debug.DrawLine(start, end, color, duration, depthTest);
	}

	#if !ENABLE_LOG || PRODUCTION
	[Conditional("FALSE")]
    #endif
    
    public static void Assert(bool condition, string message="")
	{
        //[DGR] Implementation changed to prevent Dragon Assert from being hidden
        //UnityEngine.Debug.Assert(condition);
        DebugUtils.Assert(condition, message);
    }

#if !ENABLE_LOG || PRODUCTION
    [Conditional("FALSE")]
	#endif
	public static void LogException(System.Exception exception)
	{
		UnityEngine.Debug.LogException(exception);
	}

	#if !ENABLE_LOG || PRODUCTION
	[Conditional("FALSE")]
	#endif
	public static void LogException(System.Exception exception, Object context)
	{
		UnityEngine.Debug.LogException(exception, context);
	}
}
*/