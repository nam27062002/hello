// MenuSceneController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 21/08/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;
using System.Collections;

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
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	// Exposed members
	[Separator()]
	[SerializeField] private MenuHUD m_hud = null;
	public MenuHUD hud {
		get { return m_hud; }
	}

	[SerializeField]
	private FogManager.FogAttributes m_fogSetup;
	public FogManager.FogAttributes fogSetup {
		get { return m_fogSetup; }
	}

	// Temp vars
	private string m_selectedDragon = "";
	public string selectedDragon {
		get { return m_selectedDragon; }
	}

	private string m_selectedLevel = "";
	public string selectedLevel {
		get { return m_selectedLevel; }
	}

	// Shortcuts to interesting elements of the menu
	// Screens controller
	private MenuScreensController m_screensController = null;
	public MenuScreensController screensController {
		get { return m_screensController; }
	}

	// Dragon selector - responsible to set selected dragon
	private MenuDragonSelector m_dragonSelector = null;
	public MenuDragonSelector dragonSelector {
		get {
			if(m_dragonSelector == null) {
				m_dragonSelector = GetScreen(MenuScreens.DRAGON_SELECTION).FindComponentRecursive<MenuDragonSelector>();
			}
			return m_dragonSelector;
		}
	}

	// Dragon scroller - responsible to scroll the camera through the dragons
	private MenuDragonScroller m_dragonScroller = null;
	public MenuDragonScroller dragonScroller {
		get {
			if(m_dragonScroller == null) {
				// Use FindComponentRecursive rather than GetComponentInChildren to include inactive objects in the search
				m_dragonScroller = GetScreenScene(MenuScreens.DRAGON_SELECTION).FindComponentRecursive<MenuDragonScroller>();
			}
			return m_dragonScroller;
		}
	}

	// Current dragon 3D preview
	public MenuDragonPreview selectedDragonPreview {
		get { return dragonScroller.GetDragonPreview(selectedDragon); }
	}

	// Is the camera moving around?
	public bool isTweening {
		get {
			// Check direct tweens on the camera only
			return DOTween.IsTweening(mainCamera);
		}
	}

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Call parent
		base.Awake();

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

		// Define initial selected dragon
		if(string.IsNullOrEmpty(GameVars.menuInitialDragon)) {
			// Default behaviour: Last dragon used
			m_selectedDragon = UsersManager.currentUser.currentDragon;	// UserProfile should be loaded and initialized by now
		} else {
			// Forced dragon
			//SetSelectedDragon(GameVars.menuInitialDragon);
			m_selectedDragon = GameVars.menuInitialDragon;
			GameVars.menuInitialDragon = string.Empty;	// Reset var
		}

		ParticleManager.instance.poolLimits = ParticleManager.PoolLimits.Unlimited;
	}

	protected IEnumerator Start()
	{
		// Make sure loading screen is hidden
		LoadingScreen.Toggle(false);

		// Start loading pet pill's on the background!
		PetsScreenController petsScreen = screensController.GetScreen((int)MenuScreens.PETS).GetComponent<PetsScreenController>();
		StartCoroutine(petsScreen.InstantiatePillsAsync());

		yield return new WaitForSeconds(5.0f);
		if ( ApplicationManager.instance.appMode == ApplicationManager.Mode.TEST )
		{
			// Select dragon classic and go to play
			DragonManager.GetDragonData("dragon_classic").Acquire();
			OnDragonSelected("dragon_classic");
			OnPlayButton();
		}
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
		// Let the dragon selector do the job
		dragonSelector.SetSelectedDragon(_sku);
	}

	/// <summary>
	/// Recreate fog texture using current parameters.
	/// </summary>
	public void RecreateFogTexture() {
		if(m_fogSetup.texture != null) {
			m_fogSetup.DestroyTexture();
		}

		m_fogSetup.CreateTexture();
		m_fogSetup.RefreshTexture();
		m_fogSetup.FogSetup();
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	/// <summary>
	/// Play button has been pressed.
	/// </summary>
	public void OnPlayButton() {
		// Initialize and show loading screen
		LoadingScreen.InitWithCurrentData();
		LoadingScreen.Toggle(true);

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

