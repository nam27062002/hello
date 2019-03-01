using UnityEngine;
using System.Net;

public class ProductionNetworkDriver : NetworkDriver
{
    public virtual NetworkReachability CurrentNetworkReachability
    {
        get
        {
            return Application.internetReachability;
        }        
    }

    public virtual HttpWebRequest CreateHttpWebRequest(string url)
    {
        return (HttpWebRequest)HttpWebRequest.Create(url);
    }
}
