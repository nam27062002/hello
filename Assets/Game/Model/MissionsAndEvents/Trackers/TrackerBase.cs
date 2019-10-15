// TrackerBase.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;
using System.Linq;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Base class for trackers used in missions and global events.
/// </summary>
[Serializable]
public class TrackerBase {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	// Add as many as needed
	// Might be use by specific trackers to tune some features based on mode (i.e. value formatting)
	public enum Mode {
		GENERIC,
		MISSION,
		GLOBAL_EVENT
	}
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Variables
	[SerializeField] private long m_currentValue = 0;
	public long currentValue { 
		get { return m_currentValue; }
		set { SetValue(value); }
	}

	private bool m_enabled = false;
	public bool enabled {
		get { return m_enabled; }
		set { 
            m_enabled = value; 
        }
	}

	protected Mode m_mode = Mode.GENERIC;
	public Mode mode {
		get { return m_mode; }
		set { m_mode = value; }
	}

	// Events
	public UnityEvent OnValueChanged = new UnityEvent();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerBase() {

	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerBase() {

	}

	//------------------------------------------------------------------------//
	// OVERRIDE CANDIDATE METHODS											  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finalizer method. Leave the tracker ready for garbage collection.
	/// </summary>
	public virtual void Clear() {
		// Reset current value
		m_currentValue = 0;

		// Remove any listener
		OnValueChanged.RemoveAllListeners();
		OnValueChanged = null;
	}

	/// <summary>
	/// Localizes and formats the description according to this tracker's type
	/// (i.e. "Eat 52 birds", "Dive 500m", "Survive 10 minutes").
	/// </summary>
	/// <returns>The localized and formatted description for this tracker's type.</returns>
	/// <param name="_tid">Description TID to be formatted.</param>
	/// <param name="_targetValue">Target value. Will be placed at the %U0 replacement slot.</param>
	/// <param name="_replacements">Other optional replacements, starting at %U1.</param>
	public virtual string FormatDescription(string _tid, long _targetValue, params string[] _replacements) {
		// Default: Add the formatted value to the replacements array and localize
		// No need if there are no replacements
		if(_replacements == null || _replacements.Length == 0) {
			return LocalizationManager.SharedInstance.Localize(_tid, FormatValue(_targetValue));
		} else {
			List<string> replacementsList = _replacements.ToList();
			replacementsList.Insert(0, FormatValue(_targetValue));
			return LocalizationManager.SharedInstance.Localize(_tid, replacementsList.ToArray());
		}
	}

	/// <summary>
	/// Localizes and formats a value according to this tracker's type
	/// (i.e. "52", "500 meters", "10 minutes").
	/// </summary>
	/// <returns>The localized and formatted value for this tracker's type.</returns>
	/// <param name="_value">Value to be formatted.</param>
	public virtual string FormatValue(long _value) {
		// Default: the number as is
		return StringUtils.FormatNumber(_value);
	}

	/// <summary>
	/// Round a value according to specific rules defined for every tracker type.
	/// Typically used for target values.
	/// </summary>
	/// <returns>The rounded value.</returns>
	/// <param name="_targetValue">The original value to be rounded.</param>
	public virtual long RoundTargetValue(long _targetValue) {
		// Default: At least 1, no decimals
		//return Mathf.Max(1f, Mathf.Round(_targetValue));
		return _targetValue;
	}

	/// <summary>
	/// Gets the progress string, custom formatted based on tracker type.
	/// </summary>
	/// <returns>The progress string properly formatted.</returns>
	/// <param name="_currentValue">Current value to be evaluated.</param>
	/// <param name="_targetValue">Target value to be evaulated.</param>
	/// <param name="_showTarget">Show target value? (i.e. "25/40"). Some types might override this setting if not appliable.</param>
	public virtual string GetProgressString(long _currentValue, long _targetValue, bool _showTarget = true) {
		if(_showTarget) {
			return LocalizationManager.SharedInstance.Localize("TID_FRACTION", FormatValue(_currentValue), FormatValue(_targetValue));
		} else {
			return FormatValue(_currentValue);
		}
	}

	/// <summary>
	/// Sets the initial value for the tracker.
	/// Doesn't perform any check or trigger any event.
	/// Use for initialization/reset/restore persistence.
	/// Use also by heirs to reset any custom vars that needed to be reset.
	/// </summary>
	/// <param name="_initialValue">Initial value.</param>
	public virtual void InitValue(long _initialValue) {
		// Just store new value
		m_currentValue = _initialValue;
	}

	/// <summary>
	/// Sets a new value and triggers OnValueChanged event.
	/// </summary>
	/// <param name="_newValue">The new value to be set.</param>
	public virtual void SetValue(long _newValue) {
		// Skip if not enabled
		if(!m_enabled) return;

		// Nothing to do if value is already set
		if(m_currentValue == _newValue) return;

		// Store new value
		m_currentValue = _newValue;

		// Trigger event
		OnValueChanged.Invoke();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// FACTORY																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Create and initialize a new tracker based on the given type.
	/// </summary>
	/// <returns>A new tracker. <c>null</c> if the type was not recognized.</returns>
	/// <param name="_typeSku">The type of tracker to be created. Typically a sku from the MissionTypesDefinitions table.</param>
	/// <param name="_params">Optional parameters to be passed to the tracker. Depend on tracker's type.</param>
	public static TrackerBase CreateTracker(string _typeSku, List<string> _params = null) {
		// Create a new objective based on mission type
		switch(_typeSku) {
			default: return null;
			case "score":			return new TrackerScore();
			case "gold":			return new TrackerGold();
			case "survive_time":	return new TrackerSurviveTime();
			case "zone_survive":	return new TrackerZoneSurvive(_params);
			case "visited_zones":	return new TrackerVisitedZones();
            case "kill_disguise":
			case "kill":			return new TrackerKill(_params);
            case "kill_in_love":    return new TrackerKillInLove(_params);
            case "kill_stunned":    return new TrackerKillStunned(_params);
            case "kill_frozen":     return new TrackerKillFrozen(_params);
            case "kill_equipped":   return new TrackerKillEquipped(_params);
			case "burn":			return new TrackerBurn(_params);
			case "distance":		return new TrackerDistance();
			case "dive":			return new TrackerDiveDistance();
			case "dive_time":		return new TrackerDiveTime();
            case "space":           return new TrackerSpaceDistance();
            case "space_time":      return new TrackerSpaceTime();
            case "fire_rush":		return new TrackerFireRush();
			case "destroy":			return new TrackerDestroy(_params);
			case "kill_or_destroy":	return new TrackerKillOrDestroy(_params);
			case "unlock_dragon":	return new TrackerUnlockDragon(_params);
			case "buy_skins":		return new TrackerBuySkins();
			case "daily_chest":		return new TrackerDailyChests();
			case "kill_chain":		return new TrackerKillChain(_params);
			case "critical_time":	return new TrackerCriticalTime();
			// new missions TODO
			case "eat_gold":        return new TrackerEatGolden(_params);
			case "eat_suicidal":    return new TrackerEatWhileActionActive(TrackerEatWhileActionActive.Actions.FreeFall, _params);
			case "eat_spec_anim_a": return new TrackerEatWhileActionActive(TrackerEatWhileActionActive.Actions.PilotActionA, _params);

            case "birthday_mode_count": return new TrackerBirthdayMode();
            case "birthday_stay_mode_time":  return new TrackerBirthdayModeTime();
            //-----------------------------------

            // Collect is quite special: depending on first parameter, create one of the existing trackers
            case "collect": {
				if(_params.Count < 1) return null;
				switch(_params[0]) {
					case "coins":	return new TrackerGold();
					case "eggs":	return new TrackerEggs();
					case "chests":	return new TrackerChests();
				}
			} break;

			case "destroy_blocker": return new TrackerDestroyBlockers(); break;
		}

		// Unrecoginzed mission type, aborting
		return null;
	}
    
    /// <summary>
    /// Refreshs the current value. This function will be called on the achievements that need to check a specific value on the profile
    /// Used for example in checking unlocking dragons and number of skins because we cannot unlock a dragon again
    /// </summary>
    public virtual void RefreshCurrentValue(){
    }
}