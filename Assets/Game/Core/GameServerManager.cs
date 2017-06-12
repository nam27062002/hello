/// <summary>
/// This class is responsible for offering the interface for all stuff related to server. This interface satisfies the requirements of the code taken from HSX in order to make the integration
/// easier. This class also hides its implementation, so we could have different implementations for this class and we could decide in the implementation of the method <c>SharedInstance</c> 
/// which one to use. 
/// </summary>

using FGOL.Server;
using System;
using System.Collections.Generic;
using UnityEngine;
public class GameServerManager
{
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
	/// Get a list of all current global events.
	/// </summary>
	/// <param name="_callback">Callback action.</param>
	public virtual void GetGlobalEvents(Action<Error, Dictionary<string, object>> _callback) {}
}
