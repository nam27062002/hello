// MenuShowConditionally.cs
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
/// Simple behaviour to show/hide the target game object depending on several
/// conditions.
/// </summary>
[RequireComponent(typeof(ShowHideAnimator))]
public class MenuShowConditionally : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum HideForDragons {
		IGNORE,
		FIRST,	// Hide for first dragon
		LAST,	// Hide for last dragon
		FIRST_AND_LAST,	// Hide for first and last dragons
		ALL,	// Hide for all dragons
		NONE	// Never hide based on dragon
	}

	public enum ScreenVisibilityMode {
		SHOW_ON_TARGET_SCREENS,
		HIDE_ON_TARGET_SCREENS
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Config
	// Dragon-based visibility
	[SerializeField] private bool m_checkSelectedDragon = false;

	[Comment("<color=orange>PER DRAGON ORDER:</color> Higher priority than ownership status (unless set to IGNORE)")]
	[SerializeField] private HideForDragons m_hideForDragons = HideForDragons.IGNORE;

	[Comment("<color=orange>PER DRAGON OWNERSHIP:</color> Ownership of the target dragon", 5f)]
	[Comment("Leave target dragon sku empty to use the current <color=green>selected</color> dragon on the menu as target.")]
	[SkuList(DefinitionsCategory.DRAGONS, true)]
	[SerializeField] private string m_targetDragonSku = "";
	[SerializeField] private bool m_showIfLocked = false;
	[SerializeField] private bool m_showIfAvailable = false;
	[SerializeField] private bool m_showIfOwned = true;

	// Screen-based visibility
	[SerializeField] private bool m_checkScreens = false;
	[SerializeField] private ScreenVisibilityMode m_mode = ScreenVisibilityMode.HIDE_ON_TARGET_SCREENS;
	[SerializeField] private List<MenuScreens> m_screens = new List<MenuScreens>();

	// Animation options
	[SerializeField] private bool m_restartShowAnimation = false;

	// Internal references
	private ShowHideAnimator m_animator = null;

	// Extra Properties
	public string targetDragonSku {
		get {
			// If no specific dragon is defined, use currently selected dragon
			if(string.IsNullOrEmpty(m_targetDragonSku)) {
				return InstanceManager.menuSceneController.selectedDragon;
			}
			return m_targetDragonSku;
		}
	}

	private MenuScreens currentMenuScreen {
		get { return InstanceManager.menuSceneController.screensController.currentMenuScreen; }
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get external references
		m_animator = GetComponent<ShowHideAnimator>();

		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.AddListener<DragonData>(GameEvents.DRAGON_ACQUIRED, OnDragonAcquired);
		Messenger.AddListener<NavigationScreenSystem.ScreenChangedEventData>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnScreenChanged);
	}

	/// <summary>
	/// First update.
	/// </summary>
	private void Start() {
		// Apply for the first time with current values and without animation
		Apply(targetDragonSku, currentMenuScreen, false, false);
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
		Messenger.RemoveListener<NavigationScreenSystem.ScreenChangedEventData>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnScreenChanged);
	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Check whether the object must be visible with a given param set.
	/// </summary>
	/// <returns>Whether the visibility checks are passed or not with the given parameters.</returns>
	/// <param name="_dragonSku">Dragon sku to be considered.</param>
	/// <param name="_screen">Menu screen to be considered.</param>
	public bool Check(string _dragonSku, MenuScreens _screen) {
		// Both conditions must be satisfied
		return CheckDragon(_dragonSku) && CheckScreen(_screen);
	}

	/// <summary>
	/// Check whether the object must be visible with a given dragon.
	/// </summary>
	/// <returns>Whether the visibility checks are passed or not with the given parameters.</returns>
	/// <param name="_dragonSku">Dragon sku to be considered.</param>
	public bool CheckDragon(string _dragonSku) {
		// Skip if dragon check disabled
		if(!m_checkSelectedDragon) return true;

		// Check whether the object should be visible or not
		bool show = false;
		DragonData dragon = DragonManager.GetDragonData(_dragonSku);
		if(dragon == null) return true;

		// Ownership status
		switch(dragon.lockState) {
			case DragonData.LockState.LOCKED:		show = m_showIfLocked;		break;
			case DragonData.LockState.AVAILABLE:	show = m_showIfAvailable;	break;
			case DragonData.LockState.OWNED:		show = m_showIfOwned;		break;
		}

		// Dragon ID (overrides ownership status)
		int dragonIdx = dragon.def.GetAsInt("order");
		switch(m_hideForDragons) {
			case HideForDragons.IGNORE:	break;	// Nothing to change
			case HideForDragons.FIRST:	show &= (dragonIdx != 0);	break;
			case HideForDragons.LAST:	show &= (dragonIdx != (DragonManager.dragonsByOrder.Count - 1));	break;
			case HideForDragons.FIRST_AND_LAST:	{
				show &= (dragonIdx != 0);
				show &= (dragonIdx != (DragonManager.dragonsByOrder.Count - 1));
			} break;
			case HideForDragons.ALL:	show = false;	break;	// Force hiding
			case HideForDragons.NONE:	show = true;	break;	// Force showing
		}

		return show;
	}

	/// <summary>
	/// Check whether the object must be visible with a given screen.
	/// </summary>
	/// <returns>Whether the visibility checks are passed or not with the given parameters.</returns>
	/// <param name="_screen">Menu screen to be considered.</param>
	public bool CheckScreen(MenuScreens _screen) {
		// Skip if screen check disabled
		if(!m_checkScreens) return true;

		// Is screen in the list?
		bool isScreenOnTheList = m_screens.IndexOf(_screen) >= 0;

		// Determine visibility
		bool show = true;
		switch(m_mode) {
			case ScreenVisibilityMode.SHOW_ON_TARGET_SCREENS: {
				show = isScreenOnTheList;
			} break;

			case ScreenVisibilityMode.HIDE_ON_TARGET_SCREENS: {
				show = !isScreenOnTheList;
			} break;
		}

		return show;
	}

	//------------------------------------------------------------------//
	// INTERNAL METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Apply visibility based on given parameters.
	/// </summary>
	/// <param name="_show">Whether to show or hide.</param>
	/// <param name="_useAnims">Whether to animate or not.</param>
	/// <param name="_resetAnim">Optionally force the animation to be played, even if going to the same state.</param>
	private void Apply(bool _show, bool _useAnims, bool _resetAnim) {
		// Let animator do its magic
		// If forcing animation and going from visible to visible, hide before showing again
		if(_resetAnim 
		&& _show 
		&& m_animator.visible 
		&& isActiveAndEnabled 
		&& m_animator.tweenType != ShowHideAnimator.TweenType.NONE) {
			// Go to opposite of the target state
			m_animator.Hide(_useAnims, false);

			// Program the animation to the target state in sync with the dragon scroll animation (more or less)
			StartCoroutine(LaunchDelayedAnimation(_show, _useAnims));
		} else {
			m_animator.Set(_show, _useAnims);
		}
	}

	/// <summary>
	/// Apply visibility based on given parameters.
	/// </summary>
	/// <param name="_dragonSku">Dragon sku to be considered.</param>
	/// <param name="_screen">Menu screen to be considered.</param>
	/// <param name="_useAnims">Whether to animate or not.</param>
	/// <param name="_resetAnim">Optionally force the animation to be played, even if going to the same state.</param>
	private void Apply(string _dragonSku, MenuScreens _screen, bool _useAnims, bool _resetAnim) {
		Apply(Check(_dragonSku, _screen), _useAnims, m_restartShowAnimation);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// The selected dragon on the menu has changed.
	/// </summary>
	/// <param name="_sku">The sku of the newly selected dragon.</param>
	public void OnDragonSelected(string _sku) {
		// Ignore if component not enabled
		if(!this.enabled) return;

		// Just update visibility
		Apply(
			string.IsNullOrEmpty(m_targetDragonSku) ? _sku : m_targetDragonSku,	// MenuSceneController.selectedDragon is not necessarily updated yet
			currentMenuScreen,
			true, m_restartShowAnimation
		);
	}

	/// <summary>
	/// A dragon has been acquired
	/// </summary>
	/// <param name="_data">The data of the acquired dragon.</param>
	public void OnDragonAcquired(DragonData _data) {
		// Ignore if component not enabled
		if(!this.enabled) return;

		// Is it our target dragon?
		// It should be the selected dragon, but check anyway
		if(_data.def.sku != targetDragonSku) {
			return;
		}

		// Update visibility
		Apply(targetDragonSku, currentMenuScreen, true, false);
	}

	/// <summary>
	/// A navigation screen has changed.
	/// </summary>
	/// <param name="_data">The event's data.</param>
	public void OnScreenChanged(NavigationScreenSystem.ScreenChangedEventData _data) {
		// Ignore if component not enabled
		if(!this.enabled) return;

		// Is it the main menu screen system?
		if(_data.dispatcher == InstanceManager.menuSceneController.screensController) {
			// Refresh
			Apply(targetDragonSku, currentMenuScreen, true, false);
		}
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

		// Do it! (If still enabled!)
		if(this.enabled) m_animator.Set(_toShow, _useAnims);
	}
}

