// CPPlayerSource.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/09/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Reflection;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Debug class to provide cheats to modify the Player Source.
/// </summary>
public class CPPlayerSource : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private TMP_Dropdown m_playerSourceDropdown = null;

	// Internal
	private List<string> m_options = new List<string>();
	private List<string> m_values = new List<string>();

	//------------------------------------------------------------------------//
	// METHODS																  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	public void Awake() {
		// Reset internal vars
		m_options = new List<string>();
		m_values = new List<string>();

		// Use reflection to read all sources from game constants
		// @see https://stackoverflow.com/questions/10261824/how-can-i-get-all-constants-of-a-type-by-reflection
		// Public static fields
		FieldInfo[] fields = typeof(TrackingPersistenceSystem).GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);	// FlattenHierarchy to get the fields from all base types as well
		for(int i = 0; i < fields.Length; i++) {
			// We only care about strings
			if(fields[i].FieldType == typeof(string)) {
				// Is it a constant?
				// IsLiteral determines if its value is written at compile time and not changeable
				// IsInitOnly determines if the field can be set in the body of the constructor
				// For C#, a field which is readonly keyword would have both true but a const field would have only IsLiteral equal to true
				if(fields[i].IsLiteral || fields[i].IsInitOnly) {	// [AOC] In this case, since we're interested in both constants and readonlys, use both
					// It's a constant!
					// Does the name match the pattern we want?
					if(fields[i].Name.StartsWith("PLAYER_SOURCE_")) {
						// Yes! Add option to the list and store its value
						m_options.Add(fields[i].Name);
						m_values.Add(fields[i].GetValue(null) as string);
					}
				}
			}
		}
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	public void OnEnable() {
		// Init dropdown
		if(m_playerSourceDropdown != null) {
			// Current value
			string playerSource = TrackingPersistenceSystem.PLAYER_SOURCE_UNDEFINED;
			if(HDTrackingManager.Instance.TrackingPersistenceSystem != null) {
				playerSource = HDTrackingManager.Instance.TrackingPersistenceSystem.PlayerSource;
			}

			// Define options
			int initialSelection = 0;
			List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
			for(int i = 0; i < m_options.Count; ++i) {
				options.Add(new TMP_Dropdown.OptionData(m_options[i]));

				// Is it the current option?
				if(playerSource == m_values[i]) {
					initialSelection = i;
				}
			}
			m_playerSourceDropdown.AddOptions(options);
			m_playerSourceDropdown.value = initialSelection;
		
			// Detect changes
			if(m_playerSourceDropdown != null) m_playerSourceDropdown.onValueChanged.AddListener(OnPlayerSourceChanged);
		}
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from events
		if(m_playerSourceDropdown != null) m_playerSourceDropdown.onValueChanged.RemoveListener(OnPlayerSourceChanged);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The cluster id has changed.
	/// </summary>
	/// <param name="_newValueIdx">Index of the new selected option.</param>
	public void OnPlayerSourceChanged(int _newValueIdx) {
		// Nothing to do if persistence is not initialized
		if(HDTrackingManager.Instance.TrackingPersistenceSystem == null) return;

		// Just assign new source
		HDTrackingManager.Instance.TrackingPersistenceSystem.PlayerSource = m_values[_newValueIdx];
	}
}