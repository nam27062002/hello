// UIHideConditionallyByProfile.cs
// Hungry Dragon
// 
// Created by J.M.Olea on 03/03/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System.Collections.Generic;
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Hide UI elements if the profile quality is lower than the one specified
/// </summary>
public class UIHideConditionallyByProfile : MonoBehaviour, IBroadcastListener
{
    //------------------------------------------------------------------------//
    // MEMBERS AND PROPERTIES												  //
    //------------------------------------------------------------------------//
    [Tooltip("The elements will be hidden if the quality is lower or equal than this value")]
    [SerializeField] private int lowerQualityThreshold;
    [SerializeField] List<GameObject> elementsToHide;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

        Broadcaster.AddListener(BroadcastEventType.QUALITY_PROFILE_CHANGED, this);
        
        // Initial update
        UpdateVisibility(lowerQualityThreshold);

    }

    private void OnDestroy()
    {
        Broadcaster.RemoveListener(BroadcastEventType.QUALITY_PROFILE_CHANGED, this);
    }

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Hide/show all the elements if the profile quality of the app is lower (or equal) than the specified value
    /// </summary>
    /// <param name="_lowerQuality">Elements will be hidden in devices with quality lower or equal than this value</param>
    private void UpdateVisibility(int _lowerQuality)
    {
        bool show = FeatureSettingsManager.instance.GetCurrentProfileLevel() > _lowerQuality;

        foreach (GameObject obj in elementsToHide)
        {
            obj.SetActive(show);
        }
    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// External event listener.
    /// </summary>
    /// <param name="eventType"></param>
    /// <param name="broadcastEventInfo"></param>
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch (eventType)
        {
            case BroadcastEventType.QUALITY_PROFILE_CHANGED:
                {
                    // Update the elements visibility after the quality profile has changed
                    UpdateVisibility(lowerQualityThreshold);
                }
                break;
        }
    }
}