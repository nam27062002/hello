﻿// LoadingSceneController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/08/2015.
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
/// Main controller for the loading scene.
/// </summary>
public class LoadingSceneController : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string NAME = "SC_Loading";

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	[SerializeField] private Text m_loadingTxt = null;
	[SerializeField] private Slider m_loadingBar = null;

	// Internal
	private float timer = 0;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	void Awake() {
		DebugUtils.Assert(m_loadingTxt != null, "Required component!");
		DebugUtils.Assert(m_loadingBar != null, "Required component!");
	}

	/// <summary>
	/// First update.
	/// </summary>
	void Start() {
		// Load menu scene
		//GameFlow.GoToMenu();
		// [AOC] TEMP!! Simulate loading time with a timer for now
		timer = 0;
	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	void Update() {
		// Update load progress
		//m_loadingTxt.text = System.String.Format("LOADING {0}%", StringUtils.FormatNumber(SceneManager.loadProgress * 100f, 0));

		// [AOC] TODO!! Fake timer for now
		timer += Time.deltaTime;
		float loadProgress = Mathf.Min(timer/2f, 1f);	// Divide by the amount of seconds to simulate
		m_loadingTxt.text = System.String.Format("LOADING {0}%", StringUtils.FormatNumber(loadProgress * 100f, 0));
		m_loadingBar.normalizedValue = loadProgress;

		// Once load is finished, navigate to the menu scene
		if(loadProgress >= 1f && !SceneManager.isLoading) GameFlow.GoToMenu();
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	void OnDestroy() {

	}
}

