// MissionObjective_NEW.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Objective for a mission.
/// </summary>
[Serializable]
public class AchievementObjective : TrackingObjectiveBase {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	protected bool m_singleRun = false;
	public bool singleRun { get { return m_singleRun; }}

	protected string m_achievementSku = "";
	public string achievementSku { get { return m_achievementSku; }}

	protected bool m_reported = false;
	public bool reported { get { return m_reported; }}

	protected bool m_reportProgress = false;
	public bool reportProgress { get { return m_reportProgress; }}

	private int m_stepSize = 1;
	private int m_lastStep = -1;
	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//

	//------------------------------------------------------------------//
	// METHODS															//
	//------------------------------------------------------------------//
	/// <summary>
	/// Parametrized constructor.
	/// </summary>
	/// <param name="_parentMission">The mission owning this objective.</param>
	/// <param name="_missionDef">The mission definition used to initialize this objective.</param>
	/// <param name="_typeDef">The mission type definition.</param>
	/// <param name="_targetValue">Target value.</param>
	/// <param name="_singleRun">Is it a single run mission?</param>
	public AchievementObjective(DefinitionNode _achievementDef) {
		// Check params
		Debug.Assert(_achievementDef != null);

		// Single run?
		m_singleRun = _achievementDef.GetAsBool("singleRun");
		m_reportProgress = _achievementDef.GetAsBool("resportProgress");
		m_achievementSku = _achievementDef.sku;
		m_stepSize = _achievementDef.GetAsInt("stepSize", 1);

		// Use parent's initializer
		int _targetValue = _achievementDef.GetAsInt("amount", 1);
		Init(
			TrackerBase.CreateTracker(_achievementDef.Get("type"), _achievementDef.GetAsList<string>("params")),		// Create the tracker based on mission type
			_targetValue,
			null,
			"",
			""
		);

		// Subscribe to external events
		Messenger.AddListener(GameEvents.GAME_STARTED, OnGameStarted);
		Messenger.AddListener(GameEvents.GAME_ENDED, OnGameEnded);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~AchievementObjective() {
		// Unsubscribe from external events
		Messenger.RemoveListener(GameEvents.GAME_STARTED, OnGameStarted);
		Messenger.RemoveListener(GameEvents.GAME_ENDED, OnGameEnded);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A new game has started.
	/// </summary>
	public virtual void OnGameStarted() {
		// If we're a single-run objective, reset counter
		if(m_singleRun) {
			m_tracker.SetValue(0f, false);
		}
	}

	/// <summary>
	/// A new game has just ended.
	/// </summary>
	public virtual void OnGameEnded() {
		// If we're a single-run objective, reset counter
		// Unless objective was completed
		if(m_singleRun && !isCompleted) {
			m_tracker.SetValue(0f, false);
		}
	}

	/// <summary>
	/// The tracker's value has changed.
	/// </summary>
	override public void OnValueChanged() {
		// Check completion
		if(isCompleted) {
			// Cap value to target value
			m_tracker.SetValue(Mathf.Min(currentValue, (float)targetValue), false);

			// Stop tracking
			m_tracker.enabled = false;

			if ( GameCenterManager.SharedInstance.CheckIfAuthenticated() )
			{
				// Report Achievement
				if ( m_reportProgress )
				{
					GameCenterManager.SharedInstance.ReportAchievementTotal( m_achievementSku , (int)(targetValue / m_stepSize));
				}
				else
				{
					GameCenterManager.SharedInstance.ReportAchievement( m_achievementSku );
				}
				m_reported = true;
			}

			// Invoke delegate
			OnObjectiveComplete.Invoke();
		}
		else
		{
			if ( m_reportProgress && GameCenterManager.SharedInstance.CheckIfAuthenticated() )
			{
				int newStep = (int)(currentValue / m_stepSize);
				if ( newStep != m_lastStep)
				{
					// Report progress
					GameCenterManager.SharedInstance.ReportAchievementTotal( m_achievementSku , newStep);
				}
				m_lastStep = newStep;
			}
		}
	}
}