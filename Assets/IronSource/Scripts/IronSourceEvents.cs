using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;

public class IronSourceEvents : MonoBehaviour
{
	private const string ERROR_CODE = "error_code";
	private const string ERROR_DESCRIPTION = "error_description";
    private const string INSTANCE_ID_KEY = "instanceId";
    private const string PLACEMENT_KEY = "placement";	
		
	void Awake ()
	{
		gameObject.name = "IronSourceEvents";			//Change the GameObject name to IronSourceEvents.
		DontDestroyOnLoad (gameObject);					//Makes the object not be destroyed automatically when loading a new scene.
	}
	
	// ******************************* Rewarded Video Events *******************************
	private static event Action<IronSourceError> _onRewardedVideoAdShowFailedEvent;

	public static event Action<IronSourceError> onRewardedVideoAdShowFailedEvent {
		add {
			if (_onRewardedVideoAdShowFailedEvent == null || !_onRewardedVideoAdShowFailedEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdShowFailedEvent += value;
			}
		}
		
		remove {
			if (_onRewardedVideoAdShowFailedEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdShowFailedEvent -= value;
			}
		}
	}
	
	public void onRewardedVideoAdShowFailed (string description)
	{
		if (_onRewardedVideoAdShowFailedEvent != null) {
			IronSourceError sse = getErrorFromErrorObject (description);
			_onRewardedVideoAdShowFailedEvent (sse);
		}
	}

	private static event Action _onRewardedVideoAdOpenedEvent;

	public static event Action onRewardedVideoAdOpenedEvent {
		add {
			if (_onRewardedVideoAdOpenedEvent == null || !_onRewardedVideoAdOpenedEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdOpenedEvent += value;
			}
		}
		
		remove {
			if (_onRewardedVideoAdOpenedEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdOpenedEvent -= value;
			}
		}
	}
	
	public void onRewardedVideoAdOpened (string empty)
	{
		if (_onRewardedVideoAdOpenedEvent != null) {
			_onRewardedVideoAdOpenedEvent ();
		}
	}

	private static event Action _onRewardedVideoAdClosedEvent;

	public static event Action onRewardedVideoAdClosedEvent {
		add {
			if (_onRewardedVideoAdClosedEvent == null || !_onRewardedVideoAdClosedEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdClosedEvent += value;
			}
		}
		
		remove {
			if (_onRewardedVideoAdClosedEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdClosedEvent -= value;
			}
		}
	}
	
	public void onRewardedVideoAdClosed (string empty)
	{
		if (_onRewardedVideoAdClosedEvent != null) {
			_onRewardedVideoAdClosedEvent ();
		}
	}

	private static event Action _onRewardedVideoAdStartedEvent;

	public static event Action onRewardedVideoAdStartedEvent {
		add {
			if (_onRewardedVideoAdStartedEvent == null || !_onRewardedVideoAdStartedEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdStartedEvent += value;
			}
		}
		
		remove {
			if (_onRewardedVideoAdStartedEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdStartedEvent -= value;
			}
		}
	}
	
	public void onRewardedVideoAdStarted (string empty)
	{
		if (_onRewardedVideoAdStartedEvent != null) {
			_onRewardedVideoAdStartedEvent ();
		}
	}

	private static event Action _onRewardedVideoAdEndedEvent;

	public static event Action onRewardedVideoAdEndedEvent {
		add {
			if (_onRewardedVideoAdEndedEvent == null || !_onRewardedVideoAdEndedEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdEndedEvent += value;
			}
		}
		
		remove {
			if (_onRewardedVideoAdEndedEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdEndedEvent -= value;
			}
		}
	}
	
	public void onRewardedVideoAdEnded (string empty)
	{
		if (_onRewardedVideoAdEndedEvent != null) {
			_onRewardedVideoAdEndedEvent ();
		}
	}

	private static event Action<IronSourcePlacement> _onRewardedVideoAdRewardedEvent;

	public static event Action<IronSourcePlacement> onRewardedVideoAdRewardedEvent {
		add {
			if (_onRewardedVideoAdRewardedEvent == null || !_onRewardedVideoAdRewardedEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdRewardedEvent += value;
			}
		}
		
		remove {
			if (_onRewardedVideoAdRewardedEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdRewardedEvent -= value;
			}
		}
	}
	
