// PopupPause.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/03/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.SceneManagement;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// In-game pause popup.
/// </summary>
public class PopupPause : PopupPauseBase {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/InGame/PF_PopupPause";

	public enum Tabs {
		MISSIONS,
		OPTIONS,
		FAQ,

		COUNT
	}

	[SerializeField]
    private GameObject m_3dTouch;

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Shortcut to tabs system
	private TabSystem m_tabs = null;
	private TabSystem tabs {
		get {
			if(m_tabs == null) {
				m_tabs = GetComponent<TabSystem>();
			}
			return m_tabs;
		}
	}


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	override protected void Awake() {
		base.Awake();
		if (m_3dTouch != null)
		{
			// m_3dTouch.SetActive( Input.touchPressureSupported );
			m_3dTouch.SetActive( PlatformUtils.Instance.InputPressureSupprted() );
		}
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {

	}

	private bool CanReturn(){
		return PopupManager.IsLastOpenPopup( m_popup ) && m_popup.isReady;
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	override protected void OnDestroy() {
		// Remove listeners
		m_popup.OnClosePostAnimation.RemoveListener(OnClosePostAnimation);

		base.OnDestroy();
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// End game button has been pressed
	/// </summary>
	public void OnEndGameButton() {
		PopupManager.OpenPopupInstant(PopupExitRunConfirmation.PATH);
		/*
		if (GameSettings.Get(GameSettings.SHOW_EXIT_RUN_CONFIRMATION_POPUP)) {
			PopupManager.OpenPopupInstant(PopupExitRunConfirmation.PATH);
		} else {
			if (InstanceManager.gameSceneController != null) {
				InstanceManager.gameSceneController.EndGame(true);
			}
		}*/
	}

	/// <summary>
	/// Dragon info button has been pressed.
	/// </summary>
	public void OnDragonInfoButton() {
		// Tracking
		string popupName = System.IO.Path.GetFileNameWithoutExtension(PopupDragonInfo.PATH);
		HDTrackingManager.Instance.Notify_InfoPopup(popupName, "settings");

		// Open the dragon info popup and initialize it with the current dragon's data
		PopupDragonInfo popup = PopupManager.OpenPopupInstant(PopupDragonInfo.PATH).GetComponent<PopupDragonInfo>();
		popup.Init(DragonManager.currentDragon);
	}

	/// <summary>
	/// Open animation is about to start.
	/// </summary>
	override public void OnOpenPreAnimation() {
		// Call parent
		base.OnOpenPreAnimation();

		// Hide the mission tab during the first run (tutorial)
		if(!UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.FIRST_RUN) && SceneManager.GetActiveScene().name != "SC_Popups") {
			// Get the tab system component
			if(tabs != null) {
				// Set options tab as initial screen
				tabs.SetInitialScreen((int)Tabs.OPTIONS);
				//tabs.GoToScreen((int)Tabs.OPTIONS, NavigationScreen.AnimType.NONE);

				// Hide unwanted buttons
				tabs.m_tabButtons[(int)Tabs.MISSIONS].gameObject.SetActive(false);
			}
		}
	}
}