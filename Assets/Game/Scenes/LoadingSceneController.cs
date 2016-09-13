// LoadingSceneController.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 20/08/2015.
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
/// Main controller for the loading scene.
/// </summary>
public class LoadingSceneController : SceneController {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public static readonly string NAME = "SC_Loading";

    private static bool s_inSaveLoaderState = false;

    public static bool InSaveLoaderState
    {
        get { return s_inSaveLoaderState; }
    }

    //------------------------------------------------------------------//
    // MEMBERS															//
    //------------------------------------------------------------------//
    // References
    [SerializeField] private Text m_loadingTxt = null;
	[SerializeField] private Slider m_loadingBar = null;

	// Internal
	private float timer = 0;

    private bool m_startLoadFlow = true;
    private bool m_loading = false;
    private bool m_loadingDone = false;

    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    override protected void Awake() {        		
        // Call parent
		base.Awake();
		ContentManager.InitContent();
		// Check required references
		DebugUtils.Assert(m_loadingTxt != null, "Required component!");
		DebugUtils.Assert(m_loadingBar != null, "Required component!");                
    }    

	/// <summary>
	/// First update.
	/// </summary>
	void Start() {                
        SaveFacade.Instance.OnLoadComplete += OnLoadingFinished;

        // Load menu scene
        //GameFlow.GoToMenu();
        // [AOC] TEMP!! Simulate loading time with a timer for now
        timer = 0;

        // [AOC] This is a safe place to instantiate all the singletons
        //		 Do it now so we have it under control
        //		 Add all the new created singletons
        // Content and persistence
        //DefinitionsManager.CreateInstance(true);

        // [DGR] A single point to handle applications events (init, pause, resume, etc) in a high level.
        // No parameter is passed because it has to be created only once in order to make sure that it's initialized only once
        ApplicationManager.CreateInstance();
        
		UsersManager.CreateInstance();                

        // Game
        DragonManager.CreateInstance(true);
		LevelManager.CreateInstance(true);
		MissionManager.CreateInstance(true);
		ChestManager.CreateInstance(true);
		RewardManager.CreateInstance(true);
		EggManager.CreateInstance(true);
		EggManager.InitFromDefinitions();

		// Settings and setup
		GameSettings.CreateInstance(false);

		// Tech
		GameSceneManager.CreateInstance(true);
		FlowManager.CreateInstance(true);
		PoolManager.CreateInstance(true);
		ParticleManager.CreateInstance(true);
		PopupManager.CreateInstance(true);
		InstanceManager.CreateInstance(true);
		
        // Load persistence        
        SaveFacade.Instance.Init();               
        PersistenceManager.Init();		                      

		// Initialize localization
		SetSavedLanguage();

		// [AOC] TODO!! Figure out the proper way/place to do this
		PrecacheFonts();        
	}

    public static void SetSavedLanguage()
    {
        string strLanguageSku = PlayerPrefs.GetString(PopupSettings.KEY_SETTINGS_LANGUAGE);

        if (string.IsNullOrEmpty(strLanguageSku))
        {
            strLanguageSku = LocalizationManager.SharedInstance.GetDefaultSystemLanguage();
        }

        LocalizationManager.SharedInstance.SetLanguage(strLanguageSku);
    }    

    /// <summary>
    /// Called every frame.
    /// </summary>
    void Update() {       
		// Update load progress
		//m_loadingTxt.text = System.String.Format("LOADING {0}%", StringUtils.FormatNumber(SceneManager.loadProgress * 100f, 0));

		// [AOC] TODO!! Fake timer for now
		timer += Time.deltaTime;
		float loadProgress = Mathf.Min(timer/1f, 1f);	// Divide by the amount of seconds to simulate
		m_loadingTxt.text = System.String.Format("LOADING {0}%", StringUtils.FormatNumber(loadProgress * 100f, 0));
		m_loadingBar.normalizedValue = loadProgress;

        // The persistence is loaded once this loading state is loaded and the social facade is initialized (or a reasonable timeout in order to prevent the game from getting stack 
        // if social facade can't be initialized). We want to wait for the social facade to be initialized in order to have the chance to reuse a previous login to the social platform 
        // if the session hasn't expired yet.
        if (!GameSceneManager.isLoading && 
            (SocialFacade.Instance.IsInited() || timer > 1f)) {
            StartLoadFlow();
        }

        // Once load is finished, navigate to the menu scene
        if (loadProgress >= 1f && !GameSceneManager.isLoading && m_loadingDone) {
            FlowManager.GoToMenu();
        }
    }

	/// <summary>
	/// Destructor.
	/// </summary>
	override protected void OnDestroy() {
		base.OnDestroy();
	}

	/// <summary>
	/// Precache fonts to avoid in-game CPU spikes.
	/// </summary>
	private void PrecacheFonts() {
		// Target fonts
		string[] fontNames = new string[] {
			"FNT_Default",
			"FNT_Bold"
		};

		// Target sizes
		int[] sizes = new int[] {
			50, 72, 100
		};

		// Precache string
		string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!\"·$%&/()=?¿*+-_{}<>|@#€\\";	// Almost every standard char (no language customs)

		// Do it!
		for(int i = 0; i < fontNames.Length; i++) {
			// Load font
			Font f = Resources.Load("UI/Fonts/" + fontNames[i]) as Font;

			// Precache all sizes for that font
			for(int j = 0; j < sizes.Length; j++) {
				f.RequestCharactersInTexture(chars, sizes[j]);
			}
		}
	}

    private void StartLoadFlow()
    {
        if (m_startLoadFlow)
        {
            s_inSaveLoaderState = true;

            m_startLoadFlow = false;
            m_loading = true;
            m_loadingDone = false;

            Debug.Log("Started Loading Flow");

            PersistenceManager.Load();            
        }
    }

    private void OnLoadingFinished()
    {
        Debug.Log("OnLoadingFinished - " + m_loading);        
        if (m_loading)
        {
            SaveFacade.Instance.OnLoadComplete -= OnLoadingFinished;

            m_loading = false;

            Action onComplete = delegate ()
            {
                Debug.Log("SaveLoaderState (OnLoadingFinished) :: Auth state check complete!");

                s_inSaveLoaderState = false;
                m_loadingDone = true;                
            };

            SocialFacade.Network network = SocialManager.GetSelectedSocialNetwork();
            if (SocialManager.Instance.IsUser(network) && !SaveFacade.Instance.cloudSaveEnabled)
            {
                Debug.Log("SaveLoaderState (OnLoadingFinished) :: Check Facebook User Auth State!");
                SocialManager.Instance.Authenticate(network, onComplete);
            }
            else
            {
                onComplete();
            }
        }
    }
}

