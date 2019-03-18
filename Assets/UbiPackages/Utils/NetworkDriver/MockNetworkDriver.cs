using System.Net;
using UnityEngine;

public class MockNetworkDriver : MockDriver, NetworkDriver
{
#if UNITY_EDITOR
    private static string MOCK_THROTTLE_SLEEP_TIME = "NetworkMockThrottleSleepTime";
    private static int sm_mockThrottleSleepTime;
    public static int MockThrottleSleepTime
    {
        get
        {
            return UnityEditor.EditorPrefs.GetInt(MOCK_THROTTLE_SLEEP_TIME, 0);
        }

        set
        {
            UnityEditor.EditorPrefs.SetInt(MOCK_THROTTLE_SLEEP_TIME, value);           
        }
    }

    private static string MOCK_REACHABILITY = "NetworkMockReachability";
    public static bool IsMockNetworkReachabilityEnabled
    {
        get
        {
            return MockNetworkReachabilityAsInt > -1;
        }

        set
        {
            if (!value)
            {
                MockNetworkReachabilityAsInt = -1;
            }
        }
    }

    private static NetworkReachability sm_mockReachability;
    public static int MockNetworkReachabilityAsInt
    {
        get
        {
            return UnityEditor.EditorPrefs.GetInt(MOCK_REACHABILITY, -1);
        }

        set
        {
            UnityEditor.EditorPrefs.SetInt(MOCK_REACHABILITY, value);
        }
    }

    public static NetworkReachability MockNetworkReachability
    {
        get
        {
            int index = UnityEditor.EditorPrefs.GetInt(MOCK_REACHABILITY, -1);
            return (index == -1) ? NetworkReachability.NotReachable : (NetworkReachability)index;            
        }

        set
        {
            MockNetworkReachabilityAsInt = (int)value;
        }
    }
#endif

    public ProductionNetworkDriver m_prodDriver = new ProductionNetworkDriver();    

    public bool IsMockResponseContentLengthEnabled { get; set; }
    public long MockResponseContentLength { get; set; }

    public bool IsMockResponseStatusCodeEnabled { get; set; }
    public int MockResponseStatusCode { get; set; }    

    public NetworkReachability CurrentNetworkReachability
    {
        get
        {
#if UNITY_EDITOR
            if (IsMockNetworkReachabilityEnabled)
            {
                return MockNetworkReachability;
            }
            else
#endif
            {
                return m_prodDriver.CurrentNetworkReachability;
            }
        }
    }

    public MockNetworkDriver(GetExceptionTypeToThrowDelegate getExceptionToThrowDelegate) : base(getExceptionToThrowDelegate)    
    {        
        IsMockResponseContentLengthEnabled = false;
    }

    public HttpWebRequest CreateHttpWebRequest(string url)
    {
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.CreateHttpWebRequest, url);
        if (exceptionType == EExceptionType.None)
        {
            return m_prodDriver.CreateHttpWebRequest(url);
        }
        else
        {
            ThrowException(exceptionType);
            return null;
        }          
    }

    public HttpWebResponse GetResponse(HttpWebRequest request)
    {
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.GetResponse, "*");
        if (exceptionType == EExceptionType.None)
        {
            return m_prodDriver.GetResponse(request);
        }
        else
        {
            ThrowException(exceptionType);
            return null;
        }
    }

    public long GetResponseContentLength(HttpWebResponse response)
    {
        return (IsMockResponseContentLengthEnabled) ? MockResponseContentLength : m_prodDriver.GetResponseContentLength(response);
    }

    public int GetResponseStatusCodeAsInt(HttpWebResponse response)
    {
        return (IsMockResponseStatusCodeEnabled) ? MockResponseStatusCode : m_prodDriver.GetResponseStatusCodeAsInt(response);
    }

    public int GetThrottleSleepTime()
    {
#if UNITY_EDITOR
        return (MockThrottleSleepTime == 0) ? m_prodDriver.GetThrottleSleepTime() : MockThrottleSleepTime;
#else
        return m_prodDriver.GetThrottleSleepTime();
#endif
    }
}
