using UnityEngine;
using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Net;
using System.IO;
using System.Text;
using System.Threading;

#pragma warning disable 414

public class NetworkManager : SingletonMonoBehaviour<NetworkManager>
{
	
	
#if UNITY_IOS && !UNITY_EDITOR
	[DllImport ("__Internal", CallingConvention=CallingConvention.Cdecl)] private static extern string NetworkClient_GetDefaultProxyURL ();
	[DllImport ("__Internal", CallingConvention=CallingConvention.Cdecl)] private static extern int NetworkClient_GetDefaultProxyPort ();
#endif

	private bool debug = false;

	public class HttpRequestCreator : IWebRequestCreate {
		public WebRequest Create(Uri uri)
		{
			// return new HttpWebRequest(uri);	
			return HttpWebRequest.Create(uri);
		}
	}
	
	override public string ToString() {
		return "NetworkManager"; 
	}

	//////////////////////////////////////////////////////////////////////////

	public static int NUM_NETWORK_RETRIES = 3;

	public static int NUM_NETWORK_RETRIES_FOR_RELOAD = 2;

	private static int HTTP_TIMEOUT_CONNECT = 15000;
	private static int HTTP_TIMEOUT_READING = 30000;

	//////////////////////////////////////////////////////////////////////////

	public enum NetworkResult
	{
		NETWORK_RESULT_OK,
		NETWORK_RESULT_CANCELLED,
		NETWORK_RESULT_FAILED,

		NETWORK_RESULT_UNKNOWN
	}

	//////////////////////////////////////////////////////////////////////////
	public delegate void OnResquestWWWResponse(WWWResponse response);


	public class NetworkRequest
	{
		public static int TIME_BETWEEN_RETRIES = 1000;
		
		public enum eNetworkRequestType
		{
			NETWORK_REQUEST_SEND_PACKET,
			NETWORK_REQUEST_DOWNLOAD_FILE
		};
		
		public NetworkRequest(string strURL, byte[] strBody, Dictionary<string, string> headers, bool sendEncoded, bool receiveEncoded, OnResquestWWWResponse callback, object objectX)
		{
			m_eRequestType = eNetworkRequestType.NETWORK_REQUEST_SEND_PACKET;
			m_strMethod = "";
			m_strKey = "";
			m_strURL = strURL;
			m_strSaveFile = "";
			m_kHeaders = headers;
			m_strBody = strBody;
			m_bSendEncoded = sendEncoded;
			m_bReceiveEncoded = receiveEncoded;
			m_bRequestSent = false;
			m_kServerResponse = null;
			m_eProcessRequestResult = NetworkResult.NETWORK_RESULT_UNKNOWN;
			m_dCallback = callback;
			m_kObjectX = objectX;
		}
		public NetworkRequest(string strKey, string strURL, Dictionary<string, string> headers, string strSaveFile, OnResquestWWWResponse callback, object objectX)
		{
			m_eRequestType = eNetworkRequestType.NETWORK_REQUEST_DOWNLOAD_FILE;
			m_strMethod = "";
			m_strKey = strKey;
			m_strURL = strURL;
			m_strSaveFile = strSaveFile;
			m_strBody = new byte[]{};
			m_kHeaders = headers;
			m_bSendEncoded = false;
			m_bReceiveEncoded = false;
			m_bRequestSent = false;
			m_kServerResponse = null;
			m_eProcessRequestResult = NetworkResult.NETWORK_RESULT_UNKNOWN;
			m_dCallback = callback;
			m_kObjectX = objectX;
		}
		
		public eNetworkRequestType     	m_eRequestType;
		public string             		m_strMethod;
		public string             		m_strKey;
		public string             		m_strURL;
		public string             		m_strSaveFile;
		public byte[]             		m_strBody;
		public bool                    	m_bSendEncoded;
		public bool                    	m_bReceiveEncoded;
		
		public bool                    	m_bRequestSent;
		public NetworkResult			m_eProcessRequestResult;
		public object					m_kObjectX;

		public Dictionary<string, string> 		m_kHeaders;
		public WWWResponse	m_kServerResponse;

