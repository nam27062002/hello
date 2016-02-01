
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.Net;
using System.Security.Cryptography;

using SimpleJSON;

/**
 * Class used for all the communication with OUR servers.
 */
public class Server
{
	// ----------------------------------------------------------------------- //

	private const bool debug = true;
	private const string DEBUG_TAG = "[SERVER] ";

	// ----------------------------------------------------------------------- //

	private const int STACK_ACTIONS_MIN_TIME = 2000;	// min. wait before send last action in stack.
	private const int STACK_ACTIONS_MAX_TIME = 7000;	// max. wait before send all actions in stack.

	private const int ACTIONS_RETRY_TIMEOUT = 15000;	// time-out to send a RETRY of last command (action-list)

	private int mCacheUpdatesMinTimer = 0;		// minimum milliseconds without new commands to wait before send a a new packet to server
	private int mCacheUpdatesMaxTimer = 0;		// maximun milliseconds waitting from first command to send a packet to server

	private bool mForceSendAllNow = false;

		// ----------------------------------------------------------------------- //
	
	string serverUrl;

	Dictionary<string,string> headers = null;

	// Commands To send
	private List<Command> commandsStack = new List<Command>();

	private readonly UnityEngine.Object locker = new UnityEngine.Object();

	private List<int> errorCodesWithRetry = new List<int> ();

	private long currentTimeMillisOnLogin;


	/**
	 * 
	 */
	public Server(string serverUrl, string clientVersion, string clientBuild)
	{
		this.serverUrl = serverUrl;

		headers = new Dictionary<string, string>();
		headers.Add("X-ClientBuild", clientBuild);
		headers.Add("X-Version", clientVersion);
		headers.Add("X-Model", UnityEngine.SystemInfo.deviceModel);
#if UNITY_IPHONE
		headers.Add("X-Brand", "iOS");
#else
		headers.Add("X-Brand", UnityEngine.SystemInfo.deviceModel.Split(' ')[0]); // usually the manufacturer comes as the first word of the deviceModel
#endif


		errorCodesWithRetry.Add (502);		// 503: Bad Gateway
		errorCodesWithRetry.Add (404);		// 404: Not Found

		NetworkManager.SharedInstance.Initialise(serverUrl, ENCRYPT_PASSWORD, clientVersion);
	}


	// ----------------------------------------------------------------------- //


	public void LogicUpdate()
	{
		GamePlayActionsLogicUpdate();

		CommandsLogicUpdate();
	}


	// ----------------------------------------------------------------------- //

	private bool mWaittingCommandResponse = false;

	private class Command
	{
		public string url;
		public Dictionary<string,string> headers;
		public byte[] data;
		public string action;
		public RequestNetworkOnline.OnResponse onResponse;
		public RequestNetworkOnline.OnResponseError onResponseError;
		public int retry;

		public double sentAt;
	}

	/**
	 * 
	 */
	public void SendCommand(string commandName, RequestNetworkOnline.OnResponse onResponse = null, RequestNetworkOnline.OnResponseError onResponseError = null)
	{
		SendCommand (commandName, null, onResponse, onResponseError);
	}

	/**
	 * 
	 */
	public void SendCommand(string commandName, byte[] body, RequestNetworkOnline.OnResponse onResponse = null, RequestNetworkOnline.OnResponseError onResponseError = null)
	{
		SendCommand(commandName, null, body, onResponse, onResponseError);
	}

	/**
	 * 
	 */
	public void SendCommand(string commandName, Dictionary<string, string> urlParams, byte[] body, RequestNetworkOnline.OnResponse onResponse = null, RequestNetworkOnline.OnResponseError onResponseError = null)
	{
		SendCommand(commandName, urlParams, null, body, onResponse, onResponseError);
	}

