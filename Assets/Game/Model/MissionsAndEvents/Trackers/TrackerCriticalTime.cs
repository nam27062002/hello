// TrackerDiveTime.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tracker for diving time.
/// </summary>
public class TrackerCriticalTime : TrackerBase {
	//------------------------------------------------------------------------//
	// MEMBERS																  //
	//------------------------------------------------------------------------//
	// Internal
	private bool m_critical = false;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerCriticalTime() {
		// Subscribe to external events
		Messenger.AddListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Messenger.AddListener(MessengerEvents.GAME_UPDATED, OnGameUpdated);
		Messenger.AddListener<DragonHealthModifier, DragonHealthModifier>(MessengerEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerCriticalTime() {
		
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finalizer method. Leave the tracker ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unsubscribe from external events
		Messenger.RemoveListener(MessengerEvents.GAME_STARTED, OnGameStarted);
		Messenger.RemoveListener(MessengerEvents.GAME_UPDATED, OnGameUpdated);
		Messenger.RemoveListener<DragonHealthModifier, DragonHealthModifier>(MessengerEvents.PLAYER_HEALTH_MODIFIER_CHANGED, OnHealthModifierChanged);

		// Call parent
		base.Clear();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A new game has started.
	/// </summary>
	private void OnGameStarted() {
		// Reset flag
		m_critical = false;
		currentValue = 0;
	}


	/// <summary>
	/// Localizes and formats a value according to this tracker's type
	/// (i.e. "52", "500 meters", "10 minutes").
	/// </summary>
	/// <returns>The localized and formatted value for this tracker's type.</returns>
	/// <param name="_value">Value to be formatted.</param>
	override public string FormatValue(float _value) {
		// Format value as time
		// [AOC] Different formats for global events!
		TimeUtils.EFormat format = TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES;
		if(m_mode == Mode.GLOBAL_EVENT) {
			format = TimeUtils.EFormat.WORDS_WITHOUT_0_VALUES;
		}
		return TimeUtils.FormatTime(_value, format, 3, TimeUtils.EPrecision.DAYS);
	}

	/// <summary>
	/// Gets the progress string, custom formatted based on tracker type.
	/// </summary>
	/// <returns>The progress string properly formatted.</returns>
	/// <param name="_currentValue">Current value to be evaluated.</param>
	/// <param name="_targetValue">Target value to be evaulated.</param>
	/// <param name="_showTarget">Show target value? (i.e. "25/40"). Some types might override this setting if not appliable.</param>
	public override string GetProgressString(float _currentValue, float _targetValue, bool _showTarget = true) {
		//Time trackers will show a percentage as a progress string
		float p = (_currentValue * 100) / _targetValue;
		return StringUtils.FormatNumber(p, 2, 0, false) + "%";
	}

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void OnGameUpdated() {
		// We'll receive this event only while the game is actually running, so no need to check anything
		// Is the dragon underwater?
		if(m_critical) {
			currentValue += Time.deltaTime;
		}
	}

	/// <summary>
	/// Raises the health modifier changed event.
	/// </summary>
	/// <param name="_oldModifier">Old modifier.</param>
	/// <param name="_newModifier">New modifier.</param>
	private void OnHealthModifierChanged( DragonHealthModifier _oldModifier, DragonHealthModifier _newModifier )
	{
		m_critical = (_newModifier != null && _newModifier.IsCritical());
		if (!m_critical)
			currentValue = 0;
	}
}