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
using System.Collections;
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
	[Comment("Leave empty to use the current selected dragon on the menu as target.")]
	[SkuList(DefinitionsCategory.DRAGONS, true)]
	[SerializeField] private string m_targetDragonSku = "";

	[Comment("Ownership of the selected dragon", 5f)]
	[SerializeField] private bool m_showIfLocked = false;
	[SerializeField] private bool m_showIfAvailable = false;
	[SerializeField] private bool m_showIfOwned = true;

	[Comment("Will override ownership status for those dragons", 5f)]
	[SerializeField] private HideForDragons m_hideForDragons;

	[Comment("Animation options")]
	[SerializeField] private bool m_restartShowAnimation = false;

	// Internal references
	private ShowHideAnimator m_animator = null;

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
		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.AddListener<DragonData>(GameEvents.DRAGON_ACQUIRED, OnDragonAcquired);

		// Apply for the first time with currently selected dragon and without animation
		Apply(InstanceManager.GetSceneController<MenuSceneController>().selectedDragon, false, false);
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

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Apply visibility based on given parameters.
	/// </summary>
	/// <param name="_sku">The sku of the dragon to be considered.</param>
	/// <param name="_useAnims">Whether to animate or not.</param>
	/// <param name="_resetAnim">Optionally force the animation to be played, even if going to the same state.</param>
	private void Apply(string _sku, bool _useAnims, bool _resetAnim) {
		// Check whether the object should be visible or not
		bool toShow = false;
		DragonData dragon = DragonManager.GetDragonData(_sku);
		if(dragon == null) return;

		// Ownership status
		switch(dragon.lockState) {
			case DragonData.LockState.LOCKED:		toShow = m_showIfLocked;	break;
			case DragonData.LockState.AVAILABLE:	toShow = m_showIfAvailable;	break;
			case DragonData.LockState.OWNED:		toShow = m_showIfOwned;		break;
		}

		// Dragon ID (overrides ownership status)
		int dragonIdx = dragon.def.GetAsInt("order");
		switch(m_hideForDragons) {
			case HideForDragons.NONE:	break;	// Nothing to change
			case HideForDragons.FIRST:	toShow &= (dragonIdx != 0);	break;
			case HideForDragons.LAST:	toShow &= (dragonIdx != (DragonManager.dragonsByOrder.Count - 1));	break;
			case HideForDragons.FIRST_AND_LAST:	{
				toShow &= (dragonIdx != 0);
				toShow &= (dragonIdx != (DragonManager.dragonsByOrder.Count - 1));
			} break;
			case HideForDragons.ALL:	toShow = false;	break;	// Force hiding
		}

		// Let animator do its magic
		// If forcing animation and going from visible to visible, hide before showing again
		if(_resetAnim && toShow && m_animator.visible && isActiveAndEnabled && m_animator.tweenType != ShowHideAnimator.TweenType.NONE) {
			// Go to opposite of the target state
			m_animator.Hide(_useAnims, false);

			// Program the animation to the target state in sync with the dragon scroll animation (more or less)
			StartCoroutine(LaunchDelayedAnimation(toShow, _useAnims));
		} else {
			m_animator.Set(toShow, _useAnims);
		}
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
		Apply(string.IsNullOrEmpty(m_targetDragonSku) ? _sku : m_targetDragonSku, true, m_restartShowAnimation);
	}

	/// <summary>
	/// A dragon has been acquired
	/// </summary>
	/// <param name="_data">The data of the acquired dragon.</param>
	public void OnDragonAcquired(DragonData _data) {
		// Is it our target dragon?
		string targetSku = m_targetDragonSku;
		if(string.IsNullOrEmpty(targetSku)) {
			targetSku = InstanceManager.GetSceneController<MenuSceneController>().selectedDragon;
		}

		// It should be the selected dragon, but check anyway
		if(_data.def.sku != targetSku) {
			return;
		}

		// Update visibility
		Apply(targetSku, true, false);
	}

	/// <summary>
	/// Use it to set the animator's visibility after a delay via StartCoroutine().
	/// </summary>
	/// <returns>The coroutine function.</returns>
	/// <param name="_toShow">Whether to show or hide the object.</param>
	/// <param name="_useAnims">Whether to use anims or not.</param>
	private IEnumerator LaunchDelayedAnimation(bool _toShow, bool _useAnims) {
		// Delay
		yield return new WaitForSeconds(m_animator.tweenDuration);

		// Do it!
		m_animator.Set(_toShow, _useAnims);
	}
}