	public void onRewardedVideoAdRewarded (string description)
	{
		if (_onRewardedVideoAdRewardedEvent != null) {
			IronSourcePlacement ssp = getPlacementFromObject (description);
			_onRewardedVideoAdRewardedEvent (ssp);
		}	
	}

	private static event Action<IronSourcePlacement> _onRewardedVideoAdClickedEvent;

	public static event Action<IronSourcePlacement> onRewardedVideoAdClickedEvent {
		add {
			if (_onRewardedVideoAdClickedEvent == null || !_onRewardedVideoAdClickedEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdClickedEvent += value;
			}
		}
		
		remove {
			if (_onRewardedVideoAdClickedEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdClickedEvent -= value;
			}
		}
	}
	
	public void onRewardedVideoAdClicked (string description)
	{
		if (_onRewardedVideoAdClickedEvent != null) {
			IronSourcePlacement ssp = getPlacementFromObject (description);
			_onRewardedVideoAdClickedEvent (ssp);
		}	
	}

	private static event Action<bool> _onRewardedVideoAvailabilityChangedEvent;

	public static event Action<bool> onRewardedVideoAvailabilityChangedEvent {
		add {
			if (_onRewardedVideoAvailabilityChangedEvent == null || !_onRewardedVideoAvailabilityChangedEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAvailabilityChangedEvent += value;
			}
		}

		remove {
			if (_onRewardedVideoAvailabilityChangedEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAvailabilityChangedEvent -= value;
			}
		}
	}

	public void onRewardedVideoAvailabilityChanged (string stringAvailable)
	{
		bool isAvailable = (stringAvailable == "true") ? true : false;
		if (_onRewardedVideoAvailabilityChangedEvent != null)
			_onRewardedVideoAvailabilityChangedEvent (isAvailable);
	}

	// ******************************* RewardedVideo DemandOnly Events *******************************

    private static event Action<string,bool> _onRewardedVideoAvailabilityChangedDemandOnlyEvent;

	public static event Action<string,bool> onRewardedVideoAvailabilityChangedDemandOnlyEvent {
		add {
			if (_onRewardedVideoAvailabilityChangedDemandOnlyEvent == null || !_onRewardedVideoAvailabilityChangedDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAvailabilityChangedDemandOnlyEvent += value;
			}
		}

		remove {
			if (_onRewardedVideoAvailabilityChangedDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAvailabilityChangedDemandOnlyEvent -= value;
			}
		}
	}

	public void onRewardedVideoAvailabilityChangedDemandOnly (string args)
	{

		if (_onRewardedVideoAvailabilityChangedDemandOnlyEvent != null && !String.IsNullOrEmpty(args)) {

			List<object> argList = IronSourceJSON.Json.Deserialize (args) as List<object>;
		     bool isAvailable = (argList[1].ToString().ToLower() == "true") ? true : false;
			 string instanceId = argList[0].ToString();

			_onRewardedVideoAvailabilityChangedDemandOnlyEvent (instanceId,isAvailable);
		}
	}


	private static event Action<string> _onRewardedVideoAdOpenedDemandOnlyEvent;

	public static event Action<string> onRewardedVideoAdOpenedDemandOnlyEvent {
		add {
			if (_onRewardedVideoAdOpenedDemandOnlyEvent == null || !_onRewardedVideoAdOpenedDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdOpenedDemandOnlyEvent += value;
			}
		}
		
		remove {
			if (_onRewardedVideoAdOpenedDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdOpenedDemandOnlyEvent -= value;
			}
		}
	}
	
	public void onRewardedVideoAdOpenedDemandOnly (string instanceId)
	{
		if (_onRewardedVideoAdOpenedDemandOnlyEvent != null) {
			_onRewardedVideoAdOpenedDemandOnlyEvent (instanceId);
		}
	}

	private static event Action<string> _onRewardedVideoAdClosedDemandOnlyEvent;

	public static event Action<string> onRewardedVideoAdClosedDemandOnlyEvent {
		add {
			if (_onRewardedVideoAdClosedDemandOnlyEvent == null || !_onRewardedVideoAdClosedDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdClosedDemandOnlyEvent += value;
			}
		}
		
		remove {
			if (_onRewardedVideoAdClosedDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdClosedDemandOnlyEvent -= value;
			}
		}
	}
	
