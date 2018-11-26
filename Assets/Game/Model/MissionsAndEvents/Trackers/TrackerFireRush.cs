// TrackerBase.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Tracker for score.
/// </summary>
public class TrackerFireRush : TrackerBase, IBroadcastListener {
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Default constructor.
	/// </summary>
	public TrackerFireRush() {
		// Subscribe to external events
		Broadcaster.AddListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);
	}

	/// <summary>
	/// Destructor
	/// </summary>
	~TrackerFireRush() {
		
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Finalizer method. Leave the tracker ready for garbage collection.
	/// </summary>
	override public void Clear() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);

		// Call parent
		base.Clear();
	}

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.FURY_RUSH_TOGGLED:
            {
                FuryRushToggled furyRushToggled = (FuryRushToggled)broadcastEventInfo;
                OnFireRushToggled( furyRushToggled.activated, furyRushToggled.type );
            }break;
        }
    }
    
	/// <summary>
	/// Localizes and formats the description according to this tracker's type
	/// (i.e. "Eat 52 birds", "Dive 500m", "Survive 10 minutes").
	/// </summary>
	/// <returns>The localized and formatted description for this tracker's type.</returns>
	/// <param name="_tid">Description TID to be formatted.</param>
	/// <param name="_targetValue">Target value. Will be placed at the %U0 replacement slot.</param>
	/// <param name="_replacements">Other optional replacements, starting at %U1.</param>
	override public string FormatDescription(string _tid, long _targetValue, params string[] _replacements) {
		// Singular/Plural issue (https://mdc-tomcat-jira100.ubisoft.org/jira/browse/HDK-1202)
		// Figure out which tid to use
		string timeTid = "TID_GEN_TIME";
		if(_targetValue > 1) {
			timeTid = "TID_GEN_TIME_PLURAL";
		}

		// Insert it at the start of the replacements array
		List<string> replacementsList = (_replacements == null) ? new List<string>(1) : _replacements.ToList();
		replacementsList.Insert(0, LocalizationManager.SharedInstance.Localize(timeTid));

		// Call parent
		return base.FormatDescription(_tid, _targetValue, replacementsList.ToArray());
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The fire rush has been toggled.
	/// </summary>
	/// <param name="_toggled">Whether it has been activated or deactivated.</param>
	/// <param name="_type">The type of fire rush (mega?).</param>
	private void OnFireRushToggled(bool _toggled, DragonBreathBehaviour.Type _type) {
		// If activated, increase current value
		if(_toggled) currentValue++;
	}
}