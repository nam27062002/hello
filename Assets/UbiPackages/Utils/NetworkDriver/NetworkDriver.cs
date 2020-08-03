using UnityEngine;
using System.Net;

/// <summary>
/// Wrapper for network operations. It's used to be able to offer an alternative implementation that lets us simulate related to network errors.
/// </summary>
public interface NetworkDriver
{    
    NetworkReachability CurrentNetworkReachability { get; }
    HttpWebRequest CreateHttpWebRequest(string url);
    HttpWebResponse GetResponse(HttpWebRequest request);
    long GetResponseContentLength(HttpWebResponse response);
    int GetResponseStatusCodeAsInt(HttpWebResponse response);
    int GetThrottleSleepTime();
}