	public void onRewardedVideoAdClosedDemandOnly (string instanceId)
	{
		if (_onRewardedVideoAdClosedDemandOnlyEvent != null) {
			_onRewardedVideoAdClosedDemandOnlyEvent (instanceId);
		}
	}

	private static event Action<string,IronSourcePlacement> _onRewardedVideoAdRewardedDemandOnlyEvent;

	public static event Action<string,IronSourcePlacement> onRewardedVideoAdRewardedDemandOnlyEvent {
		add {
			if (_onRewardedVideoAdRewardedDemandOnlyEvent == null || !_onRewardedVideoAdRewardedDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdRewardedDemandOnlyEvent += value;
			}
		}
		
		remove {
			if (_onRewardedVideoAdRewardedDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdRewardedDemandOnlyEvent -= value;
			}
		}
	}
	
	public void onRewardedVideoAdRewardedDemandOnly (string args)
	{
		if (_onRewardedVideoAdRewardedDemandOnlyEvent != null && !String.IsNullOrEmpty (args)) {

			List<object> argList = IronSourceJSON.Json.Deserialize (args) as List<object>;  
			string instanceId = argList[0].ToString();
			IronSourcePlacement ssp = getPlacementFromObject (argList[1]);
			_onRewardedVideoAdRewardedDemandOnlyEvent (instanceId, ssp);
		}	
	}

    private static event Action<string,IronSourceError> _onRewardedVideoAdShowFailedDemandOnlyEvent;

	public static event Action<string,IronSourceError> onRewardedVideoAdShowFailedDemandOnlyEvent {
		add {
			if (_onRewardedVideoAdShowFailedDemandOnlyEvent == null || !_onRewardedVideoAdShowFailedDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdShowFailedDemandOnlyEvent += value;
			}
		}
		
		remove {
			if (_onRewardedVideoAdShowFailedDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdShowFailedDemandOnlyEvent -= value;
			}
		}
	}
	
	public void onRewardedVideoAdShowFailedDemandOnly (string args)
	{  
		if (_onRewardedVideoAdShowFailedDemandOnlyEvent != null && !String.IsNullOrEmpty (args)) {
			List<object> argList = IronSourceJSON.Json.Deserialize (args) as List<object>;
			IronSourceError sse = getErrorFromErrorObject(argList[1]);
			string instanceId = argList[0].ToString();
			_onRewardedVideoAdShowFailedDemandOnlyEvent (instanceId, sse);
		}
	}

	private static event Action<string,IronSourcePlacement> _onRewardedVideoAdClickedDemandOnlyEvent;

	public static event Action<string,IronSourcePlacement> onRewardedVideoAdClickedDemandOnlyEvent {
		add {
			if (_onRewardedVideoAdClickedDemandOnlyEvent == null || !_onRewardedVideoAdClickedDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdClickedDemandOnlyEvent += value;
			}
		}
		
		remove {
			if (_onRewardedVideoAdClickedDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onRewardedVideoAdClickedDemandOnlyEvent -= value;
			}
		}
	}
	
	public void onRewardedVideoAdClickedDemandOnly (string args)
	{ 
		if (_onRewardedVideoAdClickedDemandOnlyEvent != null && !String.IsNullOrEmpty (args)) {
			List<object> argList = IronSourceJSON.Json.Deserialize (args) as List<object>; 
			string instanceId = argList[0].ToString();
			IronSourcePlacement ssp = getPlacementFromObject (argList[1]);
			_onRewardedVideoAdClickedDemandOnlyEvent (instanceId, ssp);
		}	
	}

	// ******************************* Interstitial Events *******************************

	private static event Action _onInterstitialAdReadyEvent;

	public static event Action onInterstitialAdReadyEvent {
		add {
			if (_onInterstitialAdReadyEvent == null || !_onInterstitialAdReadyEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdReadyEvent += value;
			}
		}
		
		remove {
			if (_onInterstitialAdReadyEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdReadyEvent -= value;
			}
		}
	}
	
	public void onInterstitialAdReady ()
	{
		if (_onInterstitialAdReadyEvent != null)
			_onInterstitialAdReadyEvent ();
	}

	private static event Action<IronSourceError> _onInterstitialAdLoadFailedEvent;

