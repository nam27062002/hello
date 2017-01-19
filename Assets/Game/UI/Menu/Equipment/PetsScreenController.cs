// PetsScreenController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/07/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class PetsScreenController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] TabSystem m_categoryTabs = null;

	// Internal
	private Dictionary<string, PetCategoryTab> m_tabsByCategory = null;

	// Internal references
	private NavigationShowHideAnimator m_animator = null;
	private NavigationShowHideAnimator animator {
		get { 
			if(m_animator == null) {
				m_animator = GetComponent<NavigationShowHideAnimator>();
			}
			return m_animator;
		}
	}

	// Cache some data for faster access
	private DragonData m_dragonData = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to animator's events
		animator.OnShowPreAnimation.AddListener(OnShowPreAnimation);
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from animator's events
		animator.OnShowPreAnimation.RemoveListener(OnShowPreAnimation);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Setup the screen with a specific pet selected.
	/// </summary>
	/// <param name="_initialPetSku">The pet to focus. Leave empty to load current setup.</param>
	public void Initialize(string _initialPetSku = "") {
		// If not done yet, fill the tabs dictionary
		if(m_tabsByCategory == null) {
			m_tabsByCategory = new Dictionary<string, PetCategoryTab>();
			foreach(NavigationScreen screen in m_categoryTabs.screens) {
				PetCategoryTab tab = (PetCategoryTab)screen;
				m_tabsByCategory.Add(tab.screenName, tab);	// [AOC] Screen name is set from the editor and it matches the category IDs
			}
		}

		// Store reference to target dragon data for faster access
		MenuSceneController menuController = InstanceManager.GetSceneController<MenuSceneController>();
		m_dragonData = DragonManager.GetDragonData(menuController.selectedDragon);

		// Initialize all tabs one by one
		foreach(KeyValuePair<string, PetCategoryTab> kvp in m_tabsByCategory) {
			kvp.Value.Init(kvp.Key, m_dragonData);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Screen is about to be open.
	/// </summary>
	/// <param name="_animator">The animator that triggered the event.</param>
	public void OnShowPreAnimation(ShowHideAnimator _animator) {
		// Refresh with initial data!
		Initialize();
	}
}