// MenuDragonShowIfOwned.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Simple behaviour to show/hide the target game object depending on the 
/// selected dragon and whether it's owned or not.
/// </summary>
[RequireComponent(typeof(ShowHideAnimator))]
public class MenuDragonShowConditionally : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	private enum HideForDragons {
		NONE,
		FIRST,
		LAST,
		FIRST_AND_LAST,
		ALL
	}

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Config
	[Comment("Ownership of the selected dragon", 5f)]
	[SerializeField] private bool m_showIfLocked = false;
	[SerializeField] private bool m_showIfAvailable = false;
	[SerializeField] private bool m_showIfOwned = true;

	[Comment("Will override ownership status for those dragons", 5f)]
	[SerializeField] private HideForDragons m_hideForDragons;

	// Internal references
	private ShowHideAnimator m_animator = null;

	// Internal
	private List<DragonDef> m_sortedDefs = null;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external references
		m_animator = GetComponent<ShowHideAnimator>();
	}

	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Get sorted defs
		m_sortedDefs = DefinitionsManager.dragons.defsListByMenuOrder;

		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.AddListener<DragonData>(GameEvents.DRAGON_ACQUIRED, OnDragonAcquired);

		// Apply for the first time with currently selected dragon and without animation
		Apply(InstanceManager.GetSceneController<MenuSceneController>().selectedDragon, false);
	}
	
	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.RemoveListener<DragonData>(GameEvents.DRAGON_ACQUIRED, OnDragonAcquired);
	}

	/// <summary>
	/// Changes dragon selected to the given one.
	/// </summary>
	/// <param name="_sku">The sku of the dragon we want to be the current one.</param>
	public void SetSelectedDragon(string _sku) {
		// Notify game
		Messenger.Broadcast<string>(GameEvents.MENU_DRAGON_SELECTED, _sku);
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Apply visibility based on given parameters.
	/// </summary>
	/// <param name="_sku">The sku of the dragon to be considered.</param>
	/// <param name="_useAnims">Whether to animate or not.</param>
	private void Apply(string _sku, bool _useAnims) {
		// Check whether the object should be visible or not
		bool toShow = false;
		DragonData dragon = DragonManager.GetDragonData(_sku);

		// Ownership status
		switch(dragon.lockState) {
			case DragonData.LockState.LOCKED:		toShow = m_showIfLocked;	break;
			case DragonData.LockState.AVAILABLE:	toShow = m_showIfAvailable;	break;
			case DragonData.LockState.OWNED:		toShow = m_showIfOwned;		break;
		}

		// Dragon ID (overrides ownership status)
		switch(m_hideForDragons) {
			case HideForDragons.NONE:	break;	// Nothing to change
			case HideForDragons.FIRST:	toShow &= (dragon.def.menuOrder != 0);	break;
			case HideForDragons.LAST:	toShow &= (dragon.def.menuOrder != (m_sortedDefs.Count - 1));	break;
			case HideForDragons.FIRST_AND_LAST:	{
				toShow &= (dragon.def.menuOrder != 0);
				toShow &= (dragon.def.menuOrder != (m_sortedDefs.Count - 1));
			} break;
			case HideForDragons.ALL:	toShow = false;	break;	// Force hiding
		}

		// Let animator do its magic
		m_animator.Set(toShow, _useAnims);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The selected dragon on the menu has changed.
	/// </summary>
	/// <param name="_sku">The sku of the newly selected dragon.</param>
	public void OnDragonSelected(string _sku) {
		// Just update visibility
		Apply(_sku, true);
	}

	/// <summary>
	/// A dragon has been acquired
	/// </summary>
	/// <param name="_data">The data of the acquired dragon.</param>
	public void OnDragonAcquired(DragonData _data) {
		// It should be the selected dragon, but check anyway
		if(_data.def.sku != InstanceManager.GetSceneController<MenuSceneController>().selectedDragon) {
			return;
		}

		// Update visibility
		Apply(_data.def.sku, true);
	}
}