	public static event Action<IronSourceError> onInterstitialAdLoadFailedEvent {
		add {
			if (_onInterstitialAdLoadFailedEvent == null || !_onInterstitialAdLoadFailedEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdLoadFailedEvent += value;
			}
		}
		
		remove {
			if (_onInterstitialAdLoadFailedEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdLoadFailedEvent -= value;
			}
		}
	}
	
	public void onInterstitialAdLoadFailed (string description)
	{
		if (_onInterstitialAdLoadFailedEvent != null) {
			IronSourceError sse = getErrorFromErrorObject (description);
			_onInterstitialAdLoadFailedEvent (sse);
		}
	}

	private static event Action _onInterstitialAdOpenedEvent;

	public static event Action onInterstitialAdOpenedEvent {
		add {
			if (_onInterstitialAdOpenedEvent == null || !_onInterstitialAdOpenedEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdOpenedEvent += value;
			}
		}
		
		remove {
			if (_onInterstitialAdOpenedEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdOpenedEvent -= value;
			}
		}
	}
	
	public void onInterstitialAdOpened (string empty)
	{
		if (_onInterstitialAdOpenedEvent != null) {
			_onInterstitialAdOpenedEvent ();
		}
	}

	private static event Action _onInterstitialAdClosedEvent;

	public static event Action onInterstitialAdClosedEvent {
		add {
			if (_onInterstitialAdClosedEvent == null || !_onInterstitialAdClosedEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdClosedEvent += value;
			}
		}
		
		remove {
			if (_onInterstitialAdClosedEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdClosedEvent -= value;
			}
		}
	}
	
	public void onInterstitialAdClosed (string empty)
	{
		if (_onInterstitialAdClosedEvent != null) {
			_onInterstitialAdClosedEvent ();
		}
	}

	private static event Action _onInterstitialAdShowSucceededEvent;

	public static event Action onInterstitialAdShowSucceededEvent {
		add {
			if (_onInterstitialAdShowSucceededEvent == null || !_onInterstitialAdShowSucceededEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdShowSucceededEvent += value;
			}
		}
		
		remove {
			if (_onInterstitialAdShowSucceededEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdShowSucceededEvent -= value;
			}
		}
	}
	
	public void onInterstitialAdShowSucceeded (string empty)
	{
		if (_onInterstitialAdShowSucceededEvent != null) {
			_onInterstitialAdShowSucceededEvent ();
		}
	}

	private static event Action<IronSourceError> _onInterstitialAdShowFailedEvent;

	public static event Action<IronSourceError> onInterstitialAdShowFailedEvent {
		add {
			if (_onInterstitialAdShowFailedEvent == null || !_onInterstitialAdShowFailedEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdShowFailedEvent += value;
			}
		}
		
		remove {
			if (_onInterstitialAdShowFailedEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdShowFailedEvent -= value;
			}
		}
	}
	
	public void onInterstitialAdShowFailed (string description)
	{
		if (_onInterstitialAdShowFailedEvent != null) {
			IronSourceError sse = getErrorFromErrorObject (description);
			_onInterstitialAdShowFailedEvent (sse);
		}	
	}

	private static event Action _onInterstitialAdClickedEvent;

	public static event Action onInterstitialAdClickedEvent {
		add {
			if (_onInterstitialAdClickedEvent == null || !_onInterstitialAdClickedEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdClickedEvent += value;
			}
		}
		
		remove {
			if (_onInterstitialAdClickedEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdClickedEvent -= value;
			}
		}
	}
	
	public void onInterstitialAdClicked (string empty)
	{
		if (_onInterstitialAdClickedEvent != null) {
			_onInterstitialAdClickedEvent ();
		}
	}

	// ******************************* Interstitial DemanOnly Events *******************************

	private static event Action<string> _onInterstitialAdReadyDemandOnlyEvent;

	public static event Action<string> onInterstitialAdReadyDemandOnlyEvent {
		add {
			if (_onInterstitialAdReadyDemandOnlyEvent == null || !_onInterstitialAdReadyDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdReadyDemandOnlyEvent += value;
			}
		}
		
		remove {
			if (_onInterstitialAdReadyDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdReadyDemandOnlyEvent -= value;
			}
		}
	}
	
	public void onInterstitialAdReadyDemandOnly (string instanceId)
	{
		if (_onInterstitialAdReadyDemandOnlyEvent != null)
			_onInterstitialAdReadyDemandOnlyEvent (instanceId);
	}


