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
	[SerializeField] private Image m_missionIcon = null;
    [SerializeField] private GameObject m_mission3dIcon = null;
    [SerializeField] private TextMeshProUGUI m_missionText = null;
	[SerializeField] private TextMeshProUGUI m_rewardText = null;
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


        // Check if the icon is an image or a 3d model
        if (m_mission.def.GetAsBool("is3dIcon"))
        {

            // Icon is a 3d Model
            m_mission3dIcon.gameObject.SetActive(true);
            m_mission3dIcon.transform.DestroyAllChildren(true);

            GameObject model = Instantiate(Resources.Load<GameObject>(UIConstants.MISSION_3D_ICONS_PATH + m_mission.def.GetAsString("icon")));
            model.transform.SetParent(m_mission3dIcon.transform, false);

            m_missionIcon.gameObject.SetActive(false);

        }
        else
        {

            // Icon is a sprite
            m_missionIcon.gameObject.SetActive(true);
            m_missionIcon.sprite = Resources.Load<Sprite>(UIConstants.MISSION_ICONS_PATH + m_mission.def.GetAsString("icon"));

            m_mission3dIcon.gameObject.SetActive(false);

        }

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

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}