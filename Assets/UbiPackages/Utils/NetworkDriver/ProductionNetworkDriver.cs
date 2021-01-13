using UnityEngine;
using System.Net;

public class ProductionNetworkDriver : NetworkDriver
{
    public NetworkReachability CurrentNetworkReachability
    {
        get
        {
            return DeviceUtilsManager.SharedInstance.internetReachability;
        }        
    }

    public HttpWebRequest CreateHttpWebRequest(string url)
    {
        return (HttpWebRequest)HttpWebRequest.Create(url);
    }

    public HttpWebResponse GetResponse(HttpWebRequest request)
    {
        return (HttpWebResponse)request.GetResponse();
    }

    public long GetResponseContentLength(HttpWebResponse response)
    {
        return response.ContentLength;
    }

    public int GetResponseStatusCodeAsInt(HttpWebResponse response)
    {
        return (int)response.StatusCode;
    }

    public int GetThrottleSleepTime()
    {
        return 0;
    }
}
