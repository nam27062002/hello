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

	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// References
	[SerializeField] private Text m_loadingTxt = null;
	[SerializeField] private Slider m_loadingBar = null;

	// Internal
	private float timer = 0;

	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		// Call parent
		base.Awake();

		// Check required references
		DebugUtils.Assert(m_loadingTxt != null, "Required component!");
		DebugUtils.Assert(m_loadingBar != null, "Required component!");
	}

	/// <summary>
	/// First update.
	/// </summary>
	void Start() {
		// Load menu scene
		//GameFlow.GoToMenu();
		// [AOC] TEMP!! Simulate loading time with a timer for now
		timer = 0;

		// [AOC] This is a safe place to instantiate all the singletons
		//		 Do it now so we have it under control
		//		 Add all the new created singletons
		// Content and persistence
		UserProfile.CreateInstance();
		DefinitionsManager.CreateInstance();

		// Game
		DragonManager.CreateInstance();
		LevelManager.CreateInstance();
		MissionManager.CreateInstance();
		ChestManager.CreateInstance();
		RewardManager.CreateInstance();

		// Settings and setup
		GameSettings.CreateInstance();

		// Tech
		GameSceneManager.CreateInstance();
		FlowManager.CreateInstance();
		PoolManager.CreateInstance();
		ParticleManager.CreateInstance();
		PopupManager.CreateInstance();
		InstanceManager.CreateInstance();

		// Social
		ExternalPlatformManager.CreateInstance();
		ExternalPlatformManager.instance.Init();

		// Load persistence
		PersistenceManager.Init();
		PersistenceManager.Load();

		// Initialize localization
		Localization.SetSavedLanguage();

		// [AOC] TODO!! Figure out the proper way/place to do this
		PrecacheFonts();
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

		// Once load is finished, navigate to the menu scene
		if(loadProgress >= 1f && !GameSceneManager.isLoading) FlowManager.GoToMenu();
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
}

