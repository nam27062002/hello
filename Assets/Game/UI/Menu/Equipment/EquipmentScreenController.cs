// EquipmentScreenController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 08/07/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
[RequireComponent(typeof(TabSystem))]
[RequireComponent(typeof(NavigationShowHideAnimator))]
public class EquipmentScreenController : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public enum Tab {
		DISGUISES,
		PETS,
		PHOTO
	};
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private DisguisesScreenController m_disguisesScreen = null;
	[SerializeField] private PetsScreenController m_petsScreen = null;

	// Internal references
	private TabSystem m_tabs = null;
	private TabSystem tabs {
		get { 
			if(m_tabs == null) {
				m_tabs = GetComponent<TabSystem>();
			}
			return m_tabs;
		}
	}

	private NavigationShowHideAnimator m_animator = null;
	private NavigationShowHideAnimator animator {
		get { 
			if(m_animator == null) {
				m_animator = GetComponent<NavigationShowHideAnimator>();
			}
			return m_animator;
		}
	}

	// Initial setup
	private string m_initialDragonSku = "";
	private string m_previouslySelectedDragonSku = "";	// Store it to scroll back to it when leaving the screen
	private string m_initialDisguiseSku = "";
	private string m_initialPetSku = "";
	private Tab m_initialTab = Tab.DISGUISES;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to animator's events
		animator.OnShowPreAnimation.AddListener(OnShowPreAnimation);
		animator.OnHidePostAnimation.AddListener(OnHidePostAnimation);
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Setup the screen before opening it.
	/// Can be called while open as well, but might result in weird visual behaviour.
	/// </summary>
	/// <param name="_dragonSku">Dragon sku. Leave empty for current dragon. If there is a mismatch with <paramref name="_disguiseSku"/> associated dragon, the latter will be used.</param>
	/// <param name="_disguiseSku">Disguise sku. Leave empty for current disguise on target dragon. If there is a mismatch with the disguise's associated dragon and <paramref name="_dragonSku"/>, the first will be used.</param>
	/// <param name="_petSku">Pet sku. Leave empty for current pet on target dragon.</param>
	/// <param name="_initialTab">Initial tab to be displayed. Use <c>NONE</c> to keep last active tab.</param>
	public void Setup(string _dragonSku, string _disguiseSku, string _petSku, Tab _initialTab) {
		// Store initial vars
		m_initialDragonSku = _dragonSku;
		m_initialDisguiseSku = _disguiseSku;
		m_initialPetSku = _petSku;
		m_initialTab = _initialTab;

		// If a valid disguise is defined, it overrides target dragon
		DefinitionNode disguiseDef = DefinitionsManager.SharedInstance.GetDefinition(DefinitionsCategory.DISGUISES, _disguiseSku);
		if(disguiseDef != null) {
			m_initialDragonSku = disguiseDef.Get("dragonSku");
		}

		// If screen is open, instantly refresh
		// Otherwise the OnShowPreAnimation will take care of it
		if(animator.visible) {
			Initialize();
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Focus target dragon, load initial disguise and go to target tab!
	/// </summary>
	private void Initialize() {
		// Select target dragon (if any)
		if(!string.IsNullOrEmpty(m_initialDragonSku)) {
			// Store currently selected dragon to restore selection when leaving
			// Unless we already have a target dragon to go to when leaving
			MenuSceneController menuController = InstanceManager.GetSceneController<MenuSceneController>();
			if(string.IsNullOrEmpty(m_previouslySelectedDragonSku)) {
				m_previouslySelectedDragonSku = menuController.selectedDragon;
			}

			// Select target dragon - dragon scroller should scroll to it
			menuController.SetSelectedDragon(m_initialDragonSku);
		}

		// Initialize disguises screen with the target disguise
		m_disguisesScreen.Initialize(m_initialDisguiseSku);

		// Initialize pets screen with the target pet
		//m_petsScreen.Initialize(m_initialPetSku);

		// Switch to initial tab
		// If screen is open, use animation
		// Otherwise, the animation will be triggered by the Equipment Screen's NavigationScreen component
		if(animator.visible) {
			tabs.GoToScreen((int)m_initialTab);
		} else {
			tabs.GoToScreen((int)m_initialTab, NavigationScreen.AnimType.NONE);
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

	/// <summary>
	/// Screen has just closed
	/// </summary>
	/// <param name="_animator">The animator that triggered the event.</param>
	public void OnHidePostAnimation(ShowHideAnimator _animator) {
		// Make sure screens are properly finalized
		m_disguisesScreen.Finalize();

		// Scroll back to previously selected dragon if different than current one
		MenuSceneController menuController = InstanceManager.GetSceneController<MenuSceneController>();
		if(menuController.selectedDragon != m_previouslySelectedDragonSku) {
			// Select target dragon
			menuController.SetSelectedDragon(m_previouslySelectedDragonSku);
		}

		// Clear internal data
		m_initialDragonSku = "";
		m_previouslySelectedDragonSku = "";
		m_initialDisguiseSku = "";
		m_initialTab = Tab.DISGUISES;	// Always show disguises tab by default
	}
}