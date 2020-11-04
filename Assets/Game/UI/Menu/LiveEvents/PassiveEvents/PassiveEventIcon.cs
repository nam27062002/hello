// PassiveEventIcon.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 28/06/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for the passive event icon in the menu HUD.
/// </summary>
public class PassiveEventIcon : IPassiveEventIcon {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	[Flags]
	private enum DisplayLocation {
		// [AOC] Max 32 values (try inheriting from long if more are needed)
		NONE					= 1 << 0,
		PLAY_SCREEN				= 1 << 1,
		NORMAL_MODE				= 1 << 2,
		TOURNAMENT_MODE			= 1 << 3,
		INGAME					= 1 << 4,
		OPEN_EGG_SCREEN			= 1 << 5
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private ModifierIcon m_regularModifierIcon = null;
	[SerializeField] private ModifierIcon m_welcomeBackModifierIcon = null;

	//------------------------------------------------------------------------//
	// IPassiveEventIcon IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the manager for this specific passive event type.
	/// </summary>
	/// <returns>The event manager corresponding to this event type.</returns>
	protected override HDPassiveEventManager GetEventManager() {
		return HDLiveDataManager.passive;
	}

	/// <summary>
	/// Update visuals when new data has been received.
	/// </summary>
	protected override void RefreshDataInternal() {

        // Get passive manager again (in case we changed from live to local or the other way)
        m_passiveEventManager = GetEventManager();
		HDPassiveEventDefinition def = m_passiveEventManager.data.definition as HDPassiveEventDefinition;

		// Are we showing a welcome back passive event?
		bool isWelcomeBackPassive = HDLiveDataManager.instance.IsLocalPassiveEventAvailable();


		bool showRegular = false, showWelcomeBack  = false;

        if (def.mainMod == null)
        {
			// Not valid definition. Hide everything 
			showRegular = false;
			showWelcomeBack = false;

		} else
        {
			if (isWelcomeBackPassive && m_welcomeBackModifierIcon != null)
			{
                // If welcome back is active, try to use this specific WB icon
				m_welcomeBackModifierIcon.InitFromDefinition(def.mainMod);
				showRegular = false;
				showWelcomeBack = true;

			}
			else if (m_regularModifierIcon != null)
			{
                // In case WB not active (or WB icon not implemented) use the regular icon
				m_regularModifierIcon.InitFromDefinition(def.mainMod);
				showRegular = true;
				showWelcomeBack = false;
			}
		}

		if (m_regularModifierIcon != null) m_regularModifierIcon.gameObject.SetActive(showRegular);
		if (m_welcomeBackModifierIcon != null) m_welcomeBackModifierIcon.gameObject.SetActive(showWelcomeBack);
                     
	}


	/// <summary>
	/// Do custom visibility checks based on passive event type.
	/// </summary>
	/// <returns>Whether the icon can be displayed or not.</returns>
	protected override bool RefreshVisibilityInternal() {
        // Get passive manager again (in case we changed from live to local or the other way)
        m_passiveEventManager = GetEventManager();
        
		// Check current screen and event's UI settings
		bool show = true;
		Modifier mod = m_passiveEventManager.m_passiveEventDefinition.mainMod;
		if(mod != null) {
			// Where can this mod be displayed?
			DisplayLocation location = DisplayLocation.NONE;
            switch(mod.GetUICategory()) {
				case "stats": {
					location = DisplayLocation.PLAY_SCREEN 
						| DisplayLocation.NORMAL_MODE
						| DisplayLocation.TOURNAMENT_MODE
						| DisplayLocation.INGAME;
				} break;

				case "metagame": {
					location = DisplayLocation.PLAY_SCREEN 
						| DisplayLocation.NORMAL_MODE
						| DisplayLocation.OPEN_EGG_SCREEN;
				} break;

				case "levelUp": {
					location = DisplayLocation.PLAY_SCREEN 
						| DisplayLocation.NORMAL_MODE
						| DisplayLocation.INGAME;
				} break;
			}

			// Are we in the right place?
			// Loading Screen
			if(LoadingScreen.isVisible) {
				// Check tournament mode
				switch(SceneController.mode) {
					case SceneController.Mode.DEFAULT: { 
						show &= CheckLocation(location, DisplayLocation.INGAME | DisplayLocation.NORMAL_MODE);
					} break;

					case SceneController.Mode.TOURNAMENT: {
						show &= CheckLocation(location, DisplayLocation.INGAME | DisplayLocation.TOURNAMENT_MODE);
					} break;
				}
			}

			// Menu
			else if(InstanceManager.menuSceneController != null) {
				// Which screen?
				MenuScreen currentScreen = InstanceManager.menuSceneController.currentScreen;
				switch(currentScreen) {
					case MenuScreen.PLAY: {
						show &= CheckLocation(location, DisplayLocation.PLAY_SCREEN);
					} break;

					case MenuScreen.DRAGON_SELECTION: {
						show &= CheckLocation(location, DisplayLocation.NORMAL_MODE);
					} break;

					case MenuScreen.TOURNAMENT_INFO: {
						show &= CheckLocation(location, DisplayLocation.TOURNAMENT_MODE);
					} break;

					case MenuScreen.OPEN_EGG: {
						show &= CheckLocation(location, DisplayLocation.OPEN_EGG_SCREEN);
					} break;
				}
			}

			// Ingame / Results
			else {
				// Check tournament mode
				switch(GameSceneController.mode) {
					case GameSceneController.Mode.DEFAULT: {
						show &= CheckLocation(location, DisplayLocation.INGAME | DisplayLocation.NORMAL_MODE);
					} break;

					case GameSceneController.Mode.TOURNAMENT: {
						show &= CheckLocation(location, DisplayLocation.INGAME | DisplayLocation.TOURNAMENT_MODE);
					} break;
				}
			}
		}

		// All checks done!
		return show;
	}

	//------------------------------------------------------------------------//
	// INTERNAL	UTILS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Compare two location flags.
	/// </summary>
	/// <returns><c>true</c>, if the location to check is included in the reference location.</returns>
	/// <param name="_ref">Reference location.</param>
	/// <param name="_toCheck">Location to check.</param>
	private bool CheckLocation(DisplayLocation _ref, DisplayLocation _toCheck) {
		return (_ref & _toCheck) == _toCheck;	// This tells us if all the flags in _toCheck are included in _ref
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

}