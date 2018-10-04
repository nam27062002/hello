﻿/// <summary>
/// This class is responsible for offering the interface for all stuff related to server. This interface satisfies the requirements of the code taken from HSX in order to make the integration
/// easier. This class also hides its implementation, so we could have different implementations for this class and we could decide in the implementation of the method <c>SharedInstance</c> 
/// which one to use. 
/// </summary>

using FGOL.Server;
using System;
using System.Text;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
public class GameServerManager
{
	#region singleton
	//------------------------------------------------------------------------//
	// SINGLETON IMPLEMENTATION												  //
	//------------------------------------------------------------------------//
    private static GameServerManager s_pInstance = null;
    public static GameServerManager SharedInstance
    {
        get
        {
            if (s_pInstance == null)
            {
				// Test mode?
				if (DebugSettings.useDebugServer) {
					// Offline implementation is used
					s_pInstance = new GameServerManagerOffline();
				} else {
					// Calety implementation is used
					s_pInstance = new GameServerManagerCalety();
				}

				// Configure new instance
				s_pInstance.Configure();
            }

            return s_pInstance;
        }
    }
    #endregion

	//------------------------------------------------------------------------//
	// AUX CLASSES															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Generic server response object.
	/// </summary>
	public class ServerResponse : Dictionary<string, object> {
		/// <summary>
		/// Override for a nice string output.
		/// </summary>
		override public string ToString() {
			// Special case if empty
			int remaining = this.Count;
			if(remaining == 0) return "{ }";

			// Json-like formatting
			StringBuilder sb = new StringBuilder();
			sb.AppendLine("{");
			foreach(KeyValuePair<string, object> kvp in this) {
				// Add entry
				sb.Append("    \"").Append(kvp.Key).Append("\" : ");

				// Special case for strings, surraund with quotation marks
				if(kvp.Value.GetType() == typeof(string)) {
					sb.Append("\"").Append(kvp.Value.ToString()).Append("\"");
				} else {
					sb.Append(kvp.Value.ToString());
				}
				remaining--;

				// If not last one, add separator
				if(remaining > 0) sb.Append(",");

				// New line
				sb.AppendLine();
			}
			sb.AppendLine("}");

			return sb.ToString();
		}
	}

	/// <summary>
	/// Server callback signature.
	/// </summary>
	/// <param name="_error">Error returned by the server. <c>null</c> if there was no error.</param>
	/// <param name="_response">Response data sent by the server. Can be <c>null</c> if the command has no response (i.e. ping).</param>
	public delegate void ServerCallback(Error _error, ServerResponse _response);

	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Make sure that JSON dataused to communicate with the server is properly formatted!
	public static readonly CultureInfo JSON_FORMAT = CultureInfo.InvariantCulture;

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
    private bool m_configured = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

    public virtual void Destroy() {}

	/// <summary>
	/// 
	/// </summary>
    public void Configure()
    {
		if (!m_configured)
        {
			// Init some common stuff
			// Initialize server time with current local time
			ServerManager.SharedInstance.SetServerTime((double)TimeUtils.DateToTimestamp(DateTime.UtcNow) / 1000.0f);

			// Let heirs do their stuff
			ExtendedConfigure();

			// Mark as configured!
            m_configured = true;
        }
    }

    public virtual void Reset() {}

    /// <summary>
    /// 
    /// </summary>
    public void CheckConnection(Action<Error> callback)
	{
        InternalCheckConnection(callback, false);
    }