	/**
	 * 
	 */
	public void SendCommand(string commandName, Dictionary<string, string> urlParams, Dictionary<string, string> headerParams, byte[] body, RequestNetworkOnline.OnResponse onResponse = null, RequestNetworkOnline.OnResponseError onResponseError = null)
	{
		if (debug) Debug.Log (DEBUG_TAG + "Request COMMAND: " + commandName );
		if (debug && body != null) Debug.Log (DEBUG_TAG + " - body : " + System.Text.Encoding.UTF8.GetString(body));
		
		Command command = new Command ();
		command.url = serverUrl + commandName;
		
		// Add parameters to the url
		if ( urlParams != null )
		{
			string concatenator = "?";
			foreach( KeyValuePair<string,string> pair in urlParams)
			{
				command.url += concatenator +  pair.Key + "=" + pair.Value;
				concatenator = "&";
			}
		}

		// add parameters to the header
		if (headerParams == null)
		{
			headerParams = new Dictionary<string, string>();
		}

		foreach (string key in headers.Keys)
		{
			headerParams[key] = headers[key];
		}

		// headerParams["X-Platform"] = InstanceManager.RequestNetwork.platform;

		command.headers = headerParams;
		command.data = body;
		command.action = commandName;
		command.onResponse = onResponse;
		command.onResponseError = onResponseError;
		
		lock( locker )
		{
			commandsStack.Add( command );
		}
	}

	/**
	 * 
	 */
	private void CommandsLogicUpdate(bool flushAllCommandsNow = false)
	{
		if (commandsStack.Count > 0)
		{
			if (!mWaittingCommandResponse || flushAllCommandsNow)
			{
				do
				{
					Command command = null;
					lock( locker )
					{
						command = commandsStack[0];
						commandsStack.RemoveAt(0);
					}

					mWaittingCommandResponse = true;

					// command.sentAt = InstanceManager.RequestNetwork.GetServerTime();

					// InstanceManager.Instance.RequestWWW (command.url, command.data, command.headers, OnResquestWWWResponse, command);
				}
				while (flushAllCommandsNow && commandsStack.Count > 0);
			}
		}
	}

	/**
	 * 
	 */
	private void OnResquestWWWResponse(WWWResponse response)
	{
		mWaittingCommandResponse = false;

		Command command = response.objectX as Command;

		// Latency_AddEntry ((float)(InstanceManager.RequestNetwork.GetServerTime() - command.sentAt));

//		Debug.Log ("[LATENCY] " + Latency_GetLastEntryDuration() + " // " + Latency_GetAverageDuration());

/*
		if (++pepe == 7)
		{
			response.success = false;
			response.responseCode = RequestNetwork.NET_ERROR_UPDATE_REQUIRED;
		}
*/
		Debug.Log(response.ToString());

		if (response.success)
		{
			if (debug) Debug.Log(DEBUG_TAG + "Response COMMAND: " + command.action + " / Success [" + response.responseCode + "] / Body: " + response.body);

			if (command.onResponse != null)
			{
				command.onResponse(response.body, command.action, ""+response.responseCode);
			}
		}
		else
		{
			Debug.Log(DEBUG_TAG + "Response COMMAND: " + command.action + " / FAILED [" + response.responseCode + "] / ERROR: " + response.errorMessage);

			// if a know errorCode.. then retry...
			if (errorCodesWithRetry.Contains(response.responseCode))
			{
				// if (command.retry < InstanceManager.Config.GetNumberOfRetriesOnServerResponseError())
				{
					command.retry++;
					mWaittingCommandResponse = true;
					// InstanceManager.Instance.RequestWWW (command.url, command.data, command.headers, OnResquestWWWResponse, command);
					return;
				}
			}

			if (response.responseCode == RequestNetwork.NET_ERROR_UPDATE_REQUIRED)
			{
				if (!logged)
				{
					// UNDO the START event of loading funnel
					// InstanceManager.MetricsManager.LoadingFunnel(MetricsManager.ELoadingFunnelStep.START, true);
				}

				Logout();

				// InstanceManager.FlowManager.NotifyCriticalError(false, response.responseCode, null);				
			}
			else if (command.onResponseError != null)
			{
				/*string errorResponse = "";
				if ( response.www.responseHeaders.ContainsKey("X-ERRORRESPONSE"))
				{
					errorResponse = response.www.responseHeaders["X-ERRORRESPONSE"];
				}*/

				command.onResponseError(response.responseCode, response.errorMessage, response.body, command.action);
			}
			else
			{
				Logout();
				
				// InstanceManager.FlowManager.NotifyCriticalError(false, response.responseCode, null);
			}
		}
	}



	// ----------------------------------------------------------------------- //
	// ----------------------------------------------------------------------- //
	// ----------------------------------------------------------------------- //


	string uid;

	int sessionId = 0;
	int packetId = 0;
	int actionId = 1;
	string token;

	bool logged = false;

	RequestNetworkOnline.OnResponse onGamePlayResponse = null;

	/**
	 * 
	 */
	public bool IsLogged()
	{
		return logged;
	}

