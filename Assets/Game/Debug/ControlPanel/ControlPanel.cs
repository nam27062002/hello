// ControlPanel.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#if DEBUG && !DISABLE_LOGS
#define ENABLE_LOGS
#endif

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// In-game control panel for cheats, debug settings and more.
/// </summary>
public class ControlPanel : UbiBCN.SingletonMonoBehaviour<ControlPanel> {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly Color FPS_THRESHOLD_COLOR_1 = new Color(40f/255f, 220f/255f, 140/255f, 0.75f);	// Green
	public static readonly Color FPS_THRESHOLD_COLOR_2 = new Color(230f/255f, 150f/255f, 0f/255f, 0.75f);	// Orange
	public static readonly Color FPS_THRESHOLD_COLOR_3 = new Color(230f/255f, 75f/255f, 030f/255f, 0.75f);	// Red

	private const string LAST_TAB_IDX_PREF_KEY = "ControlPanel.LastTabIdx";

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// External references
	[SerializeField] private Canvas m_canvas = null;
	[SerializeField] private RectTransform m_panel;
	public static RectTransform panel {
		get { return instance.m_panel; }
	}

	[SerializeField] private Button m_toggleButton;
	public static Button toggleButton {
		get { return instance.m_toggleButton; }
	}

	[SerializeField] private TextMeshProUGUI m_fpsCounter;
	public static TextMeshProUGUI fpsCounter {
		get { return instance.m_fpsCounter; }
	}

	[SerializeField] private GameObject m_statsCounter;
    public static GameObject statsCounter
    {
        get { return instance.m_statsCounter; }
    }

    private bool m_isStatsEnabled;
	public bool IsStatsEnabled {
		get { return m_isStatsEnabled; }
		set { OnDebugSettingChanged(DebugSettings.SHOW_STATS, value); }	// Use for quick override
	}

	private bool m_isFPSEnabled;

	[SerializeField] private TextMeshProUGUI m_memoryLabel;
	public static TextMeshProUGUI memoryLabel {
		get { return instance.m_memoryLabel; }
	}
	private bool m_showMemoryUsage;
    public bool ShowMemoryUsage {
        get {
			return m_showMemoryUsage;
        }

        set {
			m_showMemoryUsage = value;
            // Activate labels to show memory usage
			m_memoryLabel.gameObject.SetActive(m_showMemoryUsage);
        }        
    }

    [SerializeField]
    private TextMeshProUGUI m_entitiesCounter;

    [SerializeField]
    private TextMeshProUGUI m_logicUnitsCounter;

	[SerializeField]
	private TextMeshProUGUI m_vertexCount_npc;

	[SerializeField]
	private TextMeshProUGUI m_drawCalls_npc;


    // Exposed setup
    [Space]
	[SerializeField] private float m_activationTime = 3f;

	// Tabs setup
	[System.Serializable]
	private class TabSetup {
		public SelectableButton button = null;
		public GameObject prefab = null;
	}

	[Space]
	[SerializeField] private RectTransform m_tabsContainer = null;
	[SerializeField] private TabSystem m_tabs = null;
	[SerializeField] private TabSetup[] m_tabsSetup = null;

	// Internal logic
	private float m_activateTimer;
    const int m_NumDeltaTimes = 30;
	float[] m_DeltaTimes;
	int m_DeltaIndex;

	// FPS Recording
	[SerializeField] private CPFpsRecorder m_fpsRecorder;
	public static CPFpsRecorder fpsRecorder {
		get { return instance.m_fpsRecorder; }
	}

	// Int to String
	private static int MAX_INTEGER_AS_STRING = 150;
    private static string[] integerAsStrings;
    private static string NEGATIVE_STRING_AS_STRING = "-";

