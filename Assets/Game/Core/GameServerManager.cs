/// <summary>
/// This class is responsible for offering the interface for all stuff related to server. This interface satisfies the requirements of the code taken from HSX in order to make the integration
/// easier. This class also hides its implementation, so we could have different implementations for this class and we could decide in the implementation of the method <c>SharedInstance</c> 
/// which one to use. 
/// </summary>

using FGOL.Server;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
public class GameServerManager
{
	// Make sure that JSON dataused to communicate with the server is properly formatted!
	public static readonly CultureInfo JSON_FORMAT = CultureInfo.InvariantCulture;

    #region singleton
    // Singleton ///////////////////////////////////////////////////////////
    private static GameServerManager s_pInstance = null;

    public static GameServerManager SharedInstance
    {
        get
        {
            if (s_pInstance == null)
            {
				// Test mode?
				if(DebugSettings.useDebugServer) {
					// Offline implementation is used
					s_pInstance = new GameServerOffline();
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

    private bool m_configured = false;

    public void Configure()
    {
		if (!m_configured)
        {
            
			ExtendedConfigure();
            m_configured = true;            
        }
    }

    protected virtual void ExtendedConfigure() {}
    public virtual void Init(GeoLocation.Location location) {}
    public virtual void SetServerLocation(GeoLocation.Location location) {}

    public virtual void Ping(Action<Error> callback) {}
    public virtual void LogInToServerThruPlatform(string platformId, string platformUserId, string platformToken, Action<Error, Dictionary<string, object>> callback) {}
    public virtual void LogOut(Action<Error> callback) {}

    public void CheckConnection(System.Action<FGOL.Server.Error> callback)
    {
        if (Application.internetReachability != NetworkReachability.NotReachable)
        {
            if (callback != null)
            {
                callback(null);
            }
        }
        else
        {
            Debug.Log("HSXServer (CheckConnection) :: InternetReachability NotReachable");
            callback(new FGOL.Server.ClientConnectionError("InternetReachability NotReachable", FGOL.Server.ErrorCodes.ClientConnectionError));
        }
    }

    public virtual void GetServerTime(Action<Error, Dictionary<string, object>> callback) {}
    public virtual void GetPersistence(Action<Error, Dictionary<string, object>> callback) {}
    public virtual void SetPersistence(string persistence, Action<Error, Dictionary<string, object>> callback) {}
    public virtual void SendLog(string message, string stackTrace, UnityEngine.LogType logType) {}
    public virtual void UpdateSaveVersion(bool prelimUpdate, Action<Error, Dictionary<string, object>> callback) {}
    public virtual void GetQualitySettings(Action<Error, Dictionary<string, object>> callback) {}
    public virtual void SetQualitySettings(string qualitySettings, Action<Error, Dictionary<string, object>> callback) {}
    public virtual void SendPlayTest(bool silent, string playTestUserId, string trackingData, Action<Error, Dictionary<string, object>> callback) {}    

	//------------------------------------------------------------------------//
	// GLOBAL EVENTS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the current global event for this user from the server.
	/// Current global event can be a past event with pending rewards, an active event,
	/// a future event or no event at all.
	/// </summary>
	/// <param name="_callback">Callback action.</param>
	public virtual void GlobalEvent_GetCurrent(Action<Error, Dictionary<string, object>> _callback) {}

	/// <summary>
	/// Get the current value and (optionally) the leaderboard for a specific event.
	/// </summary>
	/// <param name="_eventID">The identifier of the event whose state we want.</param>
	/// <param name="_getLeaderboard">Whether to retrieve the leaderboard as well or not (top 100 + player).</param>
	/// <param name="_callback">Callback action.</param>
	public virtual void GlobalEvent_GetState(string _eventID, bool _getLeaderboard, Action<Error, Dictionary<string, object>> _callback) {}

	/// <summary>
	/// Register a score to a target event.
	/// </summary>
	/// <param name="_eventID">The identifier of the target event.</param>
	/// <param name="_score">The score to be registered.</param>
	/// <param name="_callback">Callback action.</param>
	public virtual void GlobalEvent_RegisterScore(string _eventID, float _score, Action<Error> _callback) {}

	/// <summary>
	/// Reward the player for his contribution to an event.
	/// </summary>
	/// <param name="_eventID">The identifier of the target event.</param>
	/// <param name="_callback">Callback action. Given rewards?</param>
	public virtual void GlobalEvent_ApplyRewards(string _eventID, Action<Error, Dictionary<string, object>> _callback) {}
}
