// ControlPanel.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

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
	private float m_activateTimer;

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
		m_activateTimer = 0;
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