	/**
	 * 
	 */
	public void Logout()
	{
		logged = false;

		gamePlayActionsStack.Clear();
	}

	/**
	 * 
	 */
	public void SendLogin(string uid, Dictionary<string,string> data, string token, RequestNetworkOnline.OnResponse onResponse)
	{
		if (IsLogged())
		{
			Logout();
		}

		mForceSendAllNow = false;
		gamePlayActionsStack.Clear();

		this.uid = uid;
		onGamePlayResponse = onResponse;

		// JSON object
		JSONClass node = new JSONClass();
		node.Add("cmd", "login");
		// node.Add("checksum", "" + DefinitionsManager.Instance.getRulesCRC());
		// data.Add( "checksum", "" + DefinitionsManager.Instance.getRulesCRC());
		node.Add("data", CreateCommandArgs( data ));
		string stringData = node.ToString();

		// if (InstanceManager.Config.IsGameplayEncryptedEnabled())
		{
			stringData = simpleStringEncrypt(stringData);
		}

		WWWForm form = new WWWForm();
		form.AddField("uid", uid);
		form.AddField("data", stringData);
		form.AddField(KEY_SIGNATURE,  CreateSignature(uid, stringData, token));
		
		SendCommand("/game", form.data, OnLoginOrGamePlayActionsResponse, OnLoginResponseError);

		currentTimeMillisOnLogin = getCurrentTimeMillis();

		MessagingTrace_Add (node, false);
	}

	/**
	 * 
	 */
	public void SendGamePlayAction(string actionName, Dictionary<string,string> actionData = null, bool sendNow = false, bool isDebugAction = false)
	{
		JSONClass actionDataJObj = new JSONClass ();

		if (actionData != null)
		{
			foreach (string key in actionData.Keys)
			{
				actionDataJObj[key] = actionData[key];
			}
		}

		if (debug) Debug.Log (DEBUG_TAG + ">>> Action '" +  actionName + "' " + actionDataJObj.ToString());

		SendGamePlayActionInJSON (actionName, actionDataJObj, sendNow, isDebugAction);
	}

	/**
	 * 
	 */
	public void SendGamePlayActionInJSON(string actionName, JSONClass actionData = null, bool sendNow = false, bool isDebugAction = false)
	{
		if (!logged)
		{
			return;
		}

		if (actionData == null)
		{
			actionData = new JSONClass();
		}


		GamePlayActionsStack gamePlayAction;
		gamePlayAction.action = actionName;
		gamePlayAction.datas = actionData;
		gamePlayAction.seconds = (getCurrentTimeMillis() - currentTimeMillisOnLogin) / 1000;

		if (!isDebugAction)
		{
			// if no 'GamePlayActions' pending
			if (gamePlayActionsStack.Count == 0 && mCacheUpdatesMinTimer == 0)
			{
				mCacheUpdatesMinTimer = STACK_ACTIONS_MIN_TIME;
				mCacheUpdatesMaxTimer = STACK_ACTIONS_MAX_TIME;
			}
			else if (mCacheUpdatesMaxTimer > 0)
			{
				mCacheUpdatesMinTimer = Mathf.Max(mCacheUpdatesMinTimer, STACK_ACTIONS_MIN_TIME);
			}


			lock( locker )
			{
				gamePlayActionsStack.Add( gamePlayAction );
			}

			if (sendNow)
			{
				mForceSendAllNow = true;
			}
		}

		gamePlayActionsWithDebugStack.Add (gamePlayAction);
	}


	private void GamePlayActionsLogicUpdate()
	{
		int deltaTime = (int)(Time.deltaTime * 1000);
		
		// decrease counters to 'flush pending actions in stack' (MAXIMUM wait to flush)
		if (mCacheUpdatesMaxTimer > 0 && (mCacheUpdatesMaxTimer -= deltaTime) <= 0)
		{
			mCacheUpdatesMaxTimer = 0;
		}
		
		// decrease counters to 'flush pending actions in stack' (MINIMUM wait to flush)
		if (mCacheUpdatesMinTimer > 0 && (mCacheUpdatesMinTimer -= deltaTime) <= 0)
		{
			mCacheUpdatesMinTimer = 0;
			
			mForceSendAllNow = true;	// minimun time reached, so flush all actions currently in stack...
		}
		
		if (mForceSendAllNow)
		{
			mForceSendAllNow = false;
			
			FlushGamePlayActionsNow();

			if (gamePlayActionsStack.Count == 0)
			{
				mCacheUpdatesMinTimer = 0;
			} else {
				mCacheUpdatesMinTimer = Mathf.Max(mCacheUpdatesMinTimer, STACK_ACTIONS_MIN_TIME);
			}
		}
	}