    private static event Action<string,IronSourceError> _onInterstitialAdLoadFailedDemandOnlyEvent;

	public static event Action<string,IronSourceError> onInterstitialAdLoadFailedDemandOnlyEvent {
		add {
			if (_onInterstitialAdLoadFailedDemandOnlyEvent == null || !_onInterstitialAdLoadFailedDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdLoadFailedDemandOnlyEvent += value;
			}
		}
		
		remove {
			if (_onInterstitialAdLoadFailedDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdLoadFailedDemandOnlyEvent -= value;
			}
		}
	}
	
	public void onInterstitialAdLoadFailedDemandOnly (string args)
	{
		if (_onInterstitialAdLoadFailedDemandOnlyEvent != null && !String.IsNullOrEmpty(args)) {
			List<object> argList = IronSourceJSON.Json.Deserialize (args) as List<object>;  
			IronSourceError err = getErrorFromErrorObject(argList[1]);
			string instanceId = argList[0].ToString();
			_onInterstitialAdLoadFailedDemandOnlyEvent (instanceId, err);
		}
	}

	private static event Action<string> _onInterstitialAdOpenedDemandOnlyEvent;

	public static event Action<string> onInterstitialAdOpenedDemandOnlyEvent {
		add {
			if (_onInterstitialAdOpenedDemandOnlyEvent == null || !_onInterstitialAdOpenedDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdOpenedDemandOnlyEvent += value;
			}
		}
		
		remove {
			if (_onInterstitialAdOpenedDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdOpenedDemandOnlyEvent -= value;
			}
		}
	}
	
	public void onInterstitialAdOpenedDemandOnly (string instanceId)
	{
		if (_onInterstitialAdOpenedDemandOnlyEvent != null) {
			_onInterstitialAdOpenedDemandOnlyEvent (instanceId);
		}
	}

    private static event Action<string> _onInterstitialAdClosedDemandOnlyEvent;

	public static event Action<string> onInterstitialAdClosedDemandOnlyEvent {
		add {
			if (_onInterstitialAdClosedDemandOnlyEvent == null || !_onInterstitialAdClosedDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdClosedDemandOnlyEvent += value;
			}
		}
		
		remove {
			if (_onInterstitialAdClosedDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdClosedDemandOnlyEvent -= value;
			}
		}
	}
	
	public void onInterstitialAdClosedDemandOnly (string instanceId)
	{
		if (_onInterstitialAdClosedDemandOnlyEvent != null) {
			_onInterstitialAdClosedDemandOnlyEvent (instanceId);
		}
	}

    private static event Action<string> _onInterstitialAdShowSucceededDemandOnlyEvent;

	public static event Action<string> onInterstitialAdShowSucceededDemandOnlyEvent {
		add {
			if (_onInterstitialAdShowSucceededDemandOnlyEvent == null || !_onInterstitialAdShowSucceededDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdShowSucceededDemandOnlyEvent += value;
			}
		}
		
		remove {
			if (_onInterstitialAdShowSucceededDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdShowSucceededDemandOnlyEvent -= value;
			}
		}
	}
	
	public void onInterstitialAdShowSucceededDemandOnly (string instanceId)
	{
		if (_onInterstitialAdShowSucceededDemandOnlyEvent != null) {
			_onInterstitialAdShowSucceededDemandOnlyEvent (instanceId);
		}
	}

    private static event Action<string, IronSourceError> _onInterstitialAdShowFailedDemandOnlyEvent;

	public static event Action<string, IronSourceError> onInterstitialAdShowFailedDemandOnlyEvent {
		add {
			if (_onInterstitialAdShowFailedDemandOnlyEvent == null || !_onInterstitialAdShowFailedDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdShowFailedDemandOnlyEvent += value;
			}
		}
		
		remove {
			if (_onInterstitialAdShowFailedDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdShowFailedDemandOnlyEvent -= value;
			}
		}
	}
	
	public void onInterstitialAdShowFailedDemandOnly (string args)
	{ 
		if (_onInterstitialAdLoadFailedDemandOnlyEvent != null && !String.IsNullOrEmpty (args)) {
			List<object> argList = IronSourceJSON.Json.Deserialize (args) as List<object>; 
			IronSourceError sse = getErrorFromErrorObject(argList[1]);
			string instanceId = argList[0].ToString();
			_onInterstitialAdShowFailedDemandOnlyEvent (instanceId, sse);
		}
	}

