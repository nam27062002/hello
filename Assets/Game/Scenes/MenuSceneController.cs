// MenuSceneController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Main controller for the menu scene.
/// </summary>
[RequireComponent(typeof(MenuScreensController))]
public class MenuSceneController : SceneController {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string NAME = "SC_Menu";

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//


	//------------------------------------------------------------------//
	// PROPERTIES														//
	//------------------------------------------------------------------//
	private string m_selectedDragon = "";
	public string selectedDragon {
		get { return m_selectedDragon; }
	}

	private string m_selectedLevel = "";
	public string selectedLevel {
		get { return m_selectedLevel; }
	}

	private MenuScreensController m_screensController = null;
	public MenuScreensController screensController {
		get { return m_screensController; }
	}

	[Separator()]
	[SerializeField] private MenuHUD m_hud = null;
	public MenuHUD hud {
		get { return m_hud; }
	}

	[SerializeField]
	private FogManager.FogAttributes m_fogSetup;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Call parent
		base.Awake();

		// Initialize selected dragon getting the one from the profile
		m_selectedDragon = UsersManager.currentUser.currentDragon;	// UserProfile should be loaded and initialized by now

		// Initialize the selected level in a similar fashion
		m_selectedLevel = UsersManager.currentUser.currentLevel;		// UserProfile should be loaded and initialized by now

		// Shortcut to screens controller
		m_screensController = GetComponent<MenuScreensController>();     

		if (m_fogSetup.texture == null)
		{
			m_fogSetup.CreateTexture();
			m_fogSetup.RefreshTexture();
		}
		m_fogSetup.FogSetup();
	}

	protected override void OnDestroy() {
		m_fogSetup.DestroyTexture();
		base.OnDestroy();
	}
	
	/// <summary>
	/// Component enabled.
	/// </summary>
	void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.AddListener<DragonData>(GameEvents.DRAGON_ACQUIRED, OnDragonAcquired);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(GameEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.RemoveListener<DragonData>(GameEvents.DRAGON_ACQUIRED, OnDragonAcquired);
	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Quick access to one of the UI screens composing the menu scene.
	/// </summary>
	/// <returns>The requested UI screen.</returns>
	/// <param name="_screen">The screen to be obtained.</param>
	public NavigationScreen GetScreen(MenuScreens _screen) {
		return screensController.GetScreen((int)_screen);
	}

	/// <summary>
	/// Quick access to one of the 3D scenes linked of each of the screens composing 
	/// the menu scene.
	/// </summary>
	/// <returns>The requested scene.</returns>
	/// <param name="_screen">The screen whose scene we want.</param>
	public MenuScreenScene GetScreenScene(MenuScreens _screen) {
		return screensController.GetScene((int)_screen);
	}

	/// <summary>
	/// Changes dragon selected to the given one.
	/// </summary>
	/// <param name="_sku">The sku of the dragon we want to be the current one.</param>
	public void SetSelectedDragon(string _sku) {
		// Let dragon screen do the job
		MenuDragonScreenController dragonScreen = GetScreen(MenuScreens.DRAGON_SELECTION).GetComponent<MenuDragonScreenController>();
		dragonScreen.dragonSelector.SetSelectedDragon(_sku);
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Play button has been pressed.
	/// </summary>
	public void OnPlayButton() {
		// Go to game!
		// [AOC] No need to block the button, the GameFlow already controls spamming
		FlowManager.GoToGame();
	}

	/// <summary>
	/// The selected dragon has changed.
	/// </summary>
	/// <param name="_id">The id of the selected dragon.</param>
	public void OnDragonSelected(string _sku) {
		// Update menu selected dragon
		m_selectedDragon = _sku;

		// If owned and different from profile's current dragon, update profile
		if(_sku != UsersManager.currentUser.currentDragon && DragonManager.GetDragonData(_sku).isOwned) {
			// Update profile
			UsersManager.currentUser.currentDragon = _sku;
		
			// Save persistence
			PersistenceManager.Save();

			// Broadcast message
			Messenger.Broadcast<string>(GameEvents.MENU_DRAGON_CONFIRMED, _sku);
		}
	}

	/// <summary>
	/// A dragon has been unlocked.
	/// </summary>
	/// <param name="_data">The dragon that has been unlocked.</param>
	public void OnDragonAcquired(DragonData _data) {
		// Just make it the current dragon
		OnDragonSelected(_data.def.sku);
	}
}

