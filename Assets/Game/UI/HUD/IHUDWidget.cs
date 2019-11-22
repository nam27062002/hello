// IHUDWidget.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/08/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Interface for in-game HUD widgets requiring an update.
/// </summary>
public abstract class IHUDWidget : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Internal
	private float m_updateInterval = 0f;
	public float UPDATE_INTERVAL {
		get { return m_updateInterval; }
	}

	//------------------------------------------------------------------------//
	// ABSTRACT METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Called at regular intervals.
	/// </summary>
	public abstract void PeriodicUpdate();

	/// <summary>
	/// How often the widget should be updated for the given graphic quality level.
	/// </summary>
	/// <param name="_qualityLevel">Graphics quality level to be considered. A value in [0, MAX_PROFILE_LEVEL] if the user has ever chosen a profile, otherwise <c>-1</c>.</param>
	/// <returns>Seconds, how often the widget should be refreshed.</returns>
	public abstract float GetUpdateIntervalByQualityLevel(int _qualityLevel);

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	protected virtual void Awake() {
		// Init update interval based on current quality settings
		m_updateInterval = GetUpdateIntervalByQualityLevel(FeatureSettingsManager.instance.GetCurrentProfileLevel());
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	protected virtual void OnEnable() {
		// Self-register to the manager
		InstanceManager.gameSceneControllerBase.hudManager.AddWidget(this);
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	protected virtual void OnDisable() {
        if (ApplicationManager.IsAlive) {
            // Self-unregister to the manager
            InstanceManager.gameSceneControllerBase.hudManager.RemoveWidget(this);
        }
	}
}