    private static event Action<string> _onInterstitialAdClickedDemandOnlyEvent;

	public static event Action<string> onInterstitialAdClickedDemandOnlyEvent {
		add {
			if (_onInterstitialAdClickedDemandOnlyEvent == null || !_onInterstitialAdClickedDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdClickedDemandOnlyEvent += value;
			}
		}
		
		remove {
			if (_onInterstitialAdClickedDemandOnlyEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdClickedDemandOnlyEvent -= value;
			}
		}
	}
	
	public void onInterstitialAdClickedDemandOnly (string instanceId)
	{
		if (_onInterstitialAdClickedDemandOnlyEvent != null) {
			_onInterstitialAdClickedDemandOnlyEvent (instanceId);
		}
	}


	// ******************************* Rewarded Interstitial Events *******************************

	private static event Action _onInterstitialAdRewardedEvent;
	
	public static event Action onInterstitialAdRewardedEvent {
		add {
			if (_onInterstitialAdRewardedEvent == null || !_onInterstitialAdRewardedEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdRewardedEvent += value;
			}
		}
		
		remove {
			if (_onInterstitialAdRewardedEvent.GetInvocationList ().Contains (value)) {
				_onInterstitialAdRewardedEvent -= value;
			}
		}
	}
	
	public void onInterstitialAdRewarded (string empty)
	{
		if (_onInterstitialAdRewardedEvent != null) {
			_onInterstitialAdRewardedEvent ();
		}
	}

	// ******************************* Offerwall Events *******************************	

	private static event Action _onOfferwallOpenedEvent;

	public static event Action onOfferwallOpenedEvent {
		add {
			if (_onOfferwallOpenedEvent == null || !_onOfferwallOpenedEvent.GetInvocationList ().Contains (value)) {
				_onOfferwallOpenedEvent += value;
			}
		}
		
		remove {
			if (_onOfferwallOpenedEvent.GetInvocationList ().Contains (value)) {
				_onOfferwallOpenedEvent -= value;
			}			
		}
	}
	
	public void onOfferwallOpened (string empty)
	{
		if (_onOfferwallOpenedEvent != null) {
			_onOfferwallOpenedEvent ();
		}
	}

	private static event Action<IronSourceError> _onOfferwallShowFailedEvent;

	public static event Action<IronSourceError> onOfferwallShowFailedEvent {
		add {
			if (_onOfferwallShowFailedEvent == null || !_onOfferwallShowFailedEvent.GetInvocationList ().Contains (value)) {
				_onOfferwallShowFailedEvent += value;
			}
		}
		
		remove {
			if (_onOfferwallShowFailedEvent.GetInvocationList ().Contains (value)) {
				_onOfferwallShowFailedEvent -= value;
			}
		}
	}
	
	public void onOfferwallShowFailed (string description)
	{
		if (_onOfferwallShowFailedEvent != null) {
			IronSourceError sse = getErrorFromErrorObject (description);
			_onOfferwallShowFailedEvent (sse);
		}
	}

	private static event Action _onOfferwallClosedEvent;

	public static event Action onOfferwallClosedEvent {
		add {
			if (_onOfferwallClosedEvent == null || !_onOfferwallClosedEvent.GetInvocationList ().Contains (value)) {
				_onOfferwallClosedEvent += value;
			}
		}
		
		remove {
			if (_onOfferwallClosedEvent.GetInvocationList ().Contains (value)) {
				_onOfferwallClosedEvent -= value;
			}		
		}
	}
	
	public void onOfferwallClosed (string empty)
	{
		if (_onOfferwallClosedEvent != null) {
			_onOfferwallClosedEvent ();
		}
	}

	private static event Action<IronSourceError> _onGetOfferwallCreditsFailedEvent;

	public static event Action<IronSourceError> onGetOfferwallCreditsFailedEvent {
		add {
			if (_onGetOfferwallCreditsFailedEvent == null || !_onGetOfferwallCreditsFailedEvent.GetInvocationList ().Contains (value)) {
				_onGetOfferwallCreditsFailedEvent += value;
			}
		}
		
		remove {
			if (_onGetOfferwallCreditsFailedEvent.GetInvocationList ().Contains (value)) {
				_onGetOfferwallCreditsFailedEvent -= value;
			}
		}
	}
	