    protected void InternalCheckConnection(Action<Error> callback, bool highPriority = false)
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("GameServerManager (CheckConnection) :: InternetReachability NotReachable");
            callback(new ClientConnectionError("InternetReachability NotReachable", ErrorCodes.ClientConnectionError));
        }
        else
        {
            InternalPing((Error _error, GameServerManager.ServerResponse _response) =>
            {
                if (callback != null)
                {
                    callback(_error);
                }
            }, highPriority);
        }
    }

	public void OnConnectionLost()
    {
        InternalOnConnectionLost();
        HDCustomizerManager.instance.NotifyServerDown();
    }

    protected virtual void InternalOnConnectionLost() {}

    public virtual void Connection_SetIsCheckEnabled(bool value) {}

    //------------------------------------------------------------------------//
    // GENERIC SERVER MANAGEMENT											  //
    //------------------------------------------------------------------------//
    protected virtual void ExtendedConfigure() {}    
	public void Ping(ServerCallback callback)
    {
        InternalPing(callback, false);
    }

    protected virtual void InternalPing(ServerCallback callback, bool highPriority=false) { }
  
	public virtual void SendLog(string message, string stackTrace, UnityEngine.LogType logType) {}

	//------------------------------------------------------------------------//
	// SERVER TIME															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the current timestamp directly from the server.
	/// </summary>
	/// <param name="callback">Callback.</param>
	public virtual void GetServerTime(ServerCallback callback) {}

	/// <summary>
	/// Get an estimation of the current server time using the last known server time.
	/// No request will be done to the server.
	/// </summary>
	/// <returns>The estimated server time.</returns>
	public DateTime GetEstimatedServerTime() {
        // Calety already manages this, just convert it to a nice DateTime object.		
        long timestamp = GetEstimatedServerTimeAsLong();
        return TimeUtils.TimestampToDate( timestamp );
	}

    /// <summary>
    /// Get an estimation of the current server time in milliseconds by using the last known server time
    /// </summary>
    /// <returns>The estimated server time in milliseconds</returns>
    public long GetEstimatedServerTimeAsLong() {
        double unixTimestamp = ServerManager.SharedInstance.GetServerTime();    // Seconds since 1970
        return (long)unixTimestamp * 1000;
    }

    public virtual void OnGameActionProcessed(string cmd, SimpleJSON.JSONNode response) {}
    public virtual void OnGameActionFailed(string cmd, int errorCode) {}

    public Error GetLogicServerInternalError(int errorCode=-1)
    {
        return new ServerInternalError(null, null, ErrorCodes.LogicError, errorCode);
    }

    //------------------------------------------------------------------------//
    // LOGIN																  //
    //------------------------------------------------------------------------//
    public void Auth(ServerCallback callback)
    {
        InternalAuth(callback, false);
    }

    protected virtual void InternalAuth(ServerCallback callback, bool highPriority=false) { }
    public virtual void LogOut() {}
    public virtual bool IsLoggedIn() { return false; }
    public virtual void OnLogOut() {}    

    //------------------------------------------------------------------------//
    // CUSTOMIZER															  //
    //------------------------------------------------------------------------//

    //------------------------------------------------------------------------//
    // PERSISTENCE															  //
    //------------------------------------------------------------------------//
    public virtual void GetPersistence(ServerCallback callback) {}
    public virtual void SetPersistence(string persistence, ServerCallback callback) {}
	public virtual void UpdateSaveVersion(bool prelimUpdate, ServerCallback callback) {}
    
	//------------------------------------------------------------------------//
	// QUALITY SETTINGS														  //
	//------------------------------------------------------------------------//
	public virtual void GetQualitySettings(ServerCallback callback) {}
    public virtual void SetQualitySettings(string qualitySettings, ServerCallback callback) {}
    public virtual void GetGameSettings(ServerCallback callback) {}

    //------------------------------------------------------------------------//
    // OTHERS																  //
    //------------------------------------------------------------------------//
    public virtual void SendPlayTest(bool silent, string playTestUserId, string trackingData, ServerCallback callback) {}
    public virtual void SendTrackLoading(string step, int deltaTime, bool isFirstTime, int sessionsCount, ServerCallback callback) {}
    public virtual void GetPendingTransactions(ServerCallback callback) {}
    public void ConfirmPendingTransaction(Transaction transaction, ServerCallback callback)
    {        
        if (transaction == null)
        {
            ConfirmPendingTransactions(null, callback);
        }
        else
        {
            List<Transaction> transactions = new List<Transaction>();
            transactions.Add(transaction);
            DoConfirmPendingTransactions(transactions, callback);
        }
    }    

    public void ConfirmPendingTransactions(List<Transaction> transactions, ServerCallback callback)
    {
        if (transactions == null || transactions.Count == 0)
        {            
            callback(GetLogicServerInternalError(), null);
        }
        else
        {
            DoConfirmPendingTransactions(transactions, callback);
        }
    }

    protected virtual void DoConfirmPendingTransactions(List<Transaction> transactions, ServerCallback callback) {}

    protected ServerResponse GetConfirmPendingTransactionsResponse(List<Transaction> transactions, int errorCode=-1)
    {
        // JSON response       
        SimpleJSON.JSONNode json = GetPendingTransactionsJSON(transactions);
        if (errorCode > -1)
        {
            json["result"] = "false";
            json["code"] = errorCode;
        }

        ServerResponse res = new ServerResponse();
        res["response"] = json.ToString();                

        return res;
    }

    protected SimpleJSON.JSONNode GetPendingTransactionsJSON(List<Transaction> transactions)
    {
        // JSON response
        SimpleJSON.JSONArray array = new SimpleJSON.JSONArray();

        if (transactions != null)
        {
            Transaction _transaction;
            int count = transactions.Count;
            for (int i = 0; i < count; i++)
            {
                _transaction = transactions[i];
                if (_transaction != null)
                {
                    array.Add(null, _transaction.ToJSON());
                }
            }
        }

        SimpleJSON.JSONNode json = new SimpleJSON.JSONClass();
        json["txs"] = array;

        return json;
    }

    public virtual void SetLanguage(string serverCode, ServerCallback onDone) {}
    
    public virtual void PCSpent(int amount, string group, ServerCallback onDone){}

    public virtual void PCEarned(int amount, string group, bool paid, ServerCallback onDone){}

    //------------------------------------------------------------------------//
    // GLOBAL EVENTS														  //
    //------------------------------------------------------------------------//
    public virtual void GlobalEvent_TMPCustomizer(ServerCallback _callback) {}

	/// <summary>
	/// Get an event for this user from the server.
	/// </summary>
	/// <param name="_eventID">The identifier of the target event.</param>
	/// <param name="_callback">Callback action.</param>
	public virtual void GlobalEvent_GetEvent(int _eventID, ServerCallback _callback) {}

	/// <summary>
	/// Get the current value and (optionally) the leaderboard for a specific event.
	/// </summary>
	/// <param name="_eventID">The identifier of the event whose state we want.</param>
	/// <param name="_callback">Callback action.</param>
	public virtual void GlobalEvent_GetState(int _eventID, ServerCallback _callback) {}

	/// <summary>
	/// Register a score to a target event.
	/// </summary>
	/// <param name="_eventID">The identifier of the target event.</param>
	/// <param name="_score">The score to be registered.</param>
	/// <param name="_callback">Callback action.</param>
	public virtual void GlobalEvent_RegisterScore(int _eventID, int _score, ServerCallback _callback) {}

	/// <summary>
	/// Get the rewards for the player
	/// </summary>
	/// <param name="_eventID">The identifier of the target event.</param>
	/// <param name="_callback">Callback action.</param>
	public virtual void GlobalEvent_GetRewards(int _eventID, ServerCallback _callback) {}

	/// <summary>
	/// Get leaderboard
	/// </summary>
	/// <param name="_eventID">The identifier of the target event.</param>
	/// <param name="_callback">Callback action</param>
	public virtual void GlobalEvent_GetLeaderboard(int _eventID, ServerCallback _callback) {}


	//------------------------------------------------------------------------//
    // HD Live EVENTS														  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// HDs the events get my events. Gets the list of events assigned to this user
    /// </summary>
    /// <param name="_callback">Callback.</param>
	public virtual void HDEvents_GetMyEvents(ServerCallback _callback) {}

	/// <summary>
	/// HDs the events get event. Get the definition of the event
	/// </summary>
	/// <param name="_eventID">Event I.</param>
	/// <param name="_callback">Callback.</param>
	public virtual void HDEvents_GetDefinition(int _eventID, ServerCallback _callback) {}

	/// <summary>
	/// HDs the events get my event progess. Gets the progress in the event for the player
	/// </summary>
	/// <param name="_eventID">Event I.</param>
	/// <param name="_callback">Callback.</param>
	public virtual void HDEvents_GetMyProgess(int _eventID, ServerCallback _callback) {}
    public virtual void HDEvents_AddProgress(int _eventID, int _score, ServerCallback _callback) {}


    public virtual void HDEvents_GetLeaderboard(int _eventID, ServerCallback _callback) { }
    public virtual void HDEvents_SetScore(int _eventID, int _score, SimpleJSON.JSONNode _build, ServerCallback _callback) { }
	public virtual void HDEvents_EnterEvent(int _eventID, string _type, long _amount, int _matchmakingValue, ServerCallback _callback) {}


	public virtual void HDEvents_GetMyReward(int _eventID, ServerCallback _callback) {}
	public virtual void HDEvents_FinishMyEvent(int _eventID, ServerCallback _callback) {}

	public virtual void HDEvents_GetRefund(int _eventID, ServerCallback _callback) {}

	//------------------------------------------------------------------------//
	// DEBUG ONLY															  //
	//------------------------------------------------------------------------//	
	/// <summary>
	/// Update frame.
	/// </summary>
	public virtual void Update() {
		;	// Put a breakpoint in here to peek what the GameServerManager is doing
	}	
}
