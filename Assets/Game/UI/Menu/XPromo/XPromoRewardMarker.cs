// XPromoDayMarker.cs
// Hungry Dragon
// 
// Created by Jose M. Olea on 04/09/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using TMPro;
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class XPromoRewardMarker : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//


    [Separator("Reward preview")]
	[SerializeField]
	private Transform m_previewContainer;

	[SerializeField]
	private XPromoRewardPreview m_HDPreviewPrefab;

	[SerializeField]
	private XPromoRewardPreview m_HSEPreviewPrefab;


	[Separator("Day markers")]
	[SerializeField]
	private GameObject m_separator;

	[SerializeField]
	private TextMeshProUGUI m_dayLabel;

	[SerializeField]
	private GameObject m_clockIcon;

	[SerializeField]
	private TextMeshProUGUI m_timerCountdown;

	[SerializeField]
	private GameObject m_greenTick;

	[SerializeField]
	private GameObject m_bgroundCollected;

	[SerializeField]
	private GameObject m_bgroundReady;

	[SerializeField]
	private GameObject m_bgroundUnavailable;

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

    /// <summary>
    /// Initilizes a marker with the reward data
    /// </summary>
    /// <param name="_reward">The local reward to be displayed</param>
    /// <param name="_showSeparator">If true, adds a separator after the marker.</param>
    public void Init (XPromo.LocalReward _reward, bool _showSeparator)
    {
		m_separator.SetActive(_showSeparator);
    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
}