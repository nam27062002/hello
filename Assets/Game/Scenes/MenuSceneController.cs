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
	[SerializeField]
	private MenuTransitionManager m_transitionManager = null;
	public MenuTransitionManager transitionManager {
		get { return m_transitionManager; }
	}

	public MenuScreen currentScreen {
		get { return m_transitionManager.currentScreen; }
	}

	// Dragon selector - responsible to set selected dragon
	private MenuDragonSelector m_dragonSelector = null;
	public MenuDragonSelector dragonSelector {
		get {
			if(m_dragonSelector == null) {
				m_dragonSelector = GetScreenData(MenuScreen.DRAGON_SELECTION).ui.FindComponentRecursive<MenuDragonSelector>();
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
				m_dragonScroller = GetScreenData(MenuScreen.DRAGON_SELECTION).scene3d.FindComponentRecursive<MenuDragonScroller>();
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
		Application.lowMemory += OnLowMemory;
		// Initialize the selected level in a similar fashion
		m_selectedLevel = UsersManager.currentUser.currentLevel;		// UserProfile should be loaded and initialized by now

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

        if (FeatureSettingsManager.IsDebugEnabled)
            Debug_Awake();
    }

	protected IEnumerator Start()
	{
		// Start menu music!
		AudioController.PlayMusic("hd_menu_music");

		// Make sure loading screen is hidden
		LoadingScreen.Toggle(false, false);

		// Track FTUX funnel
		if(UsersManager.currentUser.gamesPlayed == 1) {		// Exactly after the very first run
			HDTrackingManager.Instance.Notify_Funnel_FirstUX(FunnelData_FirstUX.Steps._06a_load_done);
		}

		// Start loading pet pill's on the background!
		// PetsScreenController petsScreen = GetScreenData(MenuScreen.PETS).ui.GetComponent<PetsScreenController>();
		// StartCoroutine(petsScreen.InstantiatePillsAsync());

		// Request latest global event data
		GlobalEventManager.RequestCurrentEventData();

		// wait one tick
		yield return null;

		// Check interstitial popups
		bool popupDisplayed = false;

		// 1. Rating popup
		if(!popupDisplayed) popupDisplayed = CheckRatingFlow();

		// 2. Survey popup
		if(!popupDisplayed) popupDisplayed = PopupAskSurvey.Check();        

		// Test mode
		yield return new WaitForSeconds(5.0f);
		if ( ApplicationManager.instance.appMode == ApplicationManager.Mode.TEST )
		{
			// Select dragon classic and go to play
			DragonManager.GetDragonData("dragon_classic").Acquire();
			OnDragonSelected("dragon_classic");
			OnPlayButton();
		}

	}    

    public static string RATING_DRAGON = "dragon_crocodile";
	public static bool CheckRatingFlow()
	{
		bool ret = false;
		DragonData data = DragonManager.GetDragonData(RATING_DRAGON);
		if ( data.GetLockState() > DragonData.LockState.LOCKED )
		{
			// Check if first time here!
			bool _checked = Prefs.GetBoolPlayer( Prefs.RATE_CHECK_DRAGON, false );
			if ( _checked )
			{
				// Check we come form a run!
				// Check if we need to make the player rate the game
				if ( Prefs.GetBoolPlayer(Prefs.RATE_CHECK, true))
				{
					if ( GameSceneManager.prevScene.CompareTo(ResultsScreenController.NAME) == 0 || GameSceneManager.prevScene.CompareTo(GameSceneController.NAME) == 0)
					{
						string dateStr = Prefs.GetStringPlayer( Prefs.RATE_FUTURE_DATE, System.DateTime.Now.ToString());
						System.DateTime futureDate = System.DateTime.Now;
						if (!System.DateTime.TryParse(dateStr, out futureDate))
							futureDate = System.DateTime.Now;
						if ( System.DateTime.Compare( System.DateTime.Now, futureDate) > 0 )
						{
							// Start Asking!
							if ( Application.platform == RuntimePlatform.Android ){
								PopupManager.OpenPopupInstant( PopupAskLikeGame.PATH );	
								ret = true;
							}else if ( Application.platform == RuntimePlatform.IPhonePlayer ){
								PopupAskRateUs.OpenIOSMarketForRating();
								ret = true;
							}
						}
					}
				}
			}
			else
			{
				// Next time we will
				Prefs.SetBoolPlayer( Prefs.RATE_CHECK_DRAGON, true );
			}
		}

		return ret;
	}

	protected override void OnDestroy() {
		Application.lowMemory -= OnLowMemory;
		base.OnDestroy();
        if (FeatureSettingsManager.IsDebugEnabled)
            Debug_OnDestroy();
    }
	
	/// <summary>
	/// Component enabled.
	/// </summary>
	void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.AddListener<DragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquired);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener<string>(MessengerEvents.MENU_DRAGON_SELECTED, OnDragonSelected);
		Messenger.RemoveListener<DragonData>(MessengerEvents.DRAGON_ACQUIRED, OnDragonAcquired);
	}


	void OnLowMemory()
	{
		Resources.UnloadUnusedAssets();
		System.GC.Collect();
	}

	//------------------------------------------------------------------//
	// PUBLIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Quick access to one of the UI screens composing the menu scene.
	/// </summary>
	/// <returns>The requested UI screen.</returns>
	/// <param name="_screen">The screen to be obtained.</param>
	public ScreenData GetScreenData(MenuScreen _screen) {
		return transitionManager.GetScreenData(_screen);
	}

	/// <summary>
	/// Go to the target screen.
	/// </summary>
	/// <param name="_targetScreen">Target screen.</param>
	public void GoToScreen(MenuScreen _screen) {
		transitionManager.GoToScreen(_screen, true);
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
	/// Start open flow on the given Egg.
	/// </summary>
	/// <returns>Whether the opening process was started or not.</returns>
	/// <param name="_egg">The egg to be opened.</param>
	public bool StartOpenEggFlow(Egg _egg) {
		// Just in case, shouldn't happen anything if there is no egg incubating or it is not ready
		if(_egg == null || _egg.state != Egg.State.READY) return false;

		// Go to OPEN_EGG screen and start open flow
		OpenEggScreenController openEggScreen = GetScreenData(MenuScreen.OPEN_EGG).ui.GetComponent<OpenEggScreenController>();
		openEggScreen.StartFlow(_egg);
		GoToScreen(MenuScreen.OPEN_EGG);

		return true;
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
		LoadingScreen.Toggle(true, false);

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
            PersistenceFacade.instance.Save_Request();

            // Broadcast message
            Messenger.Broadcast<string>(MessengerEvents.MENU_DRAGON_CONFIRMED, _sku);
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

    private GameObject m_uiCanvasGO;
    public GameObject GetUICanvasGO() {
        if (m_uiCanvasGO == null) {
            if (m_hud != null) {
                m_uiCanvasGO = m_hud.transform.parent.gameObject;
            }
        }

        return m_uiCanvasGO;
    }            

    #region debug

    private GameObject Debug_GetUICanvas() {
        return GetUICanvasGO();
    }

    private void Debug_Awake() {
        Messenger.AddListener<string, bool>(MessengerEvents.CP_BOOL_CHANGED, Debug_OnChanged);        
    }

    private void Debug_OnDestroy() {
        Messenger.RemoveListener<string, bool>(MessengerEvents.CP_BOOL_CHANGED, Debug_OnChanged);
    }

    private void Debug_OnChanged(string _id, bool _newValue) {
        if (_id == DebugSettings.INGAME_HUD) {
            GameObject _uiCanvas = Debug_GetUICanvas();
            if (_uiCanvas != null) {
                _uiCanvas.gameObject.SetActive(Prefs.GetBoolPlayer(DebugSettings.INGAME_HUD, true));
            }
        }      
    }
    #endregion
}

