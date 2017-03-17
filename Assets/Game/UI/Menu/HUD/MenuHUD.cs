// MenuHUD.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/11/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Controller for the HUD in the main menu.
/// </summary>
[RequireComponent(typeof(ShowHideAnimator))]
public class MenuHUD : MonoBehaviour {
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Internal
	private bool m_dirty = true;

	private ShowHideAnimator m_animator = null;
	private ShowHideAnimator animator {
		get { 
			if(m_animator == null) {
				m_animator = GetComponent<ShowHideAnimator>();
			}
			return m_animator;
		}
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// HUD should always be on top, but for editing purposes, we keep it belows
		this.transform.SetAsLastSibling();

		// Subscribe to external events
		Messenger.AddListener<NavigationScreenSystem.ScreenChangedEventData>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnNavigationScreenChanged);
	}

	/// <summary>
	/// Start hidden.
	/// </summary>
	private void Start() {
		// Start hidden but force a check
		animator.ForceHide(false, false);	// Don't disable!
		m_dirty = true;
	}

	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		Messenger.AddListener<NavigationScreenSystem.ScreenChangedEventData>(EngineEvents.NAVIGATION_SCREEN_CHANGED, OnNavigationScreenChanged);
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// If there is a visibility change pending, wait until all required stuff is ready before applying it
		if(m_dirty) {
			// [DGR] Nothing is done while switching scenes, otherwise UsersManager could be recreated during a transition of scenes
			// causing "Some objects were not cleaned up when closing the scene" error as a consequence of spawning new GameObjects from OnDestroy
			// Perform all required checks
			// 1. GameSceneManager ready?
			if(GameSceneManager.instance == null || GameSceneManager.isLoading) return;

			// 2. UsersManager ready?
			if(UsersManager.instance == null || UsersManager.currentUser == null) return;

			// 3. InstanceManager ready?
			if(InstanceManager.instance == null) return;

			// 4. Menu ready?
			MenuSceneController menuController = InstanceManager.menuSceneController;
			if(menuController == null || menuController.screensController == null) return;

			// Everything ok! Update visibility
			// Toggle hud's visibility based on current menu screen
			bool show = true;
			switch(menuController.screensController.currentMenuScreen) {
				// Screens where we don't want the hud to be visible
				case MenuScreens.PLAY: {
					show = false;
				} break;
			}

			// Don't show if DragonSelection tutorial is not completed
			show &= UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.DRAGON_SELECTION);

			// Apply! But don't disable object so update is still called!
			if(show) {
				animator.ForceShow();
			} else {
				animator.ForceHide(true, false);
			}

			// Not diry anymore! ^_^
			m_dirty = false;
		}
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Open the currency shop popup.
	/// </summary>
	public void OpenCurrencyShopPopup() {
		// Just do it
		PopupController popup = PopupManager.OpenPopupInstant(PopupCurrencyShop.PATH);

		// In this particular case we want to allow several purchases in a row, so don't auto-close popup
		popup.GetComponent<PopupCurrencyShop>().closeAfterPurchase = false;

		// Currency popup / Resources flow disabled for now
		//UIFeedbackText.CreateAndLaunch(LocalizationManager.SharedInstance.Localize("TID_GEN_COMING_SOON"), new Vector2(0.5f, 0.33f), this.GetComponentInParent<Canvas>().transform as RectTransform);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// A navigation screen has changed.
	/// </summary>
	/// <param name="_eventData">Event data.</param>
	private void OnNavigationScreenChanged(NavigationScreenSystem.ScreenChangedEventData _eventData) {
		// Mark as dirty!
		m_dirty = true;
	}
}
