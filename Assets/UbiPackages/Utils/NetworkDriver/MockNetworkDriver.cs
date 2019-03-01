using System.Net;
using UnityEngine;

public class MockNetworkDriver : MockDriver, NetworkDriver
{
    public ProductionNetworkDriver m_prodDriver = new ProductionNetworkDriver();

    public bool IsMockNetworkReachabilityEnabled { get; set; }
    public NetworkReachability MockNetworkReachability { get; set; }

    public NetworkReachability CurrentNetworkReachability
    {
        get
        {
            if (IsMockNetworkReachabilityEnabled)
            {
                return MockNetworkReachability;
            }
            else
            {
                return m_prodDriver.CurrentNetworkReachability;
            }
        }
    }

    public MockNetworkDriver(GetExceptionTypeToThrowDelegate getExceptionToThrowDelegate) : base(getExceptionToThrowDelegate)    
    {
        IsMockNetworkReachabilityEnabled = false;
    }

    public HttpWebRequest CreateHttpWebRequest(string url)
    {
        EExceptionType exceptionType = GetExceptionTypeToThrow(EOp.CreateHttpWebRequest, url);
        if (exceptionType == MockDriver.EExceptionType.None)
        {
            return m_prodDriver.CreateHttpWebRequest(url);
        }
        else
        {
            ThrowException(exceptionType);
            return null;
        }          
    }    
}