	public void onGetOfferwallCreditsFailed (string description)
	{
		if (_onGetOfferwallCreditsFailedEvent != null) {
			IronSourceError sse = getErrorFromErrorObject (description);
			_onGetOfferwallCreditsFailedEvent (sse);

		}
	}

	private static event Action<Dictionary<string,object>> _onOfferwallAdCreditedEvent;

	public static event Action<Dictionary<string,object>> onOfferwallAdCreditedEvent {
		add {
			if (_onOfferwallAdCreditedEvent == null || !_onOfferwallAdCreditedEvent.GetInvocationList ().Contains (value)) {
				_onOfferwallAdCreditedEvent += value;
			}
		}

		remove {
			if (_onOfferwallAdCreditedEvent.GetInvocationList ().Contains (value)) {
				_onOfferwallAdCreditedEvent -= value;
			}
		}
	}

	public void onOfferwallAdCredited (string json)
	{
		if (_onOfferwallAdCreditedEvent != null)
			_onOfferwallAdCreditedEvent (IronSourceJSON.Json.Deserialize (json) as Dictionary<string,object>);
	}

	private static event Action<bool> _onOfferwallAvailableEvent;
	
	public static event Action<bool> onOfferwallAvailableEvent {
		add {
			if (_onOfferwallAvailableEvent == null || !_onOfferwallAvailableEvent.GetInvocationList ().Contains (value)) {
				_onOfferwallAvailableEvent += value;
			}
		}
		
		remove {
			if (_onOfferwallAvailableEvent.GetInvocationList ().Contains (value)) {
				_onOfferwallAvailableEvent -= value;
			}
		}
	}
	
	public void onOfferwallAvailable (string stringAvailable)
	{
		bool isAvailable = (stringAvailable == "true") ? true : false;
		if (_onOfferwallAvailableEvent != null)
			_onOfferwallAvailableEvent (isAvailable);
	}

	// ******************************* Banner Events *******************************	
	private static event Action _onBannerAdLoadedEvent;
	
	public static event Action onBannerAdLoadedEvent {
		add {
			if (_onBannerAdLoadedEvent == null || !_onBannerAdLoadedEvent.GetInvocationList ().Contains (value)) {
				_onBannerAdLoadedEvent += value;
			}
		}
		
		remove {
			if (_onBannerAdLoadedEvent.GetInvocationList ().Contains (value)) {
				_onBannerAdLoadedEvent -= value;
			}
		}
	}
	
	public void onBannerAdLoaded ()
	{
		if (_onBannerAdLoadedEvent != null)
			_onBannerAdLoadedEvent ();
	}
	
	private static event Action<IronSourceError> _onBannerAdLoadFailedEvent;
	
	public static event Action<IronSourceError> onBannerAdLoadFailedEvent {
		add {
			if (_onBannerAdLoadFailedEvent == null || !_onBannerAdLoadFailedEvent.GetInvocationList ().Contains (value)) {
				_onBannerAdLoadFailedEvent += value;
			}
		}
		
		remove {
			if (_onBannerAdLoadFailedEvent.GetInvocationList ().Contains (value)) {
				_onBannerAdLoadFailedEvent -= value;
			}
		}
	}
	
	public void onBannerAdLoadFailed (string description)
	{
		if (_onBannerAdLoadFailedEvent != null) {
			IronSourceError sse = getErrorFromErrorObject (description);
			_onBannerAdLoadFailedEvent (sse);
		}
		
	}

	private static event Action _onBannerAdClickedEvent;
	
	public static event Action onBannerAdClickedEvent {
		add {
			if (_onBannerAdClickedEvent == null || !_onBannerAdClickedEvent.GetInvocationList ().Contains (value)) {
				_onBannerAdClickedEvent += value;
			}
		}
		
		remove {
			if (_onBannerAdClickedEvent.GetInvocationList ().Contains (value)) {
				_onBannerAdClickedEvent -= value;
			}
		}
	}
	
	public void onBannerAdClicked ()
	{
		if (_onBannerAdClickedEvent != null)
			_onBannerAdClickedEvent ();
	}

	private static event Action _onBannerAdScreenPresentedEvent;
	
