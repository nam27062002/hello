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
//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// In-game control panel for cheats, debug settings and more.
/// </summary>
public class ControlPanel : SingletonMonoBehaviour<ControlPanel> {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	[SerializeField] private GameObject m_panel;
	public GameObject m_toggleButton;
	public Text m_fpsCounter;
	private float m_activateTimer;

	const int m_NumDeltaTimes = 30;
	float[] m_DeltaTimes;
	int m_DeltaIndex;
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected void Awake() {
		// Start disabled
		m_panel.SetActive(false);
		m_toggleButton.SetActive( Debug.isDebugBuild );
		m_fpsCounter.gameObject.SetActive( Debug.isDebugBuild );
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
		if ( Input.touchCount > 0 )
		{
			Touch t = Input.GetTouch(0);
			Debug.Log( t.position );
			if (t.position.x < (Screen.width * 0.1f) && t.position.y < (Screen.height * 0.1f))
			{
				m_activateTimer += Time.deltaTime;
				if ( m_activateTimer > 3.0f )
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
			if ( fps < 15 )
			{
				m_fpsCounter.color = Color.red;
			}
			else if ( fps < 25 )
			{
				m_fpsCounter.color = Color.yellow;
			}
			else
			{
				m_fpsCounter.color = Color.green;
			}
			m_fpsCounter.text = ((int)fps).ToString("D");
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
		m_panel.SetActive(!m_panel.activeSelf);

		// Disable player control while control panel is up
		if(InstanceManager.player != null) {
			InstanceManager.player.playable = !m_panel.activeSelf;
		}
	}
}