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
    
    public enum EAdType {
        AdRewarded,
        AdInterstitial,
        CP2Interstitial
    };
    
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Misc/PF_PopupAdBlocker";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Events
	public class AdFinishedEvent : UnityEvent<bool> {}
	public AdFinishedEvent OnAdFinished = new AdFinishedEvent();

	// Setup
	private GameAds.EAdPurpose m_adPurpose = GameAds.EAdPurpose.NONE;
	private EAdType m_adType = EAdType.AdRewarded;

	// Internal
	private bool m_adPending = false;

	public static bool m_sBlocking = false;
	private bool m_forcedCancel = false;

	public GameObject m_panel;
	public GameObject m_cancelButton;
	private bool m_adSuccess = false;
    //------------------------------------------------------------------------//
    // STATIC METHODS														  //
    //------------------------------------------------------------------------// 
    public static PopupController LaunchAd(bool rewarded, GameAds.EAdPurpose _adPurpose, UnityAction<bool> _onAdFinishedCallback) {
        EAdType adType = (rewarded) ? EAdType.AdRewarded : EAdType.AdInterstitial;

        if (FeatureSettingsManager.AreCheatsEnabled)
            ControlPanel.Log("Launching Ad rewarded = " + rewarded + " purpose = " + _adPurpose, ControlPanel.ELogChannel.General);

        return LaunchAdType(adType, _adPurpose, _onAdFinishedCallback);
    }

    public static PopupController LaunchCp2Interstitial(UnityAction<bool> _onAdFinishedCallback) {
        if (FeatureSettingsManager.AreCheatsEnabled)
            ControlPanel.Log("Launching CP2Intersitial", ControlPanel.ELogChannel.CP2);
            
        return LaunchAdType(EAdType.CP2Interstitial, GameAds.EAdPurpose.NONE, _onAdFinishedCallback);
    }

    /// <summary>
    /// Special static initializer which will perform some checks and show some feedbacks
    /// if the Ad can't be launched, and will display the ad with the given configuration otherwise.
    /// </summary>
    /// <param name="_adType">Ad type</param>
    /// <param name="_adPurpose">Purpose of the ad.</param>
    /// <param name="_onAdFinishedCallback">Callback to be invoked when Ad has finished.</param>
    private static PopupController LaunchAdType(EAdType _adType, GameAds.EAdPurpose _adPurpose, UnityAction<bool> _onAdFinishedCallback) {
		// If ad can't be displayed, show error message instead of the popup
		if(!GameAds.adsAvailable) {
			PopupManager.canvas.worldCamera.gameObject.SetActive(true);

            string text = GetErrorTextByAdType(_adType);            
            
			// Show some feedback
			UIFeedbackText errorText = UIFeedbackText.CreateAndLaunch(
				text, 
				new Vector2(0.5f, 0.33f), 
				PopupManager.canvas.transform as RectTransform
			);
			errorText.text.color = UIConstants.ERROR_MESSAGE_COLOR;
			errorText.sequence.onComplete += PopupManager.instance.RefreshCameraActive;
			// Notify error
			if(_onAdFinishedCallback != null) _onAdFinishedCallback.Invoke(false);	// Unsuccessful
			return null;
		}

		if (!m_sBlocking)
		{
			m_sBlocking = true;
			// Open and initialize popup with the given settings!
			PopupController popup = PopupManager.LoadPopup(PopupAdBlocker.PATH);
			popup.GetComponent<PopupAdBlocker>().Init(_adType, _adPurpose, _onAdFinishedCallback);
			popup.Open();
			return popup;
		}
		else
		{
			if(_onAdFinishedCallback != null) _onAdFinishedCallback.Invoke(false);	// Unsuccessful
		}

		return null;
	}       
    
    private static bool WasAdTriggeredByUser(EAdType _adType) {
        return _adType == EAdType.AdRewarded;
    }

    private static string GetErrorTextByAdType(EAdType _adType) {
        string tid = WasAdTriggeredByUser(_adType) ? "TID_AD_ERROR" : "TID_AD_AUTO_ERROR";        
        return LocalizationManager.SharedInstance.Localize(tid);        
    } 

	//------------------------------------------------------------------------//
	// INTERNAL METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Parametrized initializer.
	/// </summary>
	/// <param name="_adType">Ad Type</param>
	/// <param name="_adPurpose">Purpose of the ad.</param>
	/// <param name="_onAdFinishedCallback">Callback to be invoked when Ad has finished.</param>
	private void Init(EAdType _adType, GameAds.EAdPurpose _adPurpose, UnityAction<bool> _onAdFinishedCallback) {
		m_panel.SetActive(true);
		m_cancelButton.SetActive(false);

		// Mark as pending
		m_adPending = true;

		// Store parameters
		m_adType = _adType;
		m_adPurpose = _adPurpose;

		// Clear callbacks and register new one
		OnAdFinished.RemoveAllListeners();
		if (_onAdFinishedCallback != null) {
			OnAdFinished.AddListener (_onAdFinishedCallback);
		}

		// If popup already opened, launch ad immediately
		if(GetComponent<PopupController>().isOpen) {
			LaunchAd();
		}
	}

	/// <summary>
	/// Launch the ad with the current setup.
	/// </summary>
	private void LaunchAd() {
		switch (m_adType) {
            case EAdType.AdRewarded:
                GameAds.instance.ShowRewarded(m_adPurpose, OnAdResult);
                break;

            case EAdType.AdInterstitial:
                GameAds.instance.ShowInterstitial(OnAdResult);
                break;

            case EAdType.CP2Interstitial:
                HDCP2Manager.Instance.PlayInterstitial(true, OnAdResult);
                break;
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
		m_adSuccess = _success;
		StartCoroutine(RealOnAdResult(_success));
	}

	IEnumerator RealOnAdResult( bool _success )
	{
		PopupController controller = GetComponent<PopupController>();
		// Sometime we recieve the callback before the popup is completely open, so we wait to be ready and then we close it
		while ( !controller.isReady ){
			yield return null;
		}

		float delayClose = 0;

		// If the ad couldn't be displayed, show message
		if(!_success && !m_forcedCancel) {
            string text = GetErrorTextByAdType(m_adType);

            UIFeedbackText feedbackText = UIFeedbackText.CreateAndLaunch(
				text,
				Vector2.one * 0.5f,
				PopupManager.canvas.transform as RectTransform
			);
			delayClose = feedbackText.duration;
		}
		m_panel.SetActive(false);
		yield return new WaitForSecondsRealtime( delayClose );

		// Close popup
		controller.Close(true);
		m_forcedCancel = false;
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
        if (WasAdTriggeredByUser(m_adType))
    		m_cancelButton.SetActive( true );
	}

	public void OnClosePreAnimation(){
		m_cancelButton.SetActive( false );
	}

	/// <summary>
	/// Popup has just been closed.
	/// </summary>
	public void OnClosePostAnimation() {
		// Broadcast result
		OnAdFinished.Invoke(m_adSuccess);

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
