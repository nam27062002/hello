// ControlPanel.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
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
	public static readonly Color FPS_THRESHOLD_COLOR_1 = new Color(0f, 1f, 0f, 0.75f);	// Green
	public static readonly Color FPS_THRESHOLD_COLOR_2 = new Color(1f, 0.5f, 0f, 0.75f);	// Orange
	public static readonly Color FPS_THRESHOLD_COLOR_3 = new Color(1f, 0f, 0f, 0.75f);	// Red

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// External references
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

    private bool m_isFpsEnabled;
    public bool IsFpsEnabled {
        get {
            return m_isFpsEnabled;
        }

        set {
            m_isFpsEnabled = value;
            m_fpsCounter.gameObject.SetActive(m_isFpsEnabled);
        }        
    }

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
			m_memoryLabel.gameObject.SetActive(true);
        }        
    }

    [SerializeField]
    private TextMeshProUGUI m_entitiesCounter;

    [SerializeField]
    private TextMeshProUGUI m_logicUnitsCounter;

    // Exposed setup
    [Space]
	[SerializeField] private float m_activationTime = 3f;

	// Internal logic
	private float m_activateTimer;
    const int m_NumDeltaTimes = 30;
	float[] m_DeltaTimes;
	int m_DeltaIndex;

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
		// Start disabled
		m_panel.gameObject.SetActive(false);
		m_toggleButton.gameObject.SetActive( UnityEngine.Debug.isDebugBuild);
        IsFpsEnabled = UnityEngine.Debug.isDebugBuild;        
        ShowMemoryUsage = UnityEngine.Debug.isDebugBuild;
        m_logicUnitsCounter.transform.parent.gameObject.SetActive(UnityEngine.Debug.isDebugBuild && ProfilerSettingsManager.ENABLED);

        m_activateTimer = 0;
	}

	void Start()
	{
		// FPS Initialization
		m_DeltaTimes = new float[ m_NumDeltaTimes ];
		m_DeltaIndex = 0;
		float initValue = 1.0f / 30.0f;
		if ( Application.targetFrameRate > 0 )
			initValue = 1.0f / Application.targetFrameRate;
		for( int i = 0; i<m_NumDeltaTimes; i++ )
			m_DeltaTimes[i] = initValue;
	}


	protected void Update()
	{
		if ( Input.touchCount > 0 || Input.GetMouseButton(0))
		{
			Vector2 pos = Vector2.zero;
			if(Input.touchCount > 0) {
				Touch t = Input.GetTouch(0);
				pos = t.position;
			} else {
				pos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			}

			if (pos.x < (Screen.width * 0.1f) && pos.y < (Screen.height * 0.1f))
			{
                m_activateTimer += Time.unscaledDeltaTime;
				if ( m_activateTimer > m_activationTime )
				{
					Toggle();
					m_activateTimer = 0;
				}
				
			}
			else
			{
				m_activateTimer = 0;
			}
		}
		else
		{
			m_activateTimer = 0;
		}

		if ( Input.GetKeyDown(KeyCode.Tab) )
			Toggle();


		// Update FPS
		m_DeltaTimes[ m_DeltaIndex ] = Time.deltaTime;
		m_DeltaIndex++;
		if ( m_DeltaIndex >= m_NumDeltaTimes )
			m_DeltaIndex = 0;

		if ( m_fpsCounter != null )
		{
			float fps = GetFPS();            
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
			int mb = FGOL.Plugins.Native.NativeBinding.Instance.GetMemoryUsage() / (1024*1024);
			int maxMb = FGOL.Plugins.Native.NativeBinding.Instance.GetMaxMemoryUsage()/ (1024*1024);
			m_memoryLabel.text = mb + "/" + maxMb;
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


        // Quick Cheats
        if ( Input.GetKeyDown(KeyCode.L )){
			if ( InstanceManager.player != null ){
				// Dispatch global event
				Messenger.Broadcast<DragonData>(GameEvents.DRAGON_LEVEL_UP, InstanceManager.player.data);
			}
		}
	}

	public float GetFPS()
	{
		float median = 0;
		for( int i = 0; i<m_NumDeltaTimes; i++ )
		{
			median += m_DeltaTimes[i];
		}
		median = median / m_NumDeltaTimes;
		return 1.0f / median;
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
}