    public static string IntegerToString(int number)
    {
        string returnValue = null;
        if (integerAsStrings == null)
        {
            integerAsStrings = new string[MAX_INTEGER_AS_STRING + 2];
            int i;
            for (i = 1; i <= MAX_INTEGER_AS_STRING; i++)
            {
                integerAsStrings[i] = "" + i;
            }
            integerAsStrings[0] = "0";
            integerAsStrings[MAX_INTEGER_AS_STRING + 1] = "+" + MAX_INTEGER_AS_STRING;
        }

        if (number < 0)
        {
            returnValue = NEGATIVE_STRING_AS_STRING;
        }
        else if (number > MAX_INTEGER_AS_STRING)
        {
            returnValue = integerAsStrings[MAX_INTEGER_AS_STRING + 1];
        }
        else
        {
            returnValue = integerAsStrings[number];
        }

        return returnValue;
    }

    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    protected void Awake() {
		// Subscribe to external events
		Messenger.AddListener<string, bool>(MessengerEvents.CP_BOOL_CHANGED, OnDebugSettingChanged);

		// Start disabled
		m_panel.gameObject.SetActive(false);
		m_toggleButton.gameObject.SetActive( UnityEngine.Debug.isDebugBuild);
		OnDebugSettingChanged(DebugSettings.SHOW_STATS, DebugSettings.showStats);
		OnDebugSettingChanged(DebugSettings.SHOW_FPS, DebugSettings.showFps);
		OnDebugSettingChanged(DebugSettings.SHOW_FPS_RECORDER, DebugSettings.showFpsRecorder);
		ShowMemoryUsage = UnityEngine.Debug.isDebugBuild;
        m_logicUnitsCounter.transform.parent.gameObject.SetActive(UnityEngine.Debug.isDebugBuild && ProfilerSettingsManager.ENABLED);

		// Initialize tabs
		for(int i = 0; i < m_tabsSetup.Length; ++i) {
			// Create a new instance of the tab
			GameObject newTabObj = GameObject.Instantiate<GameObject>(m_tabsSetup[i].prefab, m_tabsContainer, false);

			// Add it to the tab system
			m_tabs.AddTab(i, m_tabsSetup[i].button, newTabObj.GetComponent<Tab>());
		}

		// Restore saved tab (if any)
		int lastTabIdx = PlayerPrefs.GetInt(LAST_TAB_IDX_PREF_KEY, -1);
		if(lastTabIdx >= 0) {
			m_tabs.GoToScreenInstant(lastTabIdx);
		}

		// Remember tab index every time the active tab changes
		m_tabs.OnScreenIndexChanged.AddListener(
			(int _newTabIdx) => {
				PlayerPrefs.SetInt(LAST_TAB_IDX_PREF_KEY, _newTabIdx);
			}
		);

		// Setup console channels
		Log_InitChannels();

        m_activateTimer = 0;
		CheckCanvasActivation();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string, bool>(MessengerEvents.CP_BOOL_CHANGED, OnDebugSettingChanged);
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	protected void Update() {
        if (FeatureSettingsManager.IsControlPanelEnabled) {
            if (Input.touchCount > 0 || Input.GetMouseButton(0)) {
                Vector2 pos = Vector2.zero;
                if (Input.touchCount > 0) {
                    Touch t = Input.GetTouch(0);
                    pos = t.position;
                } else {
                    pos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                }

                // Holding the top-left corner activates the control panel
                if (pos.x < (Screen.width * 0.15f) && pos.y > (Screen.height * 0.85f)) {
                    m_activateTimer += Time.unscaledDeltaTime;
                    if (m_activateTimer > m_activationTime) {
                        Toggle();
                        m_activateTimer = 0;
                    }

                } else {
                    m_activateTimer = 0;
                }
            } else {
                m_activateTimer = 0;
            }

#if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.Tab))
                Toggle();
#endif
        }


		if ( m_fpsCounter != null && m_isFPSEnabled )
		{
			float fps = FeatureSettingsManager.instance.AverageSystemFPS;
			if(fps >= 0) {
				if ( fps < 15 )
				{
					m_fpsCounter.color = FPS_THRESHOLD_COLOR_3;
				}
				else if ( fps < 25 )
				{
					m_fpsCounter.color = FPS_THRESHOLD_COLOR_2;
				}
				else
				{
					m_fpsCounter.color = FPS_THRESHOLD_COLOR_1;                   
				}

                // The string is taken from this array to prevent memory from being generated every tick
                m_fpsCounter.text = IntegerToString((int)fps);                
            } else { 
				m_fpsCounter.color = FPS_THRESHOLD_COLOR_1;
				m_fpsCounter.text = NEGATIVE_STRING_AS_STRING;
			}
		}

		if (m_showMemoryUsage)
		{
            if ((Time.frameCount % 200) == 0)
            {
                // Please don´t call every tick to GetMaxMemoryUsage or your device log will flood everything
                int mb = FGOL.Plugins.Native.NativeBinding.Instance.GetMemoryUsage() / (1024 * 1024);
                int maxMb = FGOL.Plugins.Native.NativeBinding.Instance.GetMaxMemoryUsage() / (1024 * 1024);
                m_memoryLabel.text = mb + "/" + maxMb;
            }
		}

        if (m_entitiesCounter != null && ProfilerSettingsManager.ENABLED)
        {
            int value = SpawnerManager.totalEntities;
            // The string is taken from this array to prevent memory from being generated every tick
            m_entitiesCounter.text = IntegerToString(value);
        }

        if (m_logicUnitsCounter != null && ProfilerSettingsManager.ENABLED)
        {
            int value = (int)SpawnerManager.totalLogicUnitsSpawned;
            // The string is taken from this array to prevent memory from being generated every tick
            m_logicUnitsCounter.text = IntegerToString(value);            
        }

		if (m_isStatsEnabled) {
			if (m_vertexCount_npc != null) {
				int vc = EntityManager.instance.totalVertexCount;
				if (vc > 30000) {
					m_vertexCount_npc.color = FPS_THRESHOLD_COLOR_3;
				} else if (vc > 25000) {
					m_vertexCount_npc.color = FPS_THRESHOLD_COLOR_2;
				} else {
					m_vertexCount_npc.color = FPS_THRESHOLD_COLOR_1;                   
				}
	 
				m_vertexCount_npc.text = ""+vc;
			}

			if (m_drawCalls_npc != null) {
				int dc = EntityManager.instance.drawCalls;
				if (dc > 39) {
					m_drawCalls_npc.color = FPS_THRESHOLD_COLOR_3;
				} else if (dc > 34) {
					m_drawCalls_npc.color = FPS_THRESHOLD_COLOR_2;
				} else {
					m_drawCalls_npc.color = FPS_THRESHOLD_COLOR_1;                   
				}

				m_drawCalls_npc.text = IntegerToString(dc);
			}
		}

#if UNITY_EDITOR
        // Quick Cheats
        if ( Input.GetKeyDown(KeyCode.L )){
			if ( InstanceManager.player != null ){
				// Dispatch global event
				Messenger.Broadcast<IDragonData>(MessengerEvents.DRAGON_LEVEL_UP, InstanceManager.player.data);
			}
		}

		if(Input.GetKeyUp(KeyCode.R)) {
			Debug.Log(Colors.orange.Tag("ADDING DEBUG REWARD!"));
			UsersManager.currentUser.rewardStack.Push(Metagame.Reward.CreateTypeCurrency(666, UserProfile.Currency.HARD, Metagame.Reward.Rarity.COMMON, HDTrackingManager.EEconomyGroup.CHEAT, "Debug HDK-6990"));
		}
#endif
    }

	/// <summary>
	/// Check whether canvas should be active (performance).
	/// </summary>
	private void CheckCanvasActivation() {
		// Toggle both canvas and camera
		// Check all widgets that are permanently displayed
		bool active = DebugSettings.showStats || DebugSettings.showFps || DebugSettings.showFpsRecorder || m_panel.gameObject.activeSelf;
		m_canvas.gameObject.SetActive(active);
		m_canvas.worldCamera.gameObject.SetActive(active);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Toggle the panel on and off.
	/// </summary>
	public void Toggle() {
		// Toggle panel
		m_panel.gameObject.SetActive(!m_panel.gameObject.activeSelf);
		CheckCanvasActivation();

		// Disable player control while control panel is up
		if(InstanceManager.player != null) {
			InstanceManager.player.playable = !m_panel.gameObject.activeSelf;
		}
	}

	/// <summary>
	/// Clear all prefs.
	/// </summary>
	public void OnResetPrefs() {
		// Do it - only player prefs!
		PlayerPrefs.DeleteAll();

        // The default value of some settings is true so we need to initialize them
        DebugSettings.Init();

		// Refresh control panel
		// Double toggle! xD
		Toggle();
		Toggle();
	}

	/// <summary>
	/// A sebug setting has been changed.
	/// </summary>
	/// <param name="_id">ID of the changed setting.</param>
	/// <param name="_newValue">New value of the setting.</param>
	private void OnDebugSettingChanged(string _id, bool _newValue) {
		// XP bar setting?
		switch(_id) {
			case DebugSettings.SHOW_STATS: {
				m_isStatsEnabled = _newValue;
				m_statsCounter.SetActive(m_isStatsEnabled);
				CheckCanvasActivation();
			} break;

			case DebugSettings.SHOW_FPS: {
				m_isFPSEnabled = _newValue;
				m_fpsCounter.gameObject.SetActive(m_isFPSEnabled);
				CheckCanvasActivation();
			} break;

			case DebugSettings.SHOW_FPS_RECORDER: {
				m_fpsRecorder.gameObject.SetActive(_newValue);
				CheckCanvasActivation();
			} break;
		}
	}

	//------------------------------------------------------------------//
	// LOG																//
	//------------------------------------------------------------------//
	#region log    
	public enum ELogChannel
    {        
        General,
        Customizer,
        GameCenter,
		ResultsScreen,
		LiveData,
        Store,
        CP2,
        Persistence,
		Server,
		Offers,
		Loading,
        Addressables
    };
    
    private static Dictionary<ELogChannel, string> sm_logChannelColors;
    private static Dictionary<ELogChannel, string> sm_logChannelPrefix;

	private static void Log_InitChannels() {
		Log_SetupChannel(ELogChannel.General, "", Color.white);
		Log_SetupChannel(ELogChannel.Customizer, "Customizer", Color.green);
		Log_SetupChannel(ELogChannel.ResultsScreen, "Results", Colors.paleYellow);
		Log_SetupChannel(ELogChannel.LiveData, "LiveEvents", Colors.aqua);
        Log_SetupChannel(ELogChannel.Store, "Store", Colors.coral);
        Log_SetupChannel(ELogChannel.CP2, "CP2", Colors.blue);
        Log_SetupChannel(ELogChannel.Persistence, "Persistence", Colors.fuchsia);
		Log_SetupChannel(ELogChannel.Server, "Server", Colors.teal);
		Log_SetupChannel(ELogChannel.Offers, "Offers", Colors.gold);
		Log_SetupChannel(ELogChannel.Loading, "Loading", Colors.darkGray);
        Log_SetupChannel(ELogChannel.Addressables, "Addressables", Colors.lime);        
    }

    private static string Log_GetChannelColor(ELogChannel channel)
    {
        string returnValue = null;
        if (sm_logChannelColors != null && sm_logChannelColors.ContainsKey(channel))
        {
            returnValue = sm_logChannelColors[channel];            
        }

        return returnValue;
    }

    private static string Log_GetChannelPrefix(ELogChannel channel)
    {
        if (sm_logChannelPrefix == null)
        {
            sm_logChannelPrefix = new Dictionary<ELogChannel, string>();
        }

        if (!sm_logChannelPrefix.ContainsKey(channel))
        {
            sm_logChannelPrefix[channel] = "[" + channel.ToString() + "] ";
        }

        return sm_logChannelPrefix[channel];
    }

	public static void Log_SetupChannel(ELogChannel channel, string _prefix, Color color) {
		// Prefix
		if(sm_logChannelPrefix == null) {
			sm_logChannelPrefix = new Dictionary<ELogChannel, string>();
		}
		
		if(!string.IsNullOrEmpty(_prefix)) {
			_prefix = "[" + _prefix + "] ";
		}
		sm_logChannelPrefix[channel] = _prefix;

		// Color
        if(sm_logChannelColors == null) {
            sm_logChannelColors = new Dictionary<ELogChannel, string>();
        }
		
		if(color == Color.white) {
			sm_logChannelColors[channel] = null;	// Use default color instead of white
		} else {
			sm_logChannelColors[channel] = Colors.ToHexString(color, "#", false);
		}
    }

    public static string COLOR_ERROR = Colors.ToHexString(Color.red, "#", false);
    public static string COLOR_WARNING = Colors.ToHexString(Color.yellow, "#", false);

#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    public static void LogError(string text, ELogChannel channel=ELogChannel.General)
    {
        LogToCPConsole(text, channel, COLOR_ERROR);        
        text = Log_GetChannelPrefix(channel) + text;
        Debug.LogError(text);        
    }

#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    public static void LogWarning(string text, ELogChannel channel = ELogChannel.General)
    {
        LogToCPConsole(text, channel, COLOR_WARNING);        
        text = Log_GetChannelPrefix(channel) + text;
        Debug.LogWarning(text);        
    }

#if ENABLE_LOGS
    [Conditional("DEBUG")]
#else
    [Conditional("FALSE")]
#endif
    public static void Log(string text, ELogChannel channel=ELogChannel.General, bool logToCPConsole=true, bool logToUnityConsole=true) {        
        if (logToCPConsole) {
            LogToCPConsole(text, channel);
        }

        if (logToUnityConsole) {
            LogToUnityConsole(text, channel);
        }
    }   

    private static void LogToCPConsole(string text, ELogChannel channel = ELogChannel.General, string color=null) {
        // It's logged to control panel console
        if (FeatureSettingsManager.IsControlPanelEnabled) {
            if (color == null)
            {
                color = Log_GetChannelColor(channel);
            }

            text = Log_GetChannelPrefix(channel) + text;
            CPConsoleTab.Log(text, color);
        }
    }

    private static void LogToUnityConsole(string text, ELogChannel channel=ELogChannel.General)
    {
        // It's logged to Unity console too        
        text = Log_GetChannelPrefix(channel) + text;

#if UNITY_EDITOR
        string color = Log_GetChannelColor(channel);
        if (!string.IsNullOrEmpty(color))
        {   
			// [AOC] Unfortunately, color is not properly displayed in the Unity Console if the text has more than one line break
			//		 Workaround it by just coloring the first line

			// Insert opening tag
			text = text.Insert(0, "<color=" + color + ">");

			// Insert closing tag right before the first line break, or at the end if no line breaks are found
			int idx = text.IndexOf('\n');
			if(idx > 0) {
				text = text.Insert(idx, "</color>");
			} else {
				text = text + "</color>";
			}
        }            
#endif

        Debug.Log(text);        
    }

	/// <summary>
	/// Trigger a text feedback.
	/// </summary>
	public static void LaunchTextFeedback(string _text, Color _color) {
		UIFeedbackText text = UIFeedbackText.CreateAndLaunch(
			_text,
			new Vector2(0.5f, 0.5f),
			ControlPanel.panel.parent as RectTransform
		);
		text.text.color = _color;
	}
    #endregion
}