	private void FlushGamePlayActionsNow()
	{
		if (gamePlayActionsStack.Count == 0)
		{
			return;
		}

		packetId++;

		int numActions = gamePlayActionsStack.Count;
		JSONNode json = GenerateActionList (packetId, actionId, gamePlayActionsStack);
		actionId += numActions;

//		Debug.Log ("###############: " + json.ToString());

		string stringData = json.ToString();
		// if (InstanceManager.Config.IsGameplayEncryptedEnabled())
		{
			stringData = simpleStringEncrypt(stringData);
		}

		WWWForm form = new WWWForm();
		form.AddField("uid", uid);
		form.AddField("data", stringData);
		form.AddField(KEY_SIGNATURE,  CreateSignature(uid, stringData, token));
		
		SendCommand ("/game", form.data, OnLoginOrGamePlayActionsResponse, OnGamePlayActionsResponseError);


		MessagingTrace_Add (GenerateActionList (packetId, actionId, gamePlayActionsWithDebugStack, true), false);


		// reset 'flush cache' timer
		if (gamePlayActionsStack.Count == 0)
		{
			mCacheUpdatesMinTimer = 0;
		} else {
			mCacheUpdatesMinTimer = Mathf.Max(mCacheUpdatesMinTimer, STACK_ACTIONS_MIN_TIME);
		}
	}

	private JSONNode GenerateActionList(int pid, int aid, List<GamePlayActionsStack> actionsStack, bool hasDebugActions = false)
	{
		JSONNode json = new JSONClass();
		json.Add("cmd", "actionList");
		
		JSONNode data = new JSONClass();
		data.Add("sid", new JSONData(sessionId));
		data.Add("pid", new JSONData(pid));
		// data.Add("checksum", "" + DefinitionsManager.Instance.getRulesCRC());
		
		JSONArray args = new JSONArray();
		
		for( int i = 0; i<actionsStack.Count; i++ )
		{
			GamePlayActionsStack gamePlayAction = actionsStack[i];
			JSONNode action = new JSONClass();

			if (!hasDebugActions || !gamePlayAction.action.StartsWith("debug/"))
			{
				action["aid"] = "" + aid;
				aid++;
			}

			action["action"] = gamePlayAction.action;
			action["args"] = gamePlayAction.datas;
			action["seconds"] = "" + gamePlayAction.seconds;

			args.Add( action );
		}
		actionsStack.Clear();
		
		data.Add("args", args);
		
		// json.Add("checksum", "" + DefinitionsManager.Instance.getRulesCRC());
		
		json.Add("data", data);
		

		return json;
	}


	/**
	 * 
	 */
	private void OnLoginOrGamePlayActionsResponse(string response, string cmd2, string responseStatus)
	{
		// if (response != null && InstanceManager.Config.IsGameplayEncryptedEnabled())
		{
			response = simpleStringDecrypt(response);
		}

		JSONNode json = JSON.Parse(response);

		if ( json != null )
		{
			MessagingTrace_Add (json, true);

			int responseCode = -1;
			if (json.AsObject.m_Dict.ContainsKey("response_code"))
			{
				responseCode = json["response_code"].AsInt;
			}

			// if successful response
			if (responseCode == 0)
			{
				JSONNode data = json["data"];
				string cmd = data["cmd"];
				if ("login".Equals(cmd))
				{
					string status = data["data"]["res"]["status"];
					if (status == "OK")
					{
						sessionId = data["data"]["sid"].AsInt;
						packetId = data["data"]["pid"].AsInt;
						actionId = 1;
						token = data["data"]["res"]["token"];
						
						logged = true;
					}
				}


				onGamePlayResponse (response, cmd2, responseStatus);


				if ("login".Equals(cmd) && logged)
				{
					/*
					if (InstanceManager.Instance.tempAssetsLUT != null)
					{
						InstanceManager.RequestNetwork.DebugAction("assetsLUT", InstanceManager.Instance.tempAssetsLUT);
					}
					
					if (InstanceManager.Instance.tempCustomizerJObj != null)
					{
						InstanceManager.RequestNetwork.DebugAction("customizer", InstanceManager.Instance.tempCustomizerJObj);
					}

					// If there has been any error downloading files then they have to be reported now
					ResourceLoader.Debug_Report();
					*/
				}

				/*
				if (InstanceManager.Config.AllowSendDebugActionsInWarningReports())
				{
					JSONNode dataX = data["data"];
					if (dataX != null)
					{
						if (!string.IsNullOrEmpty(dataX["warning"]) && bool.Parse(dataX["warning"]))
						{
							MessagingTrace_FlushToServer(200);
						}
					}
				}
				*/
			}
			else
			{
				Logout();
				/*				
				if (responseCode == 64 && InstanceManager.RequestNetwork.InSyncWithServerAfterAppPaused)	// SESSION_EXPIRED(64)
				{
					InstanceManager.FlowManager.GoToLoading();
				}
				else
				{
					// Notifies out of sync event
					InstanceManager.FlowManager.NotifyCriticalError(true, responseCode, json["error_msg"]);

					MessagingTrace_FlushToServer(responseCode, true);

					Debug.Log ("[OUT-OF-SYNC] (" + responseCode + ")" + json["error_msg"]);
				}
				*/
			}
		}
	}

