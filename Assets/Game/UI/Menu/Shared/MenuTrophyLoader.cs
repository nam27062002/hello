// MenuTrophyLoader.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/01/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar component to easily load a 3D league trophy into the UI.
/// </summary>
public class MenuTrophyLoader : MonoBehaviour {
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SkuList(DefinitionsCategory.LEAGUES)]
	[SerializeField] private string m_leagueSku = "";
	public string leagueSku {
		get { return m_leagueSku; }
	}

	// Internal
	private MenuTrophyPreview m_trophyInstance = null;
	public MenuTrophyPreview trophyInstance {
		get { return m_trophyInstance; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Intialization.
	/// </summary>
	private void Awake() {
		// Trophy previews are not serializable, so we can't reuse any existing trophy
		Unload();
	}

	/// <summary>
	/// Initialization.
	/// </summary>
	private void OnEnable() {
		// If a league sku was defined from inspector, load it now (unless another one is already loaded)
		if(!string.IsNullOrEmpty(m_leagueSku) && m_trophyInstance == null) {
			Load(m_leagueSku);
		}
	}

	/// <summary>
	/// Default destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Load the given league's trophy preview.
	/// If another trophy was loaded, it will be unloaded.
	/// </summary>
	/// <param name="_leagueSku">The sku of the league whose trophy we want to display. <c>null</c> to unload any active preview.</param>
	public void Load(string _leagueSku) {
		// Skip if already loaded
		if(m_trophyInstance != null && _leagueSku == m_leagueSku) return;

		// Unload previous trophy
		Unload();

		// Store new sku
		m_leagueSku = _leagueSku;

		// Load target trophy
		DefinitionNode leagueDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.LEAGUES, _leagueSku);
		if(leagueDef != null) {
			// Compose prefab path
			string pathToPrefab = UIConstants.LEAGUE_ICONS_PATH + leagueDef.GetAsString("prefabName");

			// Instantiate the prefab and add it as child of this object
			GameObject trophyPrefab = Resources.Load<GameObject>(pathToPrefab);
			if(trophyPrefab != null) {
				GameObject newInstance = Instantiate<GameObject>(trophyPrefab);
				newInstance.transform.SetParent(this.transform, false);
				newInstance.transform.localPosition = Vector3.zero;
				newInstance.transform.localRotation = Quaternion.identity;
				newInstance.SetLayerRecursively(this.gameObject.layer);

				// Get trophy controller
				m_trophyInstance = newInstance.GetComponentInChildren<MenuTrophyPreview>();

				// [AOC] CHECK!! Rescale particles?
			}
		}
	}

	/// <summary>
	/// Destroy current loaded egg, if any.
	/// </summary>
	public void Unload() {
		// Destroy all childs of the loader and clear references
		while(transform.childCount > 0) {
			DestroyImmediate(transform.GetChild(0).gameObject);  // Immediate so it can be called from the editor
		}
		m_trophyInstance = null;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}