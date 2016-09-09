using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// [DGR] CONFIG: Not supported yet
//using FGOL.Configuration;
using BestHTTP;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using System.Text;

namespace FGOL.Server
{
    public class Request
    {
        public enum Method
        {
            POST,
            GET,
            PUT
        }

        public delegate void OnCompleteDG(Error error, HTTPResponse response = null);
        public static Action OnUnrecoverableError = null;

        public static bool DebugDisableConnection = false;

        protected int m_connectionTimeout = 30;
        protected int m_timeout = 30;

        public static X509Certificate2Collection certStore = null;

        protected void Run(string url, Method method, Hashtable headers, Dictionary<string, string> parameters, byte[] data, OnCompleteDG callback, string sslValidationDomain = null)
        {
            // [DGR] CONFIG: Not supported yet            
            /*
            System.Diagnostics.Stopwatch sw = null;            
            if (Convert.ToBoolean(Config.Instance["request.profile"]))
			{
            	sw = new System.Diagnostics.Stopwatch();
            	sw.Start();
			}
            */

            HTTPMethods httpMethod = HTTPMethods.Get;

            switch (method)
            {
                case Method.POST:
                    httpMethod = HTTPMethods.Post;
                    break;
                case Method.PUT:
                    httpMethod = HTTPMethods.Put;
                    break;
            }

            Uri uri = new Uri(url);

            HTTPRequest req = new HTTPRequest(uri, httpMethod, delegate(HTTPRequest request, HTTPResponse response)
            {
                // [DGR] CONFIG: not supported yet
                /*
                if(sw != null)
                {
                    sw.Stop();
                    Debug.Log(String.Format("FGOL.Server.Request :: Response Time for URL: {0} - Elapsed: {1}ms", url, sw.ElapsedMilliseconds));
                }                

                if (Convert.ToBoolean(Config.Instance["request.dumpHeaders"]))
                {
                    if (request != null)
                    {
                        Debug.Log("FGOL.Server.Request :: Dumping Request Headers");
                        Debug.Log(request.DumpHeaders());
                    }

                    if (response != null && response.Headers != null)
                    {
                        Debug.Log("FGOL.Server.Request :: Dumping Response Headers");

                        string responseHeaders = null;

                        foreach(var header in response.Headers)
                        {
                            if (responseHeaders != null)
                            {
                                responseHeaders += "\n";
                            }

                            responseHeaders += header.Key + ": " + header.Value[0];
                        }

                        Debug.Log(responseHeaders);
                    }
                }
                */

                if(DebugDisableConnection)
                {
                    callback(new TimeoutError("DEBUG TIMEOUT"));
                }
                else
                {
                    switch(request.State)
                    {
                        case HTTPRequestStates.Finished:
                            callback(null, response);
                            break;
                        case HTTPRequestStates.Error:
                            Error error = new UnknownError();

                            if(request.Exception != null)
                            {
                                string message = request.Exception.Message;

                                if(request.Exception is System.Net.Sockets.SocketException && message == "Connection refused")
                                {
                                    error = new ServerConnectionError(message);
                                    Debug.LogError("FGOL.Server.Request :: Unable to connect to server at URL - " + url);
                                }
                                else
                                {
                                    error = new UnknownError(message);
                                    Debug.LogError("FGOL.Server.Request :: Exception occured - " + message);
                                    Debug.LogError("FGOL.Server.Request :: URL - " + url);
                                    Debug.LogError("FGOL.Server.Request :: " + request.Exception.StackTrace);
                                }
                            }
                            else if(response != null)
                            {
                                error = new UnknownError(string.Format("Status Code: {0} - {1}", response.StatusCode, response.Message));
                                Debug.LogError("FGOL.Server.Request :: Error occured - " + error);
                            }

                            callback(error);
                            break;
                        case HTTPRequestStates.Aborted:
                            callback(new UnknownError("Request Aborted!"));
                            break;
                        case HTTPRequestStates.ConnectionTimedOut:
                            callback(new TimeoutError("Connection Timed Out!", ErrorCodes.ClientConnectionTimeout));
                            break;
                        case HTTPRequestStates.TimedOut:
                            callback(new TimeoutError("Server Timed Out!", ErrorCodes.ServerConnectionTimeout));
                            break;
                    }
                }
            });

            if (!string.IsNullOrEmpty(sslValidationDomain) && uri.Scheme.ToLower() == "https")
            {
                FGOL.Assert.Fatal(certStore != null);

                req.CustomCertificationValidator += delegate (HTTPRequest request, X509Certificate cert, X509Chain origChain){

                    Debug.Log("Request (Run) :: Validating SSL Certificate");

                    bool validated = false;

                    if (cert != null)
                    {
                        X509Chain chain = new X509Chain();

                        X509Certificate2Enumerator iter = certStore.GetEnumerator();

                        while (iter.MoveNext())
                        {
                            chain.ChainPolicy.ExtraStore.Add(iter.Current);
                        }

                        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.IgnoreWrongUsage;
                        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;

                        X509Certificate2 serverCert = new X509Certificate2(cert);
                        chain.Build(serverCert);

                        if (chain.ChainStatus.Length == 0 || chain.ChainStatus[0].Status == X509ChainStatusFlags.NoError || chain.ChainStatus[0].Status == X509ChainStatusFlags.UntrustedRoot)
                        {
                            string serverDomain = serverCert.GetNameInfo(X509NameType.DnsName, false);

                            if (string.Compare(sslValidationDomain, serverDomain, true) == 0)
                            {
                                validated = true;
                            }
                            else
                            {
                                Debug.LogWarning(string.Format("Request (Run) :: Invalid server domain value! Expected %s, Received %s", sslValidationDomain, serverDomain));
                            }
                        }
                        else
                        {
                            Debug.LogWarning("Request (Run) :: Failed building cert chain");

                            if (chain.ChainStatus.Length > 0)
                            {
                                Debug.LogWarning("Request (Run) :: Chain error - " + chain.ChainStatus[0].Status);
                            }
                            else
                            {
                                Debug.LogWarning("Request (Run) :: Unknown build error");
                            }
                        }
                    }
                    else
                    {
                        Debug.LogWarning("Request (Run) :: No valid server certificate received");
                    }

                    return validated;
                };
            }

            if (headers != null)
            {
                foreach(DictionaryEntry pair in headers)
                {
                    req.AddHeader(pair.Key as string, pair.Value as string);
                }
            }

            if(data != null)
            {
                req.RawData = data;
            }
            else if (parameters != null)
            {
                foreach(KeyValuePair<string, string> pair in parameters)
                {
                    req.AddField(pair.Key, pair.Value);
                }

                req.FormUsage = BestHTTP.Forms.HTTPFormUsage.UrlEncoded;
            }

            req.ConnectTimeout = TimeSpan.FromSeconds(m_connectionTimeout);
            req.Timeout = TimeSpan.FromSeconds(m_timeout);
            req.DisableCache = true;
            req.IsCookiesEnabled = true;

            if(!DebugDisableConnection)
            {
                req.Send();
            }
            else
            {
                callback(new TimeoutError("DEBUG TIMEOUT"));
            }
        }
    }
}