	/**
	 * 
	 */
	private void OnLoginResponseError(int errorCode, string errorDesc, string response, string cmd)
	{
		// InstanceManager.FlowManager.NotifyCriticalError(false, errorCode, null);
	}

	/**
	 * 
	 */
	private void OnGamePlayActionsResponseError(int errorCode, string errorDesc, string response, string cmd)
	{
		Logout();
		// InstanceManager.FlowManager.NotifyCriticalError(false, errorCode, null);
	}



	/**
	 * 
	 */
	public void FlushAllGamePlayActions()
	{
		if (IsLogged() && gamePlayActionsStack.Count > 0)
		{
			FlushGamePlayActionsNow();

			CommandsLogicUpdate(true);
		}
	}


	// ----------------------------------------------------------------------- //

	// ----------------------------------------------------------------------- //

	// GamePlayActions To send
	private List<GamePlayActionsStack> gamePlayActionsStack = new List<GamePlayActionsStack>();
	private List<GamePlayActionsStack> gamePlayActionsWithDebugStack = new List<GamePlayActionsStack>();

	struct GamePlayActionsStack
	{
		public string action;
		public JSONNode datas;
		public long seconds;
	};

	// ----------------------------------------------------------------------- //

	private const string KEY_SIGNATURE = "sig";
	private const string ENCRYPT_PASSWORD = "sirocco";

	/**
	 * Creates a signature for the command
	 */
	private string CreateSignature(string uid, string data,string token)
	{
		string str = "data=" + data + "&uid=" + uid + token + ENCRYPT_PASSWORD;
		
		MD5 md5 = new MD5CryptoServiceProvider();
		byte[] bytes = Encoding.UTF8.GetBytes(str);
		md5.ComputeHash(bytes);
		bytes = md5.Hash;
		
		StringBuilder sb = new StringBuilder(bytes.Length * 2);
		foreach (byte b in bytes)
		{
			sb.AppendFormat("{0:x2}", b);
		}
		return sb.ToString();
	}

	/**
	 * Creates the data for a command
	 */
	private JSONNode CreateCommandArgs( Dictionary<string,string> data )
	{
		JSONClass node = new JSONClass();
		if (data != null)
		{
			foreach( KeyValuePair<string,string> pair in data )
			{
				node.Add( pair.Key, pair.Value);
			}
		}
		return node;
	}


	/**
     * Simple String encrypt (only charCodes from 32 to 127)
     */
	private string simpleStringEncrypt(string input)
	{
		char[] charText = input.ToCharArray();
		
		for (int i = 0; i < charText.Length; i++) {
			char letter = charText[i];
			
			if (letter >= 0x20 && letter < 0x80) {
				int lowBits = (letter ^ (i + 3)) & 0x1f;
				
				charText[i] = (char) ((letter & 0xffffffe0) | lowBits);
			}
		}
		
		return new string(charText);
	}
	
	/**
     * Simple String decrypt (only charCodes from 32 to 127)
     */
	private string simpleStringDecrypt(string input)
	{
		// Right now, encryption is bidirectional
		return simpleStringEncrypt(input);
	}

	// ----------------------------------------------------------------------- //

	private bool useGZipCompression = true;

	private int messagingTraceIndex = 0;
	private JSONArray messagingTraceAry = new JSONArray();

