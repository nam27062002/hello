// PopupAdBlocker.cs
// 
// Created by Alger Ortín Castellví on 29/08/2017.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System;
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
			// Show some feedback
			UIFeedbackText errorText = UIFeedbackText.CreateAndLaunch(
				LocalizationManager.SharedInstance.Localize("TID_AD_ERROR"), 
				new Vector2(0.5f, 0.33f), 
				PopupManager.canvas.transform as RectTransform
			);
			errorText.text.color = UIConstants.ERROR_MESSAGE_COLOR;

			// Notify error
			if(_onAdFinishedCallback != null) _onAdFinishedCallback.Invoke(false);	// Unsuccessful
			return;
		}

		// Open and initialize popup with the given settings!
		PopupController popup = PopupManager.LoadPopup(PopupAdBlocker.PATH);
		popup.GetComponent<PopupAdBlocker>().Init(_rewarded, _adPurpose, _onAdFinishedCallback);
		popup.Open();
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
		// Close popup
		GetComponent<PopupController>().Close(true);

		// If the ad couldn't be displayed, show message
		if(!_success) {
			UIFeedbackText.CreateAndLaunch(
				LocalizationManager.SharedInstance.Localize("TID_AD_ERROR"),
				Vector2.one * 0.5f,
				PopupManager.canvas.transform as RectTransform
			);
		}

		// Broadcast result
		OnAdFinished.Invoke(_success);
	}

	/// <summary>
	/// Popup is about to be opened.
	/// </summary>
	public void OnOpenPreAnimation() {
		// If an Ad is pending, launch it!
		if(m_adPending) LaunchAd();
	}

	/// <summary>
	/// Popup has just been closed.
	/// </summary>
	public void OnClosePostAnimation() {
		// Remove all listeners
		OnAdFinished.RemoveAllListeners();
	}
}
