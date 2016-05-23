/// <summary>
/// 17/Jan/2014
/// David Germade
/// 
/// This class is creating and offering the different objects used intensely by the rest of the class all around the project.
/// </summary>

using UnityEngine;
using SimpleJSON;
using System.Collections;
using System.Collections.Generic;

public class NetworkUtils : MonoBehaviour
{
	private static NetworkUtils instance;

	// If shutting down
	private static bool isShuttingDown = false;

	public static bool firstLoadingbarCompleted = false;    

	public static NetworkUtils Instance
	{
		get
		{
			if (instance == null)
			{
				bool _lightLoading = !Application.isPlaying;

				//
				// InstanceMng
				//
				GameObject go = new GameObject("Singleton - InstanceMng");
				DontDestroyOnLoad(go);
				instance = go.AddComponent<NetworkUtils>();                
			}

			return instance;
		}
	}

	#region Monobehaviours_utils
	public void StartChildCoroutine(IEnumerator coroutineMethod)
	{
		StartCoroutine(coroutineMethod);
	}

	public void StopChildCoroutine(IEnumerator coroutineMethod)
	{
		StopCoroutine(coroutineMethod);
	}

	public delegate void OnResquestWWWResponse(WWWResponse response);

	public void RequestWWW(string url, OnResquestWWWResponse onResponse = null)
	{

		StartCoroutine(LaunchWWW(url, null, null, onResponse));

	}

	public void RequestWWW(string url, byte[] body = null, Dictionary<string, string> headers = null, OnResquestWWWResponse onResponse = null, object objectX = null)
	{

		StartCoroutine(LaunchWWW(url, body, headers, onResponse, objectX));        
	}

	private IEnumerator LaunchWWW(string url, byte[] body = null, Dictionary<string, string> headers = null, OnResquestWWWResponse onResponse = null, object objectX = null)
	{
		if (headers == null) { headers = new Dictionary<string, string>(); }

		WWW www = null;

		Debug.Log("calling url = " + url);
		foreach (string h in headers.Keys)
		{
			Debug.Log("headers[" + h + "] = " + headers[h]);
		}
		www = new WWW(url, body, headers);

		yield return www;

		if (onResponse != null)
		{
			Debug.Log("www = " + www.text);
			WWWResponse response = new WWWResponse();

			response.www = www;
			response.objectX = objectX;

			//			foreach(string key in www.responseHeaders.Keys) {Debug.Log("### response.header: " + key + " = " + www.responseHeaders[key]);}

			bool forceUpdateResponseCode = true;
			if (string.IsNullOrEmpty(www.error))
			{
				Debug.Log("www.responseHeaders = " + www.responseHeaders.ToString());
				if (www.responseHeaders.ContainsKey("STATUS"))
				{
					Debug.Log("www.responseHeaders[\"STATUS\"] = " + www.responseHeaders["STATUS"].ToString());

					string[] statusValues = www.responseHeaders["STATUS"].Split(new char[] { ' ' });
					if (statusValues.Length >= 2)
					{
						forceUpdateResponseCode = !int.TryParse(statusValues[1], out response.responseCode);
					}
				}

				response.success = true;
				response.body = www.text;

				if (forceUpdateResponseCode)
				{
					response.responseCode = 200;
				}
			}
			else
			{
				Debug.Log("www.error : " + www.error.ToString());

				// get error code
				string[] tokens = www.error.Trim().Split(new char[] { ':', ' ' });
				if (tokens.Length > 0)
				{
					forceUpdateResponseCode = !int.TryParse(tokens[0], out response.responseCode);
				}

				response.success = false;
				response.errorMessage = www.error;
				if (forceUpdateResponseCode)
				{
					response.responseCode = 404;
				}
			}

			onResponse(response);
		}

		www.Dispose();
	}

	public class WWWResponse
	{
		public int responseCode;
		public bool success = false;
		public string body = null;
		public string errorMessage = null;
		public object objectX = null;
		public WWW www = null;
	}

	#endregion


	#region temporal data

	public JSONClass tempCustomizerJObj = null;
	public JSONClass tempAssetsLUT = null;

	#endregion



	#region Delayed Method Call

	public delegate void OnDelayedMethodCall();

	public Coroutine DelayedMethodCall(OnDelayedMethodCall methodToCall, float secondsToWait)
	{
		return StartCoroutine(DelayedMethodCallCoroutine(methodToCall, secondsToWait));
	}

	private IEnumerator DelayedMethodCallCoroutine(OnDelayedMethodCall methodToCall, float secondsToWait)
	{
		yield return new WaitForSeconds(secondsToWait);

		methodToCall();
	}

	#endregion



}
