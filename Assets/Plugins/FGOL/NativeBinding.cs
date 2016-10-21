
namespace FGOL.Plugins.Native
{
    public class NativeBinding
    {
        private static readonly INativeImplementation m_implementation = null;

        public static INativeImplementation Instance { get { return m_implementation; } }

        private NativeBinding()
        {
        }

        static NativeBinding()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            m_implementation = new NativeImplementationAndroid();
#elif UNITY_IPHONE && !UNITY_EDITOR
            m_implementation = new NativeImplementationIOS();
#elif UNITY_EDITOR
            m_implementation = new NativeImplementationEditor();          
#elif UNITY_STANDALONE_WIN
            m_implementation = new NativeImplementationPC();
#endif
        }
    }
}