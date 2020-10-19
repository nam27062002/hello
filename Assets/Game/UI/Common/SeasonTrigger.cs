// SeasonActivator.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 27/11/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple component to enable/disable game objects during specific seasons.
/// </summary>
public class SeasonTrigger : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum Action {
		ACTIVATE,
		DESTROY
	}

	[System.Serializable]
	public class SeasonalObject {
		public GameObject obj = null;
		public Action action = Action.ACTIVATE;
	}

	[System.Serializable]
	public class SeasonData {
		public string sku = SeasonManager.NO_SEASON_SKU;
		public SeasonalObject[] targets = new SeasonalObject[0];
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private SeasonData[] m_seasonsData = new SeasonData[0];

	// Internal
	// Store in a dictionary for convenience
	private Dictionary<string, SeasonalObject[]> m_seasons = new Dictionary<string, SeasonalObject[]>();
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Initialize dictionary for convenience
		for(int i = 0; i < m_seasonsData.Length; ++i) {
			m_seasons[m_seasonsData[i].sku] = m_seasonsData[i].targets;
		}

		// Apply!
		Apply();
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Apply seasonal trigger based on current season.
	/// </summary>
	public void Apply() {
		// Only if component is enabled!
		if(!this.enabled) return;

		// Cache aux vars
		string currentSeason = SeasonManager.activeSeason;

		// Traverse all seasons and decide which objects to activate and which to destroy
		HashSet<GameObject> toActivate = new HashSet<GameObject>();
		HashSet<GameObject> toDestroy = new HashSet<GameObject>();
		HashSet<GameObject> processedObjs = new HashSet<GameObject>();

		// Current season has priority
		SeasonalObject[] season = null;
		if(m_seasons.TryGetValue(currentSeason, out season)) {
			for(int i = 0; i < season.Length; ++i) {
				// Skip if target object is null
				if(season[i].obj == null) continue;

				switch(season[i].action) {
					case Action.ACTIVATE: {
						toActivate.Add(season[i].obj);
					} break;

					case Action.DESTROY: {
						toDestroy.Add(season[i].obj);
					} break;
				}
				processedObjs.Add(season[i].obj);
			}
		}

		// Process the rest of seasons
		foreach(KeyValuePair<string, SeasonalObject[]> kvp in m_seasons) {
			// Skip current season (already processed)
			if(kvp.Key == currentSeason) continue;

			// Process all objects in the target season
			foreach(SeasonalObject obj in kvp.Value) {
				// Skip if target object is null
				if(obj.obj == null) continue;

				// Skip if already processed
				if(processedObjs.Contains(obj.obj)) continue;

				// Process! For non-current seasons, just make sure objects to be activated in those seasons are destroyed
				if(obj.action == Action.ACTIVATE) {
					toDestroy.Add(obj.obj);
				}
				processedObjs.Add(obj.obj);
			}
		}

		// Processing done! Do actual stuff
		foreach(GameObject obj in toActivate) {
			obj.SetActive(true);
		}

		foreach(GameObject obj in toDestroy) {
			// Destroy game object (we wont have a season change in a single session, so we won't need this object anymore
			DestroyObject(obj);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}