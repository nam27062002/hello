// MenuPlayScreen.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on //2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using DG.Tweening;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class MenuPlayScreen : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	public GameObject m_connectButton;
	
	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() 
	{
		// Check if external connected to hide m_ConnectionButton;
		{
			// Hide connecting button
			if (m_connectButton)
				m_connectButton.SetActive(false);
		}
	}

	void OnDestroy()
	{
		
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() 
	{
		// Hide menu HUD
		InstanceManager.GetSceneController<MenuSceneController>().hud.GetComponent<ShowHideAnimator>().ForceHide(false);

		// Check Facebook/Weibo Connect visibility
	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Show menu HUD, except if the dragon selection tutorial hasn't yet been completed
		if(UsersManager.currentUser.IsTutorialStepCompleted(TutorialStep.DRAGON_SELECTION)) {
			InstanceManager.GetSceneController<MenuSceneController>().hud.GetComponent<ShowHideAnimator>().ForceShow(false);
		}

		// Save flag to not display this screen again
		GameVars.playScreenShown = true;
	}

	//------------------------------------------------------------------//
	// OTHER METHODS													//
	//------------------------------------------------------------------//

	public void OnConnectBtn()
	{
		// TODO(miguel): Disable Connection button until OnExternalLogin or OnExternalLoginError to avoid login multiple times
		SocialPlatformManager.SharedInstance.Login();

	}

	public void OnExternalLogin()
	{
		// TODO(miguel): Hide animation
		m_connectButton.SetActive(false);
	}

	public void OnExternalLoginError()
	{
		//TODO(miguel) : Enable connection button
	}

	void SocialPlatformButtonUpdate()
	{
		if (SocialPlatformManager.SharedInstance.IsLoggedIn() )
		{
			// Hide?
		}
	}

	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
}