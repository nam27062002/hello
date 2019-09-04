using System.Net;
public class NetworkManager
{
    private static NetworkManager s_pInstance = null;

    public static NetworkManager SharedInstance
    {
        get
        {
            if (s_pInstance == null)
            {
                s_pInstance = new NetworkManager();
            }

            return s_pInstance;
        }
    }

    private WebProxy m_kCurrentProxySettings = null;
    private bool m_bNeedToRetrieveProxySettings = true;

    public WebProxy GetCurrentProxySettings(bool bForce = false)
    {
        if (!bForce)
        {
            if (m_kCurrentProxySettings != null)
                return m_kCurrentProxySettings;

            if (!m_bNeedToRetrieveProxySettings)
                return null;
        }

        m_bNeedToRetrieveProxySettings = false;

        WebProxy proxy = null;

        
#if UNITY_ANDROID && !UNITY_EDITOR && false
        System.IntPtr descPtr = NetworkClient_GetDefaultProxyURL();
        string strProxyHost = Marshal.PtrToStringAnsi(descPtr);

        int iProxyPort = NetworkClient_GetDefaultProxyPort ();

        if(string.IsNullOrEmpty(strProxyHost) == false && iProxyPort != -1) 
        {
            proxy = new WebProxy(strProxyHost, iProxyPort);
        }
#elif UNITY_IOS && !UNITY_EDITOR && false
		System.IntPtr descPtr = NetworkClient_GetDefaultProxyURL();
		string strProxyHost = Marshal.PtrToStringAnsi(descPtr);

        int iProxyPort = NetworkClient_GetDefaultProxyPort ();

        if(string.IsNullOrEmpty(strProxyHost) == false && iProxyPort != -1) 
        {
            proxy = new WebProxy(strProxyHost, iProxyPort);
        }
#else
#if UNITY_EDITOR_OSX && false
        System.IntPtr descPtr = NetworkClient_GetDefaultProxyURL();
        string strProxyHost = Marshal.PtrToStringAnsi(descPtr);

        int iProxyPort = NetworkClient_GetDefaultProxyPort ();

        if(string.IsNullOrEmpty(strProxyHost) == false && iProxyPort != -1) 
        {
            proxy = new WebProxy(strProxyHost, iProxyPort);
        }
#else
        proxy = new WebProxy("bcn-net-proxy.ubisoft.org", 3128);
#endif
#endif

        // When Env is LOCAL we remove the proxy
        /*if (m_kCurrentServerEnvironment == CaletyConstants.eBuildEnvironments.BUILD_LOCAL)
        {
            proxy = null;
        }*/

        if (proxy != null)
        {
            proxy.BypassProxyOnLocal = false;

            UnityEngine.Debug.Log("Using proxy: " + proxy.Address);
        }

        m_kCurrentProxySettings = proxy;

        return m_kCurrentProxySettings;
    }
}