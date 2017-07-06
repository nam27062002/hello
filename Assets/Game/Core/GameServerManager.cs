/// <summary>
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
				if(DebugSettings.useDebugServer) {
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
	/// <summary>
	/// 
	/// </summary>
    public void Configure()
    {
		if (!m_configured)
        {
			// Init some common stuff
			// Initialize server time with current local time
			ServerManager.SharedInstance.SetServerTime((double)Globals.GetUnixTimestamp());

			// Let heirs do their stuff
			ExtendedConfigure();

			// Mark as configured!
            m_configured = true;
        }
    }

	/// <summary>
	/// 
	/// </summary>
	public void CheckConnection(ServerCallback callback)
	{
		if (Application.internetReachability != NetworkReachability.NotReachable)
		{
			if (callback != null)
			{
				callback(null, null);
			}
		}
		else
		{
			Debug.Log("HSXServer (CheckConnection) :: InternetReachability NotReachable");
			callback(new FGOL.Server.ClientConnectionError("InternetReachability NotReachable", FGOL.Server.ErrorCodes.ClientConnectionError), null);
		}
	}

	//------------------------------------------------------------------------//
	// GENERIC SERVER MANAGEMENT											  //
	//------------------------------------------------------------------------//
    protected virtual void ExtendedConfigure() {}
    public virtual void Init(GeoLocation.Location location) {}
	public virtual void Ping(ServerCallback callback) {}
	public virtual void SetServerLocation(GeoLocation.Location location) {}
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
		double unixTimestamp = ServerManager.SharedInstance.GetServerTime();	// Seconds since 1970
		return Globals.GetDateFromUnixTimestamp((long)unixTimestamp);
	}

	//------------------------------------------------------------------------//
	// LOGIN																  //
	//------------------------------------------------------------------------//
    public virtual void LogInToServerThruPlatform(string platformId, string platformUserId, string platformToken, ServerCallback callback) {}
	public virtual void LogOut(ServerCallback callback) {}

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
    
	//------------------------------------------------------------------------//
	// OTHERS																  //
	//------------------------------------------------------------------------//
	public virtual void SendPlayTest(bool silent, string playTestUserId, string trackingData, ServerCallback callback) {}    

	//------------------------------------------------------------------------//
	// GLOBAL EVENTS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the current global event for this user from the server.
	/// Current global event can be a past event with pending rewards, an active event,
	/// a future event or no event at all.
	/// </summary>
	/// <param name="_callback">Callback action.</param>
	public virtual void GlobalEvent_GetCurrent(ServerCallback _callback) {}

	/// <summary>
	/// Get the current value and (optionally) the leaderboard for a specific event.
	/// </summary>
	/// <param name="_eventID">The identifier of the event whose state we want.</param>
	/// <param name="_getLeaderboard">Whether to retrieve the leaderboard as well or not (top 100 + player).</param>
	/// <param name="_callback">Callback action.</param>
	public virtual void GlobalEvent_GetState(int _eventID, bool _getLeaderboard, ServerCallback _callback) {}

	/// <summary>
	/// Register a score to a target event.
	/// </summary>
	/// <param name="_eventID">The identifier of the target event.</param>
	/// <param name="_score">The score to be registered.</param>
	/// <param name="_callback">Callback action.</param>
	public virtual void GlobalEvent_RegisterScore(int _eventID, float _score, ServerCallback _callback) {}

	/// <summary>
	/// Reward the player for his contribution to an event.
	/// </summary>
	/// <param name="_eventID">The identifier of the target event.</param>
	/// <param name="_callback">Callback action. Given rewards?</param>
	public virtual void GlobalEvent_ApplyRewards(int _eventID, ServerCallback _callback) {}
}
