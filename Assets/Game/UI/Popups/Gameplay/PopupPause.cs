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
using InControl;

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
        if (m_popup.isReady) {
            InputDevice device = InputManager.ActiveDevice;
            if (device != null && device.Command.WasReleased) {
                m_popup.Close(false);
            }
        }
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
		// Open confirmation popup
		PopupController popup = PopupManager.OpenPopupInstant(PopupExitRunConfirmation.PATH);
		if(popup != null) {
			// Move forward so the mission 3D icons are rendered behind
			popup.transform.SetLocalPosZ(-1000f);
		}
	}

	/// <summary>
	/// Dragon info button has been pressed.
	/// </summary>
	public void OnDragonInfoButton() {
		// Open the dragon info popup and initialize it with the current dragon's data
		PopupDragonInfo.OpenPopupForDragon(InstanceManager.player.data, "pause");
	}

	/// <summary>
	/// Open animation is about to start.
	/// </summary>
	override public void OnOpenPreAnimation() {
		// Call parent
		base.OnOpenPreAnimation();

        HDTrackingManager.Instance.NotifyIngamePause();

		// Hide the mission tab during FTUX
		if((UsersManager.currentUser.gamesPlayed < GameSettings.ENABLE_MISSIONS_AT_RUN || SceneController.mode == SceneController.Mode.TOURNAMENT) && SceneManager.GetActiveScene().name != "SC_Popups") {
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