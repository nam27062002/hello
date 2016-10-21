using System.Diagnostics;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FGOL
{
    public static class Assert
    {
		#if !ENABLE_LOG || PRODUCTION
		[Conditional("FALSE")]
		#endif
        public static void Warn(bool condition, string message = "Assertion", Object context = null)
        {
            if(!condition)
            {
                StackFrame frame = (new StackTrace(true)).GetFrame(1);

                string fileName = frame.GetFileName();
                int lineNumber = frame.GetFileLineNumber();

                string assertInfo = string.Format("Filename: {0}\nMethod: {1}\nLine: {2}", fileName, frame.GetMethod(), lineNumber);
                string assertMessage = string.Format("{0}\n{1}", message, assertInfo);

				UnityEngine.Debug.LogError("Assertion :: " + assertMessage, context);
#if UNITY_EDITOR
                if(EditorUtility.DisplayDialog("Assert!", assertMessage, "Break", "Continue"))
                {
                    UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(frame.GetFileName(), frame.GetFileLineNumber());
                    UnityEngine.Debug.Break();
                }
#endif
            }
        }

		public static bool Expect(bool condition, string message = "Expected", Object context = null)
        {
            if(!condition)
            {
                StackFrame frame = (new StackTrace(true)).GetFrame(1);

                string fileName = frame.GetFileName();
                int lineNumber = frame.GetFileLineNumber();

                string assertInfo = string.Format("Filename: {0}\nMethod: {1}\nLine: {2}", fileName, frame.GetMethod(), lineNumber);
                string assertMessage = string.Format("{0}\n{1}", message, assertInfo);

				UnityEngine.Debug.LogError("Expect :: " + assertMessage, context);
				
				#if UNITY_EDITOR
                if(EditorUtility.DisplayDialog("Expect!", assertMessage, "Break", "Continue"))
                {
                    UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(frame.GetFileName(), frame.GetFileLineNumber());
                    UnityEngine.Debug.Break();
				}
				#endif
            }
            return condition;
        }

		#if !ENABLE_LOG || PRODUCTION
		[Conditional("FALSE")]
		#endif
		public static void Fatal(bool condition, string message = "Fatal", Object context = null)
        {
            if(!condition)
            {
                StackFrame frame = (new StackTrace(true)).GetFrame(1);

                string fileName = frame.GetFileName();
                int lineNumber = frame.GetFileLineNumber();

                string assertInfo = string.Format("Filename: {0}\nMethod: {1}\nLine: {2}", fileName, frame.GetMethod(), lineNumber);
                string assertMessage = string.Format("{0}\n{1}", message, assertInfo);

				UnityEngine.Debug.LogError("Fatal :: " + assertMessage, context);

#if UNITY_EDITOR
                if(EditorUtility.DisplayDialog("Fatal!", assertMessage, "Open File", "Stop"))
                {
                    UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(frame.GetFileName(), frame.GetFileLineNumber());
                }

                UnityEngine.Debug.Break();
#endif
            }
        }
    }
}