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
using XPromo;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// 
/// </summary>
public class XPromoRewardMarker : MonoBehaviour {


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
	private Localizer m_dayLabel;

	[SerializeField]
	private GameObject m_clockIcon;

	[SerializeField]
	private TextMeshProUGUI m_timerCountdown;

	[SerializeField]
	private GameObject m_greenTick;

	[SerializeField]
	private GameObject m_greyTick;

	[SerializeField]
	private GameObject m_bgroundCollected;

	[SerializeField]
	private GameObject m_bgroundReady;

	[SerializeField]
	private GameObject m_bgroundUnavailable;

	// Internal
	private LocalReward m_reward;
    public LocalReward reward
    {
        get { return m_reward;  }
    }

	private XPromoRewardPreview m_preview;

    // Cache
	private LocalReward.State m_rewardState;

	// The marker has been selected
	private bool m_selected;
	public bool selected
	{
		set
		{
			m_selected = value;
		}
	}

	private int m_index;

	public delegate void OnRewardSelectedDelegate(int index);
	public OnRewardSelectedDelegate rewardSelectedDelegate;

	// Internal
	private float m_timer;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	/// <summary>
	/// Called every frame.
	/// </summary>
	private void Update() {
		// Refresh periodically for better performance
		if (m_timer <= 0)
		{
			m_timer = 1f; // Refresh every second
			Refresh();
		}
		m_timer -= Time.deltaTime;


	}


    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// Initilizes a marker with the reward data
    /// </summary>
    /// <param name="_index">The position in the cycle</param>

    public void Init (int _index)
    {

		
		m_index = _index;
		m_reward = XPromoManager.instance.xPromoCycle.localRewards[_index];

		// Show a separator between daily markers
		bool showSeparator = (m_index < XPromoManager.instance.xPromoCycle.cycleSize - 1);
		m_separator.SetActive(showSeparator);

        // Initialize the reward preview

        if (m_reward is XPromo.LocalRewardHD)
        {
			m_preview = Instantiate(m_HDPreviewPrefab, m_previewContainer);
        }
        else if (m_reward is XPromo.LocalRewardHSE)
        {
			m_preview = Instantiate(m_HSEPreviewPrefab, m_previewContainer);
		}

        if (m_preview != null)
        {
			m_preview.Init(m_reward);
        }


        // Initialize the UI elements
		Refresh();

    }


    /// <summary>
    /// Update all the UI elements
    /// </summary>
    public void Refresh ()
    {

		// Find the state
		m_rewardState = XPromoManager.instance.xPromoCycle.GetRewardState(m_index);


		// Initialize day marker label
		m_dayLabel.Localize("TID_DAILY_LOGIN_DAY", m_reward.day.ToString());


		// Show/hide UI elements
		m_clockIcon.SetActive(m_rewardState == LocalReward.State.COUNTDOWN);
		m_greenTick.SetActive(m_rewardState == LocalReward.State.COLLECTED);
		m_greyTick.SetActive(m_rewardState != LocalReward.State.COLLECTED);
		m_bgroundCollected.SetActive(m_rewardState == LocalReward.State.COLLECTED);
		m_bgroundReady.SetActive(m_rewardState == LocalReward.State.READY);
		m_bgroundUnavailable.SetActive(m_rewardState == LocalReward.State.LOCKED);
		m_bgroundUnavailable.SetActive(m_rewardState == LocalReward.State.COUNTDOWN);
		m_timerCountdown.gameObject.SetActive(m_rewardState == LocalReward.State.COUNTDOWN);


		// Initialize timer
		if (m_rewardState == LocalReward.State.COUNTDOWN)
		{
			string timeLeft = TimeUtils.FormatTime(XPromoManager.instance.xPromoCycle.timeToCollection.TotalSeconds, TimeUtils.EFormat.ABBREVIATIONS_WITHOUT_0_VALUES, 2);
			m_timerCountdown.text = timeLeft;

		}

		// Cascade down the refresh call
		m_preview.selected = m_selected;
		m_preview.state = m_rewardState;
		m_preview.Refresh();


	}


    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//

    /// <summary>
    /// The player clicked over the reward
    /// </summary>
    public void OnClick ()
    {
        // Let the popup know
		rewardSelectedDelegate(m_index);

		Refresh();
    }

}