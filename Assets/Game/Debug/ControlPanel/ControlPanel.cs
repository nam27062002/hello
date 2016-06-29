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
//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// In-game control panel for cheats, debug settings and more.
/// </summary>
public class ControlPanel : SingletonMonoBehaviour<ControlPanel> {
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

	[SerializeField] private Text m_fpsCounter;
	public static Text fpsCounter {
		get { return instance.m_fpsCounter; }
	}

	// Exposed setup
	[Space]
	[SerializeField] private float m_activationTime = 3f;

	// Internal logic
	private float m_activateTimer;
	const int m_NumDeltaTimes = 30;
	float[] m_DeltaTimes;
	int m_DeltaIndex;

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
		m_fpsCounter.gameObject.SetActive( UnityEngine.Debug.isDebugBuild);
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
				m_activateTimer += Time.deltaTime;
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
				m_fpsCounter.text = ((int)fps).ToString("D");
			} else { 
				m_fpsCounter.color = FPS_THRESHOLD_COLOR_1;
				m_fpsCounter.text = "-";
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

	public void OnUnlockCasablancaLevels(bool _value) {
		List<DefinitionNode> levels= new List<DefinitionNode>();
		DefinitionsManager.SharedInstance.GetDefinitions(DefinitionsCategory.LEVELS, ref levels);
		DefinitionsManager.SharedInstance.SortByProperty(ref levels, "order", DefinitionsManager.SortType.NUMERIC);

		for (int i = 1; i < levels.Count; i++) {
			if (_value) {
				levels[i].SetValue("comingSoon", "false");
			} else {
				levels[i].SetValue("comingSoon", "true");
			}
		}

		Messenger.Broadcast(GameEvents.DEBUG_UNLOCK_LEVELS);
	}
}