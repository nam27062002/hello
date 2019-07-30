// CPServer.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 18/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using System.Collections;
using TMPro;
using Fabric.Crashlytics;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// All cheats/shortcuts related to server.
/// </summary>
public class CPServerTab : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private ScrollRect m_outputScroll = null;
	[SerializeField] private TextMeshProUGUI m_outputText = null;
	[SerializeField] private TextMeshProUGUI m_accountIdText = null;
	[SerializeField] private TextMeshProUGUI m_enviromentText = null;
	[SerializeField] private TextMeshProUGUI m_trackingIdText = null;
	[SerializeField] private TextMeshProUGUI m_DNAProfileIdText = null;
    [SerializeField] private TextMeshProUGUI m_AdUnitInfoText = null;
    [SerializeField] private Toggle m_debugServerToggle = null;    

    // Internal
    private DateTime m_startTimestamp;
	private StringBuilder m_outputSb = new StringBuilder();

	//private RequestNetwork requestNetwork;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization
	/// </summary>
	private void Awake() {
		// Check required refs
		Debug.Assert(m_outputScroll != null, "Required field missing!");
		Debug.Assert(m_outputText != null, "Required field missing!");

		// Clear console upon awakening
		m_startTimestamp = DateTime.UtcNow;
		OnClearConsoleButton();

		//RequestNetworkOnline.CreateInstance();
		//requestNetwork = new RequestNetworkOnline();

	}

	private void OnEnable()
	{
		m_accountIdText.text = "AccountId: " + GameSessionManager.SharedInstance.GetUID();
		m_enviromentText.text = "Env: " + ServerManager.SharedInstance.GetServerConfig().m_eBuildEnvironment.ToString();

		m_trackingIdText.text = "TrackingId: " + HDTrackingManager.Instance.GetTrackingID();
		m_DNAProfileIdText.text = "DNA profileId: " + HDTrackingManager.Instance.GetDNAProfileID();
        m_AdUnitInfoText.text = "Ads: " + GameAds.instance.GetInfo();

        m_debugServerToggle.isOn = DebugSettings.useDebugServer;
		m_debugServerToggle.onValueChanged.AddListener(OnToggleDebugServer);        
    }

	private void OnDisable() {
		m_debugServerToggle.onValueChanged.RemoveListener(OnToggleDebugServer);
	}

	private void Update()
	{
		//requestNetwork.Update();
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Given an InputField game object, extract the text from it.
	/// </summary>
	/// <returns>The text introduced in the given InputField.</returns>
	/// <param name="_inputField">The game object containing the target InputField.</param>
	private string GetInputText(GameObject _inputField) {
		TMP_InputField inputField = _inputField.FindComponentRecursive<TMP_InputField>();
		if(inputField != null) {
			return inputField.text;
		}
		return "";
	}

	/// <summary>
	/// Add a new line into the output console.
	/// </summary>
	/// <param name="_text">The text to be output.</param>
	private void Output(string _text) {
		// Add new line and timestamp
		if(m_outputSb.Length > 0) m_outputSb.AppendLine();	// Don't add new line for the very first line
		TimeSpan t = DateTime.UtcNow.Subtract(m_startTimestamp);
		m_outputSb.AppendFormat("<color={4}>{0:D2}:{1:D2}:{2:D2}.{3:D2}", t.Hours, t.Minutes, t.Seconds, t.Milliseconds, Colors.WithAlpha(Colors.white, 0.25f).ToHexString("#"));	// [AOC] Unfortunately, this version of Mono still doesn't support TimeSpan formatting (added at .Net 4)
		m_outputSb.Append(": </color>");

		// Add text
		m_outputSb.Append(_text);

		// Set text
		m_outputText.text = m_outputSb.ToString();

		// Update scroll
		// Don't reset scroll if the scroll position was manually set (different than 0)
		if(m_outputScroll.verticalNormalizedPosition < 0.01f) {	// Error margin
			StartCoroutine(ResetScrollPos());
		}

		// Output to console as well
		Debug.Log(_text);
	}

	/// <summary>
	/// Reset scroll position with a small delay.
	/// We need to do it delayed since the layout is not updated until the next frame.
	/// </summary>
	private IEnumerator ResetScrollPos() {
		//yield return new WaitForSeconds(0.1f);
		yield return new WaitForEndOfFrame();
		m_outputScroll.normalizedPosition = Vector2.zero;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Toggle debug server
	/// </summary>
	public void OnToggleDebugServer(bool _toggle) {
		DebugSettings.useDebugServer = _toggle;
	}

	/// <summary>
	/// Generic button callback.
	/// </summary>
	public void OnButton1(GameObject _input) {
		// Get optional parameters
		string paramString = GetInputText(_input);

		// Do stuff!
		Debug.Log("Button 1 pressed with params " + paramString);
		Output("Button 1 pressed with params " + paramString);

		// Request current event info
		Output("GlobalEvent_GetCurrent");
		GameServerManager.SharedInstance.GlobalEvent_GetEvent(0,
			(FGOL.Server.Error _error, GameServerManager.ServerResponse _response) => {
				if(_error == null) {
					// Did the server gave us an event?
					if(_response != null && _response["response"] != null) {
						// If the ID is different from the stored event, load the new event's data!
						//SimpleJSON.JSONNode responseJson = SimpleJSON.JSONNode.Parse(_response["response"]);
						string jsonString = _response["response"] as string;
						JsonFormatter fmt = new JsonFormatter();
						jsonString = fmt.PrettyPrint(jsonString);
						Output(jsonString);
					} else {
						Output("<color=red>ERROR!</color> Invalid response but no error was given");
					}
				} else {
					Output("<color=red>ERROR!</color> " + _error.ToString());
				}
			}
		);
	}

	/// <summary>
	/// Generic button callback.
	/// </summary>
	public void OnButton2(GameObject _input) {
		// Get optional parameters
		string paramString = GetInputText(_input);

		// Forces a crash
		IDragonData data = null;
		data.ToString();

		// Do stuff!		
		Output("Button 2 pressed with params " + paramString + " TO FORCE A CRASH");
	}

	/// <summary>
	/// Generic button callback.
	/// </summary>
	public void OnButton3(GameObject _input) {
		// Get optional parameters
		string paramString = GetInputText(_input);

        // Do stuff!		
        HDCustomizerManager.instance.CheckAndApply();
	}

	/// <summary>
	/// Generic button callback.
	/// </summary>
	public void OnButton4(GameObject _input) {
		// Get optional parameters
		//string paramString = GetInputText(_input);

		// Do stuff!
		//Debug.Log("Button 4 pressed GameAds.ShowDebugInfo");
		//GameAds.instance.ShowDebugInfo();
		GameAds.instance.ShowRewarded(GameAds.EAdPurpose.REVIVE, OnAdDone);
	}
    
    private void OnAdDone(bool success)
    {
        string msg = "OnAdDone success = " + success;
        Output(msg);
    }
    
    public void OnButton5()
    {
        GameAds.instance.ShowInterstitial(OnIntersitialDone);
    }

    public void OnButton6()
    {
        //HDCP2Manager.Instance.PlayInterstitial(false, OnIntersitialDone);        
        PopupAdBlocker.LaunchCp2Interstitial(OnCp2IntersitialDone);
    }

    private void OnCp2IntersitialDone(bool success)
    {
        string msg = "OnCp2IntersitialDone success = " + success;
        Output(msg);
    }

    public void OnDebugCP2()
    {
        FeatureSettingsManager manager = FeatureSettingsManager.instance;
        Output("CP2Enabled = " + manager.IsCP2Enabled() + " " + HDCP2Manager.Instance.GetDebugInfo());
    }

    public void OnCrashlyticsNonFatal()
    {
        Output("Crashlytics: non-fatal exception");
        Crashlytics.ThrowNonFatal();
    }

    public void OnCrashlyticsCrash()
    {
        Output("Crashlytics: crash");
        Crashlytics.Crash();
    }

    private void OnIntersitialDone( bool success )
    {
        string msg = "OnInterstitialDone success = " + success;
        Output(msg);
    }

	/// <summary>
	/// Clear console button has been pressed.
	/// </summary>
	public void OnClearConsoleButton() {
		m_outputSb.Length = 0;
		Output("Hungry Dragon v" + GameSettings.internalVersion + " console output");
	}
}