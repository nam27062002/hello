// PopupXPromoRewards.cs
// Hungry Dragon
// 
// Created by Jose M. Olea on 03/09/2020.
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
/// Controller for the XPromo popup
/// </summary>
public class PopupXPromo : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Menu/PF_PopupXPromo";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

    [SerializeField]
	protected XPromoRewardMarker m_rewardMarkerPrefab;

	[SerializeField]
	protected Transform m_rewardsContainer;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

		// Create a reward marker for each reward in the xpromotion cycle
		List<XPromo.LocalReward> rewards = XPromoManager.instance.xPromoCycle.GetCycleRewards();

		int counter = 0;
        foreach (XPromo.LocalReward reward in rewards)
        {
			XPromoRewardMarker newMarker = Instantiate(m_rewardMarkerPrefab, m_rewardsContainer);

            // If this is the last element, do not display a separator
			bool lastElement = (counter == rewards.Count - 1);

            // Initialize the marker with the reward data
            newMarker.Init(reward, !lastElement);

			counter++;

		}

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
	/// Called every frame.
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//

	/// <summary>
	/// The dismiss button has been pressed.
	/// </summary>
	public void OnCloseButton()
	{
		// Just close the poopup
		GetComponent<PopupController>().Close(true);
	}

	/// <summary>
	/// Android back button has been pressed.
	/// </summary>
	public void OnBackButton()
	{
		OnCloseButton();
	}

    /// <summary>
    /// Button collect/open HSE pressed
    /// </summary>
    public void OnCollectRewardButton()
    {

    }

    /// <summary>
    /// The right arrow has been clicked
    /// </summary>
    public void OnNextRewardButton()
    {

    }

    /// <summary>
    /// The left arrow has been clicked
    /// </summary>
	public void OnPreviousRewardButton()
    {

    }


}