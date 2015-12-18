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

	const int m_NumDeltaTimes = 30;
	float[] m_DeltaTimes;
	int m_DeltaIndex;

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
		m_LevelName.text = "Scene Name: "+ Application.loadedLevelName;

		// FPS Initialization
		m_DeltaTimes = new float[ m_NumDeltaTimes ];
		m_DeltaIndex = 0;
		float initValue = 1.0f / 30.0f;
		if ( Application.targetFrameRate > 0 )
			initValue = 1.0f / Application.targetFrameRate;
		for( int i = 0; i<m_NumDeltaTimes; i++ )
			m_DeltaTimes[i] = initValue;
	}

	private void Update()
	{
		// Update FPS
		m_DeltaTimes[ m_DeltaIndex ] = Time.deltaTime;
		m_DeltaIndex++;
		if ( m_DeltaIndex >= m_NumDeltaTimes )
			m_DeltaIndex = 0;
		m_FpsLabel.text = "FPS: " + GetFPS();
	}

	private float GetFPS()
	{
		float median = 0;
		for( int i = 0; i<m_NumDeltaTimes; i++ )
		{
			median += m_DeltaTimes[i];
		}
		median = median / m_NumDeltaTimes;
		return 1.0f / median;
	}
}