	public static event Action onBannerAdScreenPresentedEvent {
		add {
			if (_onBannerAdScreenPresentedEvent == null || !_onBannerAdScreenPresentedEvent.GetInvocationList ().Contains (value)) {
				_onBannerAdScreenPresentedEvent += value;
			}
		}
		
		remove {
			if (_onBannerAdScreenPresentedEvent.GetInvocationList ().Contains (value)) {
				_onBannerAdScreenPresentedEvent -= value;
			}
		}
	}
	
	public void onBannerAdScreenPresented ()
	{
		if (_onBannerAdScreenPresentedEvent != null)
			_onBannerAdScreenPresentedEvent ();
	}

	private static event Action _onBannerAdScreenDismissedEvent;
	
	public static event Action onBannerAdScreenDismissedEvent {
		add {
			if (_onBannerAdScreenDismissedEvent == null || !_onBannerAdScreenDismissedEvent.GetInvocationList ().Contains (value)) {
				_onBannerAdScreenDismissedEvent += value;
			}
		}
		
		remove {
			if (_onBannerAdScreenDismissedEvent.GetInvocationList ().Contains (value)) {
				_onBannerAdScreenDismissedEvent -= value;
			}
		}
	}
	
	public void onBannerAdScreenDismissed ()
	{
		if (_onBannerAdScreenDismissedEvent != null)
			_onBannerAdScreenDismissedEvent ();
	}

	private static event Action _onBannerAdLeftApplicationEvent;

	public static event Action onBannerAdLeftApplicationEvent {
		add {
			if (_onBannerAdLeftApplicationEvent == null || !_onBannerAdLeftApplicationEvent.GetInvocationList ().Contains (value)) {
				_onBannerAdLeftApplicationEvent += value;
			}
		}
		
		remove {
			if (_onBannerAdLeftApplicationEvent.GetInvocationList ().Contains (value)) {
				_onBannerAdLeftApplicationEvent -= value;
			}
		}
	}
	
	public void onBannerAdLeftApplication ()
	{
		if (_onBannerAdLeftApplicationEvent != null)
			_onBannerAdLeftApplicationEvent ();
	}

	private static event Action<string> _onSegmentReceivedEvent;
	public static event Action<string> onSegmentReceivedEvent {
		add {
			if (_onSegmentReceivedEvent == null || !_onSegmentReceivedEvent.GetInvocationList ().Contains (value)) {
				_onSegmentReceivedEvent += value;
			}
		}
		
		remove {
			if (_onSegmentReceivedEvent.GetInvocationList ().Contains (value)) {
				_onSegmentReceivedEvent -= value;
			}
		}
	}
	
	public void onSegmentReceived (string segmentName)
	{
		if (_onSegmentReceivedEvent != null)
			_onSegmentReceivedEvent (segmentName);
	}
	
	// ******************************* Helper methods *******************************	

		private IronSourceError getErrorFromErrorObject (object descriptionObject)
	{
		Dictionary<string,object> error = null;
		if (descriptionObject is IDictionary) {
			error = descriptionObject as Dictionary<string,object>;
		}
		else if (descriptionObject is String && !String.IsNullOrEmpty (descriptionObject.ToString())) {
			error = IronSourceJSON.Json.Deserialize (descriptionObject.ToString()) as Dictionary<string,object>;
		}

		IronSourceError sse = new IronSourceError (-1, "");
		if (error != null && error.Count > 0) {
			int eCode = Convert.ToInt32 (error [ERROR_CODE].ToString ());
			string eDescription = error [ERROR_DESCRIPTION].ToString ();
			sse = new IronSourceError (eCode, eDescription);
		} 
	
		return sse;
	}

	private IronSourcePlacement getPlacementFromObject (object placementObject)
	{		
		Dictionary<string,object> placementJSON = null;
		if (placementObject is IDictionary) {
			placementJSON = placementObject as Dictionary<string,object>;
		}
		else if (placementObject is String) {
			placementJSON = IronSourceJSON.Json.Deserialize (placementObject.ToString()) as Dictionary<string,object>;
		}

		IronSourcePlacement ssp = null;
		if (placementJSON != null && placementJSON.Count > 0) {
			int rewardAmount = Convert.ToInt32 (placementJSON ["placement_reward_amount"].ToString ());
			string rewardName = placementJSON ["placement_reward_name"].ToString ();
			string placementName = placementJSON ["placement_name"].ToString ();
		
			ssp = new IronSourcePlacement (placementName, rewardName, rewardAmount);
		}

		return ssp;
	}
}
