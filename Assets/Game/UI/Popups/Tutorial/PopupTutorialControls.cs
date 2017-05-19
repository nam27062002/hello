// PopupTutorialControls.cs
// 
// Created by Alger Ortín Castellví on 05/04/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Pause popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupTutorialControls : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Tutorial/PF_PopupTutorialControls";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private ShowHideAnimator m_loadingInfo = null;
	[SerializeField] private ShowHideAnimator m_playButton = null;
	[SerializeField] private TextMeshProUGUI m_loadingTxt = null;

	// References
	private GameSceneController m_sceneController = null;
	private string m_localizedLoadingString = "";
	private float m_loadProgress = 0f;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get references
		m_sceneController = InstanceManager.gameSceneController;

		// Cache localized string to avoid doing the translation every frame
		m_localizedLoadingString = LocalizationManager.SharedInstance.Localize("TID_GEN_LOADING");
		m_localizedLoadingString += " {0}%";	// Add percentage replacement at the end
	}

	/// <summary>
	/// Update loop.
	/// </summary>
	private void Update() {
		// Show/Hide elements
		m_loadProgress = m_sceneController.levelActivationProgress;
		m_loadingInfo.Set(m_loadProgress < 1f, true);
		m_playButton.Set(m_loadProgress >= 1f, true);
		if ( ApplicationManager.instance.appMode == ApplicationManager.Mode.TEST && m_loadProgress >= 1)
		{
			GetComponent<PopupController>().Close(true);
		}
		//m_loadingTxt.text = System.String.Format(m_localizedLoadingString, StringUtils.FormatNumber(m_loadProgress * 100f, 0));
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Open animation is about to start.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Prevent game from starting once loading has finished
		m_sceneController.startWhenLoaded = false;

		// Initialize popup
		m_loadingInfo.ForceShow(false);
		m_playButton.ForceHide(false);
	}

	/// <summary>
	/// Close animation has started.
	/// </summary>
	public void OnClosePreAnimation() {
		// Start playing!
		m_sceneController.startWhenLoaded = true;
	}
}