		public OnResquestWWWResponse m_dCallback;
	};

	///////////////////////////////////////////////////////////////////////////



	private string m_strServerURL;
	private string m_strServerVersion;
	private string m_strServerApplicationSecretKey;

	private List<NetworkRequest> m_kPendingNetworkRequests;
	private List<NetworkRequest> m_kPendingNetworkDownloads;

	private NetworkRequest m_pCurrentRequestBeingProcessed = null;
	private NetworkRequest m_pCurrentDownloadBeingProcessed = null;

	private int m_iNetworkProblemCounter;
	private int m_iNetworkProblemCounterForReload;

	//private DeltaTimer m_kNetworkRequestRetryTimer;
	private DeltaTimer m_kNetworkDownloadRetryTimer;

	private bool m_bReportNetworkProblem;
	private bool m_bNetworkPaused;
	private bool m_bNoNetwork;
	private bool m_bNetworkBehaviourInfiniteLoop;
	private bool m_bShowingNetworkIcon;

	private Thread m_kNetworkRequestsThread;
	private Mutex m_kNetworkRequestsMutex;

	private WebProxy m_kCurrentProxySettings;

	// INTERNAL METHODS ///////////////////////////////////////////////////////
	private WebProxy GetCurrentProxySettings()
	{
		WebProxy proxy = null;

#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaObject jo = new AndroidJavaObject("java.lang.System");
		
		string strProxyHost = jo.CallStatic<string>("getProperty", "http.proxyHost");
		string strProxyPort = jo.CallStatic<string>("getProperty", "http.proxyPort");
		
		if(string.IsNullOrEmpty(strProxyHost) == false && string.IsNullOrEmpty(strProxyPort) == false) 
		{
			proxy = new WebProxy(strProxyHost, int.Parse(strProxyPort));
		}
#elif UNITY_IOS && !UNITY_EDITOR
		string strProxyHost = NetworkClient_GetDefaultProxyURL ();
		int iProxyPort = NetworkClient_GetDefaultProxyPort ();

		if(string.IsNullOrEmpty(strProxyHost) == false && iProxyPort != -1) 
		{
			proxy = new WebProxy(strProxyHost, iProxyPort);
		}
#else
		proxy = new WebProxy("bcn-net-proxy.ubisoft.org", 3128);
#endif

		if (proxy != null)
		{
			Debug.Log("Using proxy: " + proxy.Address);
		}

		return proxy;
	}
	void NetworkRequestsThreadWorker()
	{ 
		//Catch and report any exceptions here, 
		//so that Unity doesn't crash!
		try
		{
			while(true)
			{
				//Play nice with other threads...
				Thread.Sleep(10); 

				if (!m_bNetworkPaused)
				{
					bool bNeedToProcessPacket = false;

					//Wait till it is safe to work with GameObjects.
					if (m_kNetworkRequestsMutex.WaitOne())
					{
						if (m_kPendingNetworkRequests.Count > 0)
						{
							NetworkRequest pRequestToProcess = m_kPendingNetworkRequests[0];

							if (pRequestToProcess.m_eProcessRequestResult != NetworkResult.NETWORK_RESULT_OK)
							{
								/*bool bTimeToRetryHasPassed = false;
								if (m_kNetworkRequestRetryTimer.IsFinished ())
								{
									bTimeToRetryHasPassed = true;
								}*/
								
								if (pRequestToProcess.m_eProcessRequestResult == NetworkResult.NETWORK_RESULT_UNKNOWN && !pRequestToProcess.m_bRequestSent/* && bTimeToRetryHasPassed*/)
								{
									m_pCurrentRequestBeingProcessed = pRequestToProcess;
									
									bNeedToProcessPacket = true;
								}
							}
						}

						if (bNeedToProcessPacket)
						{
							m_pCurrentRequestBeingProcessed.m_bRequestSent = true;

							m_kNetworkRequestsMutex.ReleaseMutex();

							UploadPacket_Internal (m_pCurrentRequestBeingProcessed);
						}
						else
						{
							m_kNetworkRequestsMutex.ReleaseMutex();
						}
					}
				}
			}
		}
		catch(Exception e)
		{
			if(!(e is ThreadAbortException))
			{
				if (debug)
					Debug.LogError("Unexpected Death: " + e.ToString()); 
			}
		}
	}
	private void UploadPacket_Internal (NetworkRequest networkRequest)
	{
		byte[] strFinalBody = networkRequest.m_strBody;

		if (debug)
			Debug.Log ("UploadPacket_Internal: " + networkRequest.m_strURL + " - body: " + (strFinalBody != null? System.Text.Encoding.UTF8.GetString(strFinalBody) : "(null)"));

		HttpWebRequest request = (HttpWebRequest)WebRequest.Create(networkRequest.m_strURL);

		if (m_kCurrentProxySettings != null)
		{
			request.Proxy = m_kCurrentProxySettings;
		}

		request.Timeout = HTTP_TIMEOUT_CONNECT;
		request.ReadWriteTimeout = HTTP_TIMEOUT_READING;

		if(strFinalBody != null)
		{
			string strEncrypted = System.Text.Encoding.UTF8.GetString(strFinalBody);
			if(networkRequest.m_bSendEncoded)
			{
				AESEncryptionVault crypto = new AESEncryptionVault("sirocco");
				strEncrypted = crypto.Decrypt(strEncrypted);
			}
			if(debug)
				Debug.Log(strEncrypted);
		}
		request.AllowAutoRedirect = false;
		request.Method = "POST";
		request.ContentType = "application/x-www-form-urlencoded";
		request.Accept = "Accept=text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8";
		foreach(string header in networkRequest.m_kHeaders.Keys) 
		{
			if ( header != "Content-Type" )
				request.Headers.Add(header, networkRequest.m_kHeaders[header]);	
			else
				request.ContentType = networkRequest.m_kHeaders[header];
			if (debug)
				Debug.Log("Headers[" + header + "] = " + networkRequest.m_kHeaders[header]);
		}
			

		if(strFinalBody != null) {
			request.ContentLength = strFinalBody.Length;
		}

		Stream pRequestStream = null;

		try
		{
			pRequestStream = request.GetRequestStream();
		}
		catch(System.Net.WebException e){
			if (debug)
				Debug.Log ("Exception: " + e.Message); 
		} catch(Exception e) {
			if (debug)
				Debug.Log ("Exception: " + e.Message);
		}

		bool bRequestProcessFailed = false;
		
		HttpWebResponse response = null;
		string strServerResponse = "";
		bool bResponseWasOK = false;

		int statusCode = -1;
		if (pRequestStream != null)
		{
			//using (StreamWriter writer = new StreamWriter(pRequestStream, Encoding.UTF8))
			{
				if(strFinalBody != null) {
					pRequestStream.Write (strFinalBody, 0, strFinalBody.Length);
				}
				pRequestStream.Dispose();
			}

			try
			{
				response = (HttpWebResponse) request.GetResponse();
			}
			catch(System.Net.WebException ex){
				strServerResponse = "none";
				if (debug)
					Debug.Log ("Exception UPLOAD PACKET Protocol error = " + (ex.Status == WebExceptionStatus.ProtocolError));
				if(ex.Response != null)
				{
					StreamReader reader = (new StreamReader(ex.Response.GetResponseStream()));
					strServerResponse = reader.ReadToEnd();
					statusCode = (int)((HttpWebResponse)ex.Response).StatusCode;
					reader.Close();
				} else {
					if (debug)
						Debug.Log ("UPLOAD PACKET No response in exception!");
				}
				if (debug)
					Debug.Log ("Exception UPLOAD PACKET Status : " + ex.Status.ToString() + "\nException: " + ex.ToString () + "\nResponse: " + strServerResponse);
				if (debug)
					Debug.Log ("Exception UPLOAD PACKET web exception response [" + statusCode + "]: " + (ex.Response == null));
				response = null;
			}

			if (response != null)
			{
				if(response.StatusCode == HttpStatusCode.OK)
				{
					bResponseWasOK = true;

					m_bReportNetworkProblem = false;
					m_iNetworkProblemCounter = 0;
					m_iNetworkProblemCounterForReload = 0;
					m_bNoNetwork = false;
				}
				
				if (response.ContentLength != 0)
				{
					using (StreamReader reader = new StreamReader(response.GetResponseStream()))
					{
						strServerResponse = reader.ReadToEnd();
					}
				}
				if (debug)
					Debug.Log("UPLOAD PACKET Status : " + (int)response.StatusCode);
				if (debug)
					Debug.Log("UPLOAD PACKET Response : " + strServerResponse);
			}
			else
			{
				bRequestProcessFailed = true;
			}
		}
		
		if (bRequestProcessFailed)
		{
			//m_kNetworkRequestRetryTimer.Start(NetworkRequest.TIME_BETWEEN_RETRIES);
			
			m_bReportNetworkProblem = true;
			m_iNetworkProblemCounter++;
			
			//m_bNoNetwork = m_bNoNetwork || (response.StatusCode == 5678);
		}

		if (m_kNetworkRequestsMutex.WaitOne())
		{
			if(m_pCurrentRequestBeingProcessed == null){
				if(debug){
					Debug.Log ("WARNING! UploadPacket_Internal m_pCurrentRequestBeingProcessed is NULL");
				}
			}
			if (bResponseWasOK)
			{
				m_pCurrentRequestBeingProcessed.m_eProcessRequestResult = NetworkResult.NETWORK_RESULT_OK;
			}
			else
			{
				m_pCurrentRequestBeingProcessed.m_eProcessRequestResult = NetworkResult.NETWORK_RESULT_FAILED;
			}

			m_pCurrentRequestBeingProcessed.m_bRequestSent = false;

			// create response from strServerResponse
			WWWResponse kResp = new WWWResponse();
			kResp.body = strServerResponse;
			if(response != null)
			{
				kResp.responseCode = (int)response.StatusCode;
			}
			else
			{
				kResp.responseCode = (int)statusCode;
			}
			kResp.success = bResponseWasOK;
			kResp.objectX = m_pCurrentRequestBeingProcessed.m_kObjectX;
			kResp.errorMessage = "";
			// --------------------------------------

			m_pCurrentRequestBeingProcessed.m_kServerResponse = kResp;

			m_kNetworkRequestsMutex.ReleaseMutex();
		}

		if(response != null)
		{
			response.Close();
		}
	}