	private void MessagingTrace_Add(JSONNode messageObj, bool isResponseFromServer)
	{
	/*
		if (InstanceManager.Config.UseMessagingTracking())
		{
			JSONNode traceObj = new JSONClass ();

			traceObj ["timestamp"] = "" + getCurrentTimeMillis ();
			traceObj ["body"] = messageObj;
			traceObj ["service"] = "game";
			traceObj ["type"] = isResponseFromServer? "server":"client";

			if (isResponseFromServer)
			{
				traceObj ["code"] = "200";
			}

			messagingTraceAry.Add (traceObj);
		}
		*/
	}


	public void MessagingTrace_FlushToServer(int responseCode, bool sendFullReport = false)
	{
		/*
		if (InstanceManager.Config.UseMessagingTracking())
		{
			if (sendFullReport)
			{
				messagingTraceIndex = 0;
			}

			JSONArray lastMessagingTraceAry = new JSONArray();
			for (int i=messagingTraceIndex ; i<messagingTraceAry.Count ; i++)
			{
				lastMessagingTraceAry.Add(messagingTraceAry[i]);
			}
			messagingTraceIndex = messagingTraceAry.Count;


			if (debug) Debug.Log (lastMessagingTraceAry.ToString());

			Dictionary<string, string> urlParams = new Dictionary<string, string> ();
			urlParams ["uid"] = uid;
			urlParams ["code"] = "" + responseCode;

			byte[] body = Encoding.UTF8.GetBytes(lastMessagingTraceAry.ToString());

			if (useGZipCompression)
			{
				try
				{
					body = MyUtils.Zip(body);
				}
				catch
				{
					useGZipCompression = false;
				}
			}

			Dictionary<string, string> headerParams = null;
			headerParams = new Dictionary<string, string>();
			headerParams["Content-Type"] = "application/json";
			if (useGZipCompression)
			{
				headerParams["Content-Encoding"] = "gzip";
			}
			
			SendCommand ("/api/report/send", urlParams, headerParams, body, MessagingTrace_OnReportResponse, MessagingTrace_OnReportResponseError);

			if (sendFullReport)
			{
				messagingTraceAry = new JSONArray();
				messagingTraceIndex = 0;
			}
		}
		*/
	}

	private void MessagingTrace_OnReportResponse(string response, string cmd2, string responseStatus)
	{
		if (debug)
		{
			Debug.Log ("################# MessagingTrace OK");
			Debug.Log (responseStatus + " :: " + response);
		}
	}
	
	private void MessagingTrace_OnReportResponseError(int errorCode, string errorDesc, string response, string cmd)
	{
		if (debug)
		{
			Debug.Log ("################# MessagingTrace ERROR");
			Debug.Log (errorCode + " :: " + errorDesc);
			Debug.Log (cmd + " :: " + response);
		}
	}

	// ----------------------------------------------------------------------- //

	public void MemoryTrace_SendInfoToServer(JSONClass memoryTraceJObj)
	{
		if (debug) Debug.Log (memoryTraceJObj.ToString());
		
		Dictionary<string, string> urlParams = new Dictionary<string, string> ();
		urlParams ["uid"] = uid;
		urlParams ["code"] = "" + "200";
	
		byte[] body = Encoding.UTF8.GetBytes(memoryTraceJObj.ToString());

		SendCommand ("/api/memprint/send", urlParams, body, MessagingTrace_OnReportResponse, MessagingTrace_OnReportResponseError);
	}

	// ----------------------------------------------------------------------- //

	private List<float> latencyList = new List<float>();

	private void Latency_AddEntry(float duration)
	{
		latencyList.Add (duration);

		while (latencyList.Count > 10)
		{
			latencyList.RemoveAt(0);
		}
	}

	public float Latency_GetAverageDuration()
	{
		float averageDuration = 0;

		for (int i=0 ; i<latencyList.Count ; i++)
		{
			averageDuration += latencyList[i];
		}

		return latencyList.Count > 0 ? averageDuration / latencyList.Count : 0;
	}

	public float Latency_GetLastEntryDuration()
	{
		if (latencyList.Count > 0)
		{
			return latencyList[latencyList.Count - 1];
		}
		
		return 0;
	}

	// ----------------------------------------------------------------------- //

	private static readonly System.DateTime Jan1st1970 = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
	
	private long getCurrentTimeMillis()
	{
		return (long) (System.DateTime.UtcNow - Jan1st1970).TotalMilliseconds;
	}

	// ----------------------------------------------------------------------- //

}
