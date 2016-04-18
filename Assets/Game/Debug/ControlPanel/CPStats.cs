// CPVersion.cs
// Hungry Dragon
// 
// Created by Miguel Angel Linares on 17/12/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple widget to display number version.
/// </summary>
public class CPStats : MonoBehaviour {
	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	public Text m_DeviceModel;
	public Text m_FpsLabel;
	public Text m_ScreenSize;
	public Text m_LevelName;

	ControlPanel m_ControlPanel;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() 
	{
		// Just initialize text
		m_DeviceModel.text = "Model: " + SystemInfo.deviceModel;
		m_FpsLabel.text = "FPS: ";
		m_ScreenSize.text = "Screen Size: " + Screen.width + "x"+ Screen.height;
		m_LevelName.text = "Scene Name: "+ SceneManager.GetActiveScene().name;
		m_ControlPanel = GetComponentInParent<ControlPanel>();
	}

	private void Update()
	{
		m_FpsLabel.text = "FPS: " + m_ControlPanel.GetFPS();
	}
}