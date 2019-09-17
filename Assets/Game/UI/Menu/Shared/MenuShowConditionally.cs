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
using UnityEngine.Serialization;
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

	private struct VisibilitySetup {
		public bool show;					// Show or hide?
		public bool delayed;				// Delay?
		public bool allowExternalChecks;    // Allow external checks?

		public bool animate;				// Animate?
		public bool restartAnimation;		// Restart the animation even if in the same state?
		public bool disableAfterAnimation;	// If hiding, disable object after the animation?

		public static VisibilitySetup Default() {
			return new VisibilitySetup { 
				show = true, 
				delayed = false, 
				allowExternalChecks = true, 

				animate = true, 
				restartAnimation = false, 
				disableAfterAnimation = true 
			};
		}
	}

	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed references
	[SerializeField] private ShowHideAnimator m_targetAnimator = null;
	public ShowHideAnimator targetAnimator {
		get { return m_targetAnimator; }
	}

	// Config
	// Dragon-based visibility
	[SerializeField] private bool m_checkSelectedDragon = false;

	[Comment("<color=orange>PER DRAGON ORDER:</color> Higher priority than ownership status (unless set to IGNORE)")]
	[SerializeField] private HideForDragons m_hideForDragons = HideForDragons.IGNORE;

    [Comment("<color=orange>PER TYPE:</color>")]
    [SerializeField] private IDragonData.Type m_showOnlyForType = IDragonData.Type.ALL;

    [Comment("<color=orange>PER DRAGON OWNERSHIP:</color> Ownership of the target dragon", 5f)]
	[Comment("Leave target dragon sku empty to use the current <color=green>selected</color> dragon on the menu as target.")]
	[SkuList(DefinitionsCategory.DRAGONS, true)]
	[SerializeField] private string m_targetDragonSku = "";
	[SerializeField] private bool m_showIfShadow = false;
	[SerializeField] private bool m_showIfLocked = false;
	[SerializeField] private bool m_showIfAvailable = false;
	[SerializeField] private bool m_showIfOwned = true;

	// Screen-based visibility
	[SerializeField] private bool m_checkScreens = false;
	[SerializeField] private ScreenVisibilityMode m_mode = ScreenVisibilityMode.HIDE_ON_TARGET_SCREENS;
	[SerializeField] private List<MenuScreen> m_targetScreens = new List<MenuScreen>();

	// Animation options
	[FormerlySerializedAs("m_delay")]
	[SerializeField] private float m_hideDelay = 0f;
	[SerializeField] private float m_showDelay = 0f;
	[SerializeField] private bool m_restartShowAnimation = false;

	// Events
	public delegate bool CheckDelegate(string _dragonSku, MenuScreen _screen);
	public HashSet<CheckDelegate> externalChecks = new HashSet<CheckDelegate>();

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

	private MenuScreen currentMenuScreen {
		get { return InstanceManager.menuSceneController.currentScreen; }
	}

	// Internal
	private Coroutine m_coroutine;
	private bool m_allowExternalChecks = true;
	private bool m_firstEnablePassed = false;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to external events
		Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.AddListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquired);
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnScreenChanged);

		// The animator must ask for permission before showing itself!
		if(m_targetAnimator != null) {
			m_targetAnimator.OnShowCheck.AddListener(OnAnimatorCheck);
		}

		// Start hidden (the Start call will properly initialize it based on current values)
		// Force a hide and then apply for the first time with current values and without animation
		VisibilitySetup setup = VisibilitySetup.Default();
		setup.show = false;
		setup.animate = false;
		Apply(setup);
		Apply(targetDragonSku, currentMenuScreen, setup);

		m_coroutine = null;
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Don't animate the first time the object is enabled
		VisibilitySetup setup = VisibilitySetup.Default();
		setup.animate = m_firstEnablePassed;
		Apply(targetDragonSku, currentMenuScreen, setup);
		m_firstEnablePassed = true;
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.RemoveListener<IDragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquired);
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnScreenChanged);

		if(m_targetAnimator != null) {
			m_targetAnimator.OnShowCheck.RemoveListener(OnAnimatorCheck);
		}
	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Check whether the obejct must be visible with the current dragon and menu screen.
	/// </summary>
	/// <returns>Whether the visibility checks are passed or not with the current dragon and menu screen..</returns>
	public bool Check() {
		return Check(targetDragonSku, currentMenuScreen);
	}

	/// <summary>
	/// Check whether the object must be visible with a given param set.
	/// </summary>
	/// <returns>Whether the visibility checks are passed or not with the given parameters.</returns>
	/// <param name="_dragonSku">Dragon sku to be considered.</param>
	/// <param name="_screen">Menu screen to be considered.</param>
	public bool Check(string _dragonSku, MenuScreen _screen) {
		// All conditions must be satisfied
		// No need to check the rest if one condition already fails
		if(!CheckDragon(_dragonSku)) return false;
		if(!CheckScreen(_screen)) return false;

		// External checks
		foreach(CheckDelegate externalCheck in externalChecks) {
			if(!externalCheck(_dragonSku, _screen)) return false;
		}

		// All checks passed!
		return true;
	}

	/// <summary>
	/// Check whether the object must be visible with a given dragon.
	/// </summary>
	/// <returns>Whether the visibility checks are passed or not with the given parameters.</returns>
	/// <param name="_dragonSku">Dragon sku to be considered.</param>
	public bool CheckDragon(string _dragonSku) {
		// Skip if dragon check disabled
		if(!m_checkSelectedDragon) return true;

		// Debug
		ShowHideAnimator.DebugLog(this, Colors.yellow.Tag("Checking dragon: " + _dragonSku));

		// Check whether the object should be visible or not
		bool show = false;
		IDragonData dragon = DragonManager.GetDragonData(_dragonSku);
		if(dragon == null) return true;

		// Ownership status
		switch(dragon.lockState) {
			case IDragonData.LockState.TEASE:		show = m_showIfShadow;		break;
			case IDragonData.LockState.SHADOW:		show = m_showIfShadow;		break;
			case IDragonData.LockState.LOCKED_UNAVAILABLE: show = m_showIfLocked; break;
			case IDragonData.LockState.LOCKED:		show = m_showIfLocked;		break;
			case IDragonData.LockState.AVAILABLE:	show = m_showIfAvailable;	break;
			case IDragonData.LockState.OWNED:		show = m_showIfOwned;		break;
		}

		// Dragon ID (overrides ownership status)
		switch(m_hideForDragons) {
			case HideForDragons.IGNORE:	break;	// Nothing to change
			case HideForDragons.FIRST:	show &= !DragonManager.IsFirstDragon(dragon.def.sku);	break;
			case HideForDragons.LAST:	show &= !DragonManager.IsLastDragon(dragon.def.sku);	break;
			case HideForDragons.FIRST_AND_LAST:	{
				show &= !DragonManager.IsFirstDragon(dragon.def.sku);
				show &= !DragonManager.IsLastDragon(dragon.def.sku);
			} break;
			case HideForDragons.ALL:	show = false;	break;	// Force hiding
			case HideForDragons.NONE:	show = true;	break;	// Force showing
		}

        // Dragon type
        switch (m_showOnlyForType)
        {
            case IDragonData.Type.ALL: break; // Nothing to change
            default: show &= (m_showOnlyForType == dragon.type); break; // Classic or special?
        }

		return show;
	}

	/// <summary>
	/// Check whether the object must be visible with a given screen.
	/// </summary>
	/// <returns>Whether the visibility checks are passed or not with the given parameters.</returns>
	/// <param name="_screen">Menu screen to be considered.</param>
	public bool CheckScreen(MenuScreen _screen) {
		// Skip if screen check disabled
		if(!m_checkScreens) return true;
		
		// Debug
		ShowHideAnimator.DebugLog(this, Colors.yellow.Tag("Checking screen: " + _screen));

		// Is screen in the list?
		bool isScreenOnTheList = m_targetScreens.IndexOf(_screen) >= 0;

		// Determine visibility
		bool show = true;
		switch(m_mode) {
			case ScreenVisibilityMode.SHOW_ON_TARGET_SCREENS: {
				show = isScreenOnTheList;
			} break;

			case ScreenVisibilityMode.HIDE_ON_TARGET_SCREENS: {
				show = !isScreenOnTheList && _screen != MenuScreen.NONE;	// Never show at "NONE" screen! Resolves issue HDK-1251
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
	/// <param name="_setup">Visibility settings to be applied. <c>delayed</c> value will be overwritten in some cases.</param>
	//private void Apply(bool _show, bool _useAnims, bool _resetAnim) {
	private void Apply(VisibilitySetup _setup) {
		// Debug
		ShowHideAnimator.DebugLog(this, Colors.yellow.Tag("APPLY: " + _setup.show + ", " + _setup.animate + ", " + _setup.restartAnimation));

		// Let animator do its magic
		// If forcing animation and going from visible to visible, hide before showing again
		if(m_targetAnimator != null
		&& _setup.restartAnimation 
		&& _setup.show
		&& m_targetAnimator.visible 
		&& isActiveAndEnabled 
		&& m_targetAnimator.tweenType != ShowHideAnimator.TweenType.NONE) {
			// Debug
			ShowHideAnimator.DebugLog(this, Colors.red.Tag("FORCE_HIDE 1"));

			// Go to opposite of the target state
			// Dont disable if animator parent is the same as this one, otherwise the logic of this behaviour will stop working!
			VisibilitySetup hideSetup = _setup; // struct, will create a new copy
			hideSetup.show = false;
			hideSetup.delayed = false;
			hideSetup.disableAfterAnimation = m_targetAnimator.gameObject != this.gameObject;
			SetVisibility(hideSetup);

			// Delay the animation to the target state in sync with the dragon scroll animation (more or less)
			_setup.delayed = true;
			SetVisibility(_setup);
		} else {
			// Debug
			if(_setup.show) {
				ShowHideAnimator.DebugLog(this, Colors.green.Tag("FORCE_SHOW"));
			} else {
				ShowHideAnimator.DebugLog(this, Colors.red.Tag("FORCE_HIDE 2"));
			}

			// If no animator is defined (and animating), use delays
			if(_setup.animate && m_targetAnimator == null) {
				_setup.delayed = true;
			}
			_setup.allowExternalChecks = false;
			SetVisibility(_setup);
		}
	}

	/// <summary>
	/// Apply visibility based on given parameters.
	/// </summary>
	/// <param name="_dragonSku">Dragon sku to be considered.</param>
	/// <param name="_screen">Menu screen to be considered.</param>
	/// <param name="_setup">Config to be used. <c>show</c> value will be ignored.</param>
	//private void Apply(string _dragonSku, MenuScreen _screen, bool _useAnims, bool _resetAnim) {
	private void Apply(string _dragonSku, MenuScreen _screen, VisibilitySetup _setup) {
		//Apply(Check(_dragonSku, _screen), _useAnims, m_restartShowAnimation);
		_setup.show = Check(_dragonSku, _screen);
		Apply(_setup);
	}

	/// <summary>
	/// Apply the visibility to the target object!
	/// </summary>
	/// <param name="_setup">Visibility parameters.</param>
	//private void SetVisibility(bool _show, bool _useAnims, bool _externalCheckAllowed, bool _delayed, bool _disableAfterAnimation) {
	private void SetVisibility(VisibilitySetup _setup) {
		ShowHideAnimator.DebugLog(this, Colors.orange.Tag("SET VISIBILITY: " 
														  + _setup.show + ", " 
		                                                  + _setup.animate + ", " 
		                                                  + _setup.allowExternalChecks + ", "
		                                                  + _setup.delayed + ", "
		                                                  + _setup.disableAfterAnimation
		                                                 ));

		// Cancel any pending action
		if(m_coroutine != null) {
			StopCoroutine(m_coroutine);
			m_coroutine = null;
		}

		// Delayed?
		if(_setup.delayed) {
			// Yes! Program the coroutine
			//m_coroutine = StartCoroutine(LaunchDelayedAnimation(_setup));
			m_coroutine = LaunchDelayedAnimation(_setup);
		} else {
			// Allow external checks?
			m_allowExternalChecks = _setup.allowExternalChecks;

			// Apply! Do we have an animator?
			if(m_targetAnimator != null) {
				// Yes! Launch it with given parameters
				if(_setup.show) {
					// Show
					m_targetAnimator.ForceShow(_setup.animate);
				} else {
					// Hide
					m_targetAnimator.ForceHide(_setup.animate, _setup.disableAfterAnimation);
				}
			} else {
				// No! Just apply directly to the game object
				this.gameObject.SetActive(_setup.show);
			}


			// Restore external check flag
			m_allowExternalChecks = true;

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
		// Ignore if component not enabled
		if(!this.enabled) {
			return;
		}

		// Just update visibility
		if(m_checkSelectedDragon) {
			VisibilitySetup setup = VisibilitySetup.Default();
			setup.restartAnimation = m_restartShowAnimation;

			Apply(
				string.IsNullOrEmpty(m_targetDragonSku) ? _sku : m_targetDragonSku, // MenuSceneController.selectedDragon is not necessarily updated yet
				currentMenuScreen,
				setup
			);
		}
	}

	/// <summary>
	/// A dragon has been acquired
	/// </summary>
	/// <param name="_data">The data of the acquired dragon.</param>
	public void OnDragonAcquired(IDragonData _data) {
		// Ignore if component not enabled
		if(!this.enabled) {
			return;
		}

		// Is it our target dragon?
		if(_data.def.sku != targetDragonSku) {
			return;
		}

		// Update visibility
		if(m_checkSelectedDragon) {
			Apply(targetDragonSku, currentMenuScreen, VisibilitySetup.Default());
		}
	}

	/// <summary>
	/// A menu screen has changed.
	/// </summary>
	public void OnScreenChanged(MenuScreen _from, MenuScreen _to) {
		// Ignore if component not enabled
		if(!this.enabled) {
			return;
		}

		// Refresh
		if(m_checkScreens) {
			Apply(targetDragonSku, currentMenuScreen, VisibilitySetup.Default());
		}
	}

	/// <summary>
	/// Use it to set the animator's visibility after a delay via StartCoroutine().
	/// </summary>
	/// <returns>The coroutine function.</returns>
	/// <param name="_setup">Config to be used.</param>

	private Coroutine LaunchDelayedAnimation(VisibilitySetup _setup) {
		// How much delay?
		float delay = 0f;
		if(m_targetAnimator != null) {
			delay = m_targetAnimator.tweenDuration;
		} else {
			delay = _setup.show ? m_showDelay : m_hideDelay;
		}

		// Trigger the delayed action
		return UbiBCN.CoroutineManager.DelayedCall(
			() => {
				// Debug
				ShowHideAnimator.DebugLog(this, Colors.yellow.Tag("DELAYED APPLY: " + _setup.show + ", " + _setup.animate + "\nenabled? " + this.enabled));

				// Do it! (If still enabled!)
				if(this.enabled) {
					_setup.allowExternalChecks = false;
					_setup.delayed = false;
					SetVisibility(_setup);
				}

				// Clear coroutine reference
				m_coroutine = null;
			}, delay
		);
	}
	/*
	private IEnumerator LaunchDelayedAnimation(VisibilitySetup _setup) {
		// Delay
		if(m_targetAnimator != null) {
			yield return new WaitForSeconds(m_targetAnimator.tweenDuration);
		} else {
			yield return new WaitForSeconds(_setup.show ? m_showDelay : m_hideDelay);
		}

		// Debug
		ShowHideAnimator.DebugLog(this, Colors.yellow.Tag("DELAYED APPLY: " + _setup.show + ", " + _setup.animate + "\nenabled? " + this.enabled));

		// Do it! (If still enabled!)
		if(this.enabled) {
			_setup.allowExternalChecks = false;
			_setup.delayed = false;
			SetVisibility(_setup);
		}

		m_coroutine = null;
	}
	*/

	/// <summary>
	/// An animator is checking whether it can be displayed.
	/// </summary>
	/// <param name="_anim">Animator asking for permission.</param>
	private void OnAnimatorCheck(ShowHideAnimator _anim) {
		// Skip if component is disabled
		if(!this.enabled) return;

		// If check overriden, we triggered the animation ourselves so let it go through
		if(!m_allowExternalChecks) return;

		// Check whether we can actualy trigger the animator
		if(!Check(targetDragonSku, currentMenuScreen)) {
			// Interrupt animation
			_anim.SetCheckFailed();
		}
	}
}

