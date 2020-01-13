// SeasonTriggerInstanciator.cs
// Hungry Dragon
// 
// Created by Miguel Ángel Linares on 18/12/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple component to instanciate game objects during specific seasons.
/// </summary>
public class SeasonTriggerInstanciator : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
    [System.Serializable]
    public class OnInstantiateEvent : UnityEvent<GameObject> { };
    public OnInstantiateEvent m_onInstantiateEvent = new OnInstantiateEvent();
    

	[System.Serializable]
	public class SeasonalObject {
		public Transform root = null;
        [FileList("Resources/", StringUtils.PathFormat.RESOURCES_ROOT_WITHOUT_EXTENSION, "*.prefab")]
        public string resource = "";

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

	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Apply!
		Apply();
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

        int max = m_seasonsData.Length;
        for (int i = 0; i < max; i++)
        {
            if ( m_seasonsData[i].sku == currentSeason )
            {
                // Instantiate objects
                int length =  m_seasonsData[i].targets.Length;
                for (int j = 0; j < length; j++)
                {
                    SeasonalObject seasonalObject = m_seasonsData[i].targets[j];
					Transform root = seasonalObject.root;
					// Avoid not having root or it will stay on the scene forever
					if ( root == null )
					{
						root = transform;
					}
                    GameObject go = Instantiate(Resources.Load(seasonalObject.resource), root) as GameObject;
                    m_onInstantiateEvent.Invoke(go);
                }
            }
        }
	}
}