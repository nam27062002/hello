// ResultsScreenMissionPill.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 13/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Single mission pill for the results screen.
/// </summary>
public class ResultsScreenMissionPill : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private ShowHideAnimator m_animator = null;
	public ShowHideAnimator animator {
		get { return m_animator; }
	}

	[Space]
	[SerializeField] private BaseIcon m_missionIcon = null;
    [SerializeField] private TextMeshProUGUI m_missionText = null;
	[SerializeField] private TextMeshProUGUI m_rewardText = null;

    [Space]
    [SerializeField] private GameObject m_targetZone = null;
    [SerializeField] private Localizer m_targetZoneText = null;

    public TextMeshProUGUI rewardText {
		get { return m_rewardText; }
	}

	// Internal
	private Mission m_mission = null;
	public Mission mission {
		get { return m_mission; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required fields
		Debug.Assert(m_missionIcon != null, "Required field not initialized!");
		Debug.Assert(m_missionText != null, "Required field not initialized!");
		Debug.Assert(m_rewardText != null, "Required field not initialized!");
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {

	}

	//------------------------------------------------------------------------//
	// ResultsScreenCarouselPill IMPLEMENTATION								  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the pill with the data of a given mission object.
	/// </summary>
	/// <param name="_mission">The mission to be used for initialization.</param>
	public void InitFromMission(Mission _mission) {
		// Store mission
		m_mission = _mission;

		// Aux vars
		if(m_mission == null) return;

		// Mission description
		m_missionText.text = m_mission.objective.GetDescription();

		// Reward
        UIConstants.IconType icon = UIConstants.IconType.NONE;
        switch (m_mission.reward.currency) {
            case UserProfile.Currency.SOFT: icon = UIConstants.IconType.COINS; break;
            case UserProfile.Currency.GOLDEN_FRAGMENTS: icon = UIConstants.IconType.GOLDEN_FRAGMENTS; break;
        }
        m_rewardText.text = UIConstants.GetIconString(m_mission.reward.amount, icon, UIConstants.IconAlignment.LEFT);

        // Refresh zone
        RefreshZone(m_targetZone, m_targetZoneText, m_mission.objective.zone);

        // Get the icon definition
        string iconSku = m_mission.def.GetAsString("icon");

        // The BaseIcon component will load the proper image or 3d model according to iconDefinition.xml
        m_missionIcon.LoadIcon(iconSku);
        m_missionIcon.gameObject.SetActive(true);

    }

	/// <summary>
	/// Check whether this pill must be displayed on the carousel or not.
	/// </summary>
	/// <returns><c>true</c> if the pill must be displayed on the carousel, <c>false</c> otherwise.</returns>
	public bool MustBeDisplayed() {
		// Must be displayed if mission objective was completed!
		if(m_mission == null) return false;
		return m_mission.objective.isCompleted;
	}

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Refreshes the zone info
    /// </summary>
    /// <param name="_targetZone">Zone info group</param>
    /// <param name="_targetZoneText">Localizer of the zone</param>
    /// <param name="_zoneSku">Sku of the zone as defined in zoneTriggersDefinitions</param>
    private void RefreshZone(GameObject _targetZone, Localizer _targetZoneText, string _zoneSku)
    {
        if (_targetZone == null || _targetZoneText == null)
            return;

        // Try to find the zone in the content
        DefinitionNode zoneDef = DefinitionsManager.SharedInstance.GetDefinitionByVariable(DefinitionsCategory.ZONE_TRIGGERS, "sku", _zoneSku);

        _targetZone.SetActive(true);
        _targetZoneText.gameObject.SetActive(true);

        if (string.IsNullOrEmpty(_zoneSku))
        {
            // Where
            _targetZoneText.Localize("TID_MISSIONS_WHERE_ANY_LEVEL");

        }
        else
        {
            // Show the localized name of the zone
            _targetZoneText.Localize(zoneDef.GetAsString("tidName"));
        }

        return;

    }


    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
}