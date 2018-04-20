// PopupAdBlocker.cs
// 
// Created by Alger Ortín Castellví on 29/08/2017.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Events;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Blocking popup while an ad is being displayed.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupAdBlocker : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/InGame/PF_PopupAdBlocker";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Events
	public class AdFinishedEvent : UnityEvent<bool> {}
	public AdFinishedEvent OnAdFinished = new AdFinishedEvent();

	// Setup
	private GameAds.EAdPurpose m_adPurpose = GameAds.EAdPurpose.NONE;
	private bool m_rewarded = true;

	// Internal
	private bool m_adPending = false;

	public static bool m_sBlocking = false;
	private bool m_forcedCancel = false;

	public GameObject m_cancelButton;
	//------------------------------------------------------------------------//
	// STATIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Special static initializer which will perform some checks and show some feedbacks
	/// if the Ad can't be launched, and will display the ad with the given configuration otherwise.
	/// </summary>
	/// <param name="_rewarded">Rewarded or interstitial?</param>
	/// <param name="_adPurpose">Purpose of the ad.</param>
	/// <param name="_onAdFinishedCallback">Callback to be invoked when Ad has finished.</param>
	public static void Launch(bool _rewarded, GameAds.EAdPurpose _adPurpose, UnityAction<bool> _onAdFinishedCallback) {
		// If ad can't be displayed, show error message instead of the popup
		if(!GameAds.adsAvailable) {
			PopupManager.canvas.worldCamera.gameObject.SetActive(true);
			// Show some feedback
			UIFeedbackText errorText = UIFeedbackText.CreateAndLaunch(
				LocalizationManager.SharedInstance.Localize("TID_AD_ERROR"), 
				new Vector2(0.5f, 0.33f), 
				PopupManager.canvas.transform as RectTransform
			);
			errorText.text.color = UIConstants.ERROR_MESSAGE_COLOR;
			errorText.sequence.onComplete += PopupManager.instance.RefreshCameraActive;
			// Notify error
			if(_onAdFinishedCallback != null) _onAdFinishedCallback.Invoke(false);	// Unsuccessful
			return;
		}

		if (!m_sBlocking)
		{
			m_sBlocking = true;
			// Open and initialize popup with the given settings!
			PopupController popup = PopupManager.LoadPopup(PopupAdBlocker.PATH);
			popup.GetComponent<PopupAdBlocker>().Init(_rewarded, _adPurpose, _onAdFinishedCallback);
			popup.Open();
		}
		else
		{
			if(_onAdFinishedCallback != null) _onAdFinishedCallback.Invoke(false);	// Unsuccessful
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Parametrized initializer.
	/// </summary>
	/// <param name="_rewarded">Rewarded or interstitial?</param>
	/// <param name="_adPurpose">Purpose of the ad.</param>
	/// <param name="_onAdFinishedCallback">Callback to be invoked when Ad has finished.</param>
	private void Init(bool _rewarded, GameAds.EAdPurpose _adPurpose, UnityAction<bool> _onAdFinishedCallback) {
		m_cancelButton.SetActive(false);

		// Mark as pending
		m_adPending = true;

		// Store parameters
		m_rewarded = _rewarded;
		m_adPurpose = _adPurpose;

		// Clear callbacks and register new one
		OnAdFinished.RemoveAllListeners();
		OnAdFinished.AddListener(_onAdFinishedCallback);

		// If popup already opened, launch ad immediately
		if(GetComponent<PopupController>().isOpen) {
			LaunchAd();
		}
	}

	/// <summary>
	/// Launch the ad with the current setup.
	/// </summary>
	private void LaunchAd() {
		// Rewarded?
		if(m_rewarded) {
			GameAds.instance.ShowRewarded(m_adPurpose, OnAdResult);
		} else {
			GameAds.instance.ShowInterstitial(OnAdResult);
		}

		// Ad not pending anymore!
		m_adPending = false;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The add has finished playing in the Ad manager.
	/// </summary>
	/// <param name="_success">Has the ad been played?</param>
	private void OnAdResult(bool _success) {
		StartCoroutine(RealOnAdResult(_success));
	}

	IEnumerator RealOnAdResult( bool _success )
	{
		PopupController controller = GetComponent<PopupController>();
		// Sometime we recieve the callback before the popup is completely open, so we wait to be ready and then we close it
		while ( !controller.isReady ){
			yield return null;
		}

		// Close popup
		controller.Close(true);

		// If the ad couldn't be displayed, show message
		if(!_success && !m_forcedCancel) {
			UIFeedbackText.CreateAndLaunch(
				LocalizationManager.SharedInstance.Localize("TID_AD_ERROR"),
				Vector2.one * 0.5f,
				PopupManager.canvas.transform as RectTransform
			);
		}

		m_forcedCancel = false;

		// Broadcast result
		OnAdFinished.Invoke(_success);
	}

	/// <summary>
	/// Popup is about to be opened.
	/// </summary>
	public void OnOpenPreAnimation() {
		// If an Ad is pending, launch it!
		if(m_adPending) {
			LaunchAd();
		}
	}

	public void OnOpenPostAnimation(){
		m_cancelButton.SetActive( true );
	}

	public void OnClosePreAnimation(){
		m_cancelButton.SetActive( false );
	}

	/// <summary>
	/// Popup has just been closed.
	/// </summary>
	public void OnClosePostAnimation() {
		// Remove all listeners
		OnAdFinished.RemoveAllListeners();
		m_sBlocking = false;
	}


	public void OnForceCancel()
	{
		m_forcedCancel = true;
		if (GameAds.instance.IsWaitingToPlayAnAd())
		{
			GameAds.instance.StopWaitingToPlayAnAd();
		}
		else
		{
			OnAdResult(false);
		}
	}
}