	private void OnDownloadComplete(object sender, AsyncCompletedEventArgs e)
	{
		bool bDownloadProcessFailed = false;

		if (m_kNetworkRequestsMutex.WaitOne())
		{
			if (e.Cancelled)
			{
				bDownloadProcessFailed = true;

				m_pCurrentDownloadBeingProcessed.m_eProcessRequestResult = NetworkResult.NETWORK_RESULT_CANCELLED;
			}
			else
			{
				if (e.Error == null)
				{
					m_bReportNetworkProblem = false;
					m_iNetworkProblemCounter = 0;
					m_iNetworkProblemCounterForReload = 0;
					m_bNoNetwork = false;
					
					m_pCurrentDownloadBeingProcessed.m_eProcessRequestResult = NetworkResult.NETWORK_RESULT_OK;
				}
				else
				{
					bDownloadProcessFailed = true;

					m_pCurrentDownloadBeingProcessed.m_eProcessRequestResult = NetworkResult.NETWORK_RESULT_FAILED;
				}
			}

			if (bDownloadProcessFailed)
			{
				m_kNetworkDownloadRetryTimer.Start(NetworkRequest.TIME_BETWEEN_RETRIES);
				
				m_bReportNetworkProblem = true;
				m_iNetworkProblemCounter++;
				
				//m_bNoNetwork = m_bNoNetwork || (response.StatusCode == 5678);
			}

			m_pCurrentDownloadBeingProcessed.m_bRequestSent = false;
			
			m_kNetworkRequestsMutex.ReleaseMutex();
		}
	}
	private void OnUpdateDownloadProgress(object sender, DownloadProgressChangedEventArgs e)
	{
		if (debug)
			Debug.Log("Download progress: " + e.BytesReceived);
	}
	private void DownloadFile_Internal (string strKey, string strURL, string strSaveFile)
	{
		if (debug)
			Debug.Log ("DownloadFile_Internal");

		WebClient pWebClient = new WebClient();

		pWebClient.DownloadFileCompleted += new AsyncCompletedEventHandler (OnDownloadComplete);
		pWebClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(OnUpdateDownloadProgress);

		if (m_kCurrentProxySettings != null)
		{
			pWebClient.Proxy = m_kCurrentProxySettings;
		}

		Uri uri = new Uri(strURL);
		pWebClient.DownloadFileAsync(uri, strSaveFile);
	}
	///////////////////////////////////////////////////////////////////////////



	// METHODS ///////////////////////////////////////////////////////////////
	public void Initialise (string strServerURL, string strNetworkApplicationSecretKey, string strServerVersion)
	{
		m_strServerURL = strServerURL;
		m_strServerVersion = strServerVersion;
		m_strServerApplicationSecretKey = strNetworkApplicationSecretKey;

	}
	public void UploadPacket (string strURL, byte[] strBody, ref Dictionary<string, string> kHeaders, bool bRequestEncoded, bool bResponseEncoded, OnResquestWWWResponse callback, object objectX)
	{
		NetworkRequest pNewNetworkRequest = new NetworkRequest (strURL, strBody, kHeaders, bRequestEncoded, bResponseEncoded, callback, objectX);
		
		if (!m_kNetworkRequestsMutex.SafeWaitHandle.IsInvalid && m_kNetworkRequestsMutex.WaitOne())
		{
			m_kPendingNetworkRequests.Add (pNewNetworkRequest);
			
			m_kNetworkRequestsMutex.ReleaseMutex ();
		}
	}

	public void DownloadFile (string strKey, string strURL, string strSaveFile, OnResquestWWWResponse callback, object objectX)
	{
		NetworkRequest pNewNetworkDownload = new NetworkRequest (strKey, strURL, null, strSaveFile, callback, objectX);
		
		if (!m_kNetworkRequestsMutex.SafeWaitHandle.IsInvalid && m_kNetworkRequestsMutex.WaitOne())
		{
			m_kPendingNetworkDownloads.Add (pNewNetworkDownload);
			
			m_kNetworkRequestsMutex.ReleaseMutex ();
		}
	}
	//////////////////////////////////////////////////////////////////////////



	// GETTERS ///////////////////////////////////////////////////////////////

	//////////////////////////////////////////////////////////////////////////



	// UNITY METHODS /////////////////////////////////////////////////////////
	void Awake ()
	{
		WebRequest.RegisterPrefix("http", new HttpRequestCreator()); // new! fix to the new random NotSupportedException-Whatever

		m_kPendingNetworkRequests = new List<NetworkRequest> ();
		m_kPendingNetworkDownloads = new List<NetworkRequest> ();

		m_iNetworkProblemCounter = 0;
		m_iNetworkProblemCounterForReload = 0;

		//m_kNetworkRequestRetryTimer = new DeltaTimer ();
		m_kNetworkDownloadRetryTimer = new DeltaTimer ();

		m_bReportNetworkProblem = false;
		m_bNetworkPaused = false;
		m_bNoNetwork = false;
		m_bNetworkBehaviourInfiniteLoop = false;
		m_bShowingNetworkIcon = false;

		m_kNetworkRequestsThread = new Thread(NetworkRequestsThreadWorker);
		m_kNetworkRequestsMutex = new Mutex();

		ServicePointManager.DefaultConnectionLimit = 4;
		ServicePointManager.Expect100Continue = false;

		m_kCurrentProxySettings = GetCurrentProxySettings ();
	}
	void Start()
	{
		m_kNetworkRequestsThread.Start(); 
	}
	void OnApplicationQuit()
	{
		m_kNetworkRequestsThread.Abort(); 
	}
	void Update ()
	{
		if (!m_bNetworkPaused)
		{
			bool bNeedToProcessDownload = false;
			
			if (m_kNetworkRequestsMutex.WaitOne())
			{
				if (m_pCurrentRequestBeingProcessed != null)
				{
					if (m_pCurrentRequestBeingProcessed.m_eProcessRequestResult != NetworkResult.NETWORK_RESULT_UNKNOWN)
					{
						/*switch(m_pCurrentRequestBeingProcessed.m_eProcessRequestResult)
						{
						case NetworkResult.NETWORK_RESULT_OK:
						{*/
							m_kPendingNetworkRequests.RemoveAt(0);
							NetworkRequest request = m_pCurrentRequestBeingProcessed;
							m_pCurrentRequestBeingProcessed = null;
						
							if (debug)
								Debug.Log ("Network Request Response: " + request.m_kServerResponse.body);
							request.m_dCallback(request.m_kServerResponse);

						/*}break;
						
						case NetworkResult.NETWORK_RESULT_CANCELLED:
						{
							Debug.Log ("Network Request cancelled.");

							m_pCurrentRequestBeingProcessed.m_eProcessRequestResult = NetworkResult.NETWORK_RESULT_UNKNOWN;
							
						}break;
						
						case NetworkResult.NETWORK_RESULT_FAILED:
						{
							Debug.Log ("Network Request failed.");
							
							m_pCurrentRequestBeingProcessed.m_eProcessRequestResult = NetworkResult.NETWORK_RESULT_UNKNOWN;
							
						}break;

						}*/
					}
				}

				if (m_pCurrentDownloadBeingProcessed != null)
				{
					if (m_pCurrentDownloadBeingProcessed.m_eProcessRequestResult != NetworkResult.NETWORK_RESULT_UNKNOWN)
					{
						switch(m_pCurrentDownloadBeingProcessed.m_eProcessRequestResult)
						{
							case NetworkResult.NETWORK_RESULT_OK:
							{
								m_kPendingNetworkDownloads.RemoveAt(0);
								if (debug)
									Debug.Log ("Network Download succeed.");
								
								break;
							}
							case NetworkResult.NETWORK_RESULT_CANCELLED:
							{
								if (debug)
									Debug.Log ("Network Download cancelled.");
								
								m_pCurrentDownloadBeingProcessed.m_eProcessRequestResult = NetworkResult.NETWORK_RESULT_UNKNOWN;
								
								break;
							}
							case NetworkResult.NETWORK_RESULT_FAILED:
							{
								if (debug)
									Debug.Log ("Network Download failed.");
								
								m_pCurrentDownloadBeingProcessed.m_eProcessRequestResult = NetworkResult.NETWORK_RESULT_UNKNOWN;
								
								break;
							}
						}

						m_pCurrentDownloadBeingProcessed = null;
					}
				}
				else
				{
					if (m_kPendingNetworkDownloads.Count > 0)
					{
						NetworkRequest pRequestToProcess = m_kPendingNetworkDownloads[0];

						if (pRequestToProcess.m_eProcessRequestResult == NetworkResult.NETWORK_RESULT_OK)
						{
							m_kPendingNetworkDownloads.RemoveAt(0);
						}
						else
						{
							bool bTimeToRetryHasPassed = false;
							if (m_kNetworkDownloadRetryTimer.Finished ())
							{
								bTimeToRetryHasPassed = true;
							}
							
							if (pRequestToProcess.m_eProcessRequestResult == NetworkResult.NETWORK_RESULT_UNKNOWN && !pRequestToProcess.m_bRequestSent && bTimeToRetryHasPassed)
							{
								m_pCurrentDownloadBeingProcessed = pRequestToProcess;
								
								bNeedToProcessDownload = true;
							}
						}
					}

					if (bNeedToProcessDownload)
					{
						m_pCurrentDownloadBeingProcessed.m_bRequestSent = true;

						DownloadFile_Internal (m_pCurrentDownloadBeingProcessed.m_strKey, m_pCurrentDownloadBeingProcessed.m_strURL, m_pCurrentDownloadBeingProcessed.m_strSaveFile);
					}
				}

				m_kNetworkRequestsMutex.ReleaseMutex ();
			}
		}
	}
	//////////////////////////////////////////////////////////////////////////
}
