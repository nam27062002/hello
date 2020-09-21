// PopupXPromoRewards.cs
// Hungry Dragon
// 
// Created by Jose M. Olea on 03/09/2020.
// Copyright (c) 2020 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using XPromo;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for the XPromo popup
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupXPromo : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Menu/PF_PopupXPromo";

	private const string TID_OPEN_HSE = "TID_XPROMO_OPEN_HSE";
	private const string TID_INSTALL_HSE = "TID_XPROMO_INSTALL_HSE";

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	[SerializeField] protected XPromoRewardMarker m_rewardMarkerPrefab;

	[SerializeField] protected Transform m_rewardsContainer;

	// Horizontal Scroller
	[SerializeField] private ScrollRect m_scrollRect;

	// Buttons
	[SerializeField] protected GameObject m_buttonLeft;
    [SerializeField] protected GameObject m_buttonRight;
    [SerializeField] protected GameObject m_buttonCollectHD;
    [SerializeField] protected GameObject m_buttonCollectHSE;

	[Space]
	[SerializeField] private Transform m_currencySpawnPoint = null;

	[Space]
	[SerializeField] private NumberTextAnimator m_coinsCounter = null;
	[SerializeField] private NumberTextAnimator m_pcCounter = null;


	// Internal
	private List<XPromoRewardMarker> m_rewardMarkers;

	private int m_selectedIndex; // The currently selected reward

    private LocalReward selectedReward
    {
        get
        {
			return m_rewardMarkers[m_selectedIndex].reward;
        }
    }

	private bool timerReachedZero = false;
	private float m_timer;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_rewardMarkers = new List<XPromoRewardMarker>();
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

		// Get the list of rewards in one xpromo cycle
		List<XPromo.LocalReward> rewards = XPromoManager.instance.xPromoCycle.localRewards;


        for( int i=0; i< XPromoManager.instance.xPromoCycle.cycleSize; i++)
        {
			XPromo.LocalReward reward = rewards[i];

			XPromoRewardMarker newMarker = Instantiate(m_rewardMarkerPrefab, m_rewardsContainer);

            // Position the new marker before the right spacer (thats the last element)
			newMarker.transform.SetSiblingIndex(m_rewardsContainer.childCount - 2);

            // If this is the last element, do not display a separator
			bool lastElement = (i == rewards.Count - 1);

            // Initialize the marker with the reward index
            newMarker.Init(i);

            // Initialize delegate for player clicking in the rewards
			newMarker.rewardSelectedDelegate = OnRewardSelected;

            // Keep a list with all the markers
			m_rewardMarkers.Add(newMarker);

		}



		// Select the next reward to collect
		m_selectedIndex = XPromoManager.instance.xPromoCycle.nextRewardIdx;

		// Wait one frame and scroll to it
		UbiBCN.CoroutineManager.DelayedCallByFrames(() => {

			ScrollToItem(m_rewardMarkers[m_selectedIndex].transform);

		}, 1);


		// Update the currencies counter (if we do it in Refresh() we interrupt the counter animation)
		m_coinsCounter.SetValue(UsersManager.currentUser.coins, false);
		m_pcCounter.SetValue(UsersManager.currentUser.pc, false);


		// Refresh the UI
		Refresh();



	}

    public void Update()
	{   // Refresh periodically for better performance
		if (m_timer <= 0)
		{
			m_timer = 1f; // Refresh every second

			// If the countdown reaches zero, refresh all the UI
			if (!timerReachedZero && XPromoManager.instance.xPromoCycle.timeToCollection.TotalSeconds <= 0)
			{
				// Use this flag to make sure this only happens the first time
				timerReachedZero = true;

				Refresh();
			}
		}
		m_timer -= Time.deltaTime;

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
	/// Scroll the viewport to the selected category
	/// </summary>
	/// <param name="anchor"></param>
	private void ScrollToItem(Transform anchor)
	{

		if (anchor != null)
		{

			// Create a tweener to animate the scroll
			m_scrollRect.DOGoToItem(anchor, .5f, 0.001f)
			.SetEase(Ease.OutBack);

		}
	}


    /// <summary>
    /// Update visual elements in the UI
    /// </summary>
    public void Refresh()
    {
        // Check the status of the reward we have selected
		LocalReward currentReward = XPromoManager.instance.xPromoCycle.localRewards[m_selectedIndex];
		LocalReward.State rewardState = XPromoManager.instance.xPromoCycle.GetRewardState(m_selectedIndex);


        // Can the reward be collected?
		bool collectable = false;
        switch (rewardState)
        {
			case LocalReward.State.READY:
				collectable = true;
				break;
			case LocalReward.State.COLLECTED:
                if (currentReward is LocalRewardHSE)
                {
                    // For HSE show always the collect button. Even if the reward was already collected.
					collectable = true;
                }
				break;

		}


        // Show the proper collect buttons according to the destination game
		m_buttonCollectHD.SetActive(collectable == true && currentReward is LocalRewardHD);
		m_buttonCollectHSE.SetActive(collectable == true && currentReward is LocalRewardHSE);

		// Show the arrow buttons
		m_buttonLeft.SetActive(m_selectedIndex > 0);
		m_buttonRight.SetActive(m_selectedIndex < m_rewardMarkers.Count - 1);

        // Detect if HSE is installed, and show the proper button label
        if (m_buttonCollectHSE.activeInHierarchy)
        {
			string buttonTID = XPromoManager.IsHungrySharkGameInstalled() ? TID_OPEN_HSE : TID_INSTALL_HSE;

			m_buttonCollectHSE.GetComponentInChildren<Localizer>().Localize(buttonTID);
            
        }

		// Cascade down the refresh
		for ( int i=0; i<m_rewardMarkers.Count; i++)
        {
            // Is this marker selected?
			XPromoRewardMarker marker = m_rewardMarkers[i];
			marker.selected = (m_selectedIndex == i);

			marker.Refresh();
        }

	}

	/// <summary>
	/// Transfer coins from main screen to counter.
	/// </summary>
	public void TransferCoins()
	{
		// Update counter
		m_coinsCounter.SetValue(UsersManager.currentUser.coins, true);

		ParticlesTrailFX m_coinsFX = ParticlesTrailFX.LoadAndLaunch(
			ParticlesTrailFX.COINS,
			this.GetComponentInParent<Canvas>().transform,
			m_currencySpawnPoint.position + new Vector3(0f, 0f, -0.5f),        // Offset Z so the coins don't collide with the UI elements
			m_coinsCounter.transform.position + new Vector3(0f, 0f, -0.5f)
		);
		m_coinsFX.totalDuration = m_coinsCounter.duration * 0.5f;   // Match the text animator duration (more or less)
		m_coinsFX.OnFinish.AddListener(() => { m_coinsFX = null; });

	}

	/// <summary>
	/// Transfer gems from main screen to counter.
	/// </summary>
	public void TransferGems()
	{
		// Update counter
		m_pcCounter.SetValue(UsersManager.currentUser.pc, true);

		ParticlesTrailFX m_gemsFX = ParticlesTrailFX.LoadAndLaunch(
			ParticlesTrailFX.PC,
			this.GetComponentInParent<Canvas>().transform,
			m_currencySpawnPoint.position + new Vector3(0f, 0f, -0.5f),        // Offset Z so the coins don't collide with the UI elements
			m_pcCounter.transform.position + new Vector3(0f, 0f, -0.5f)
		);
		m_gemsFX.totalDuration = m_pcCounter.duration * 0.5f;   // Match the text animator duration (more or less)
		m_gemsFX.OnFinish.AddListener(() => { m_gemsFX = null; });

	}

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
		LocalReward reward = XPromoManager.instance.xPromoCycle.OnCollectReward( m_selectedIndex );

        // At this point the reward has been collected. We just need to deal with the UI feedback.

    	if (reward is LocalRewardHSE)
        {
            // No feedback needed here
        }
		if (reward is LocalRewardHD)
		{

			Metagame.Reward rewardContent = ((LocalRewardHD)reward).reward;

			if (rewardContent is Metagame.RewardCurrency)
			{
				// Show trail of coins/gems
                if (rewardContent.type == "sc")
                {
					TransferCoins();
                }
                else if (rewardContent.type == "pc")
                {
					TransferGems();
                }
            
			}
			else
			{

				// Move to the rewards screen
				PendingRewardScreen scr = InstanceManager.menuSceneController.GetScreenData(MenuScreen.PENDING_REWARD).ui.GetComponent<PendingRewardScreen>();
				scr.StartFlow(false);   // No intro
				InstanceManager.menuSceneController.GoToScreen(MenuScreen.PENDING_REWARD);

				// Close the popup now
				GetComponent<PopupController>().Close(true);

                // No more feedback needed
                return;
                
			}

		}


		// Refresh the UI
		Refresh();


		// Reset the countdown flag
		timerReachedZero = false;

		// Select the next reward to collect
		m_selectedIndex = XPromoManager.instance.xPromoCycle.nextRewardIdx;

		// Wait one frame and scroll to it
		UbiBCN.CoroutineManager.DelayedCallByFrames(() => {

			ScrollToItem(m_rewardMarkers[m_selectedIndex].transform);

			Refresh();

		}, 1);
	}

    /// <summary>
    /// The right arrow has been clicked
    /// </summary>
    public void OnNextRewardButton()
    {
        // Check that there is a next item
		if (m_selectedIndex < m_rewardMarkers.Count - 1)
		{
			m_selectedIndex++;

			// Scroll to it
			ScrollToItem(m_rewardMarkers[m_selectedIndex].transform);

            // Update visuals
			Refresh();
		}
	}

    /// <summary>
    /// The left arrow has been clicked
    /// </summary>
	public void OnPreviousRewardButton()
    {
        // Check that there is a previous item
		if (m_selectedIndex > 0)
		{
			m_selectedIndex--;

            // Scroll to it
			ScrollToItem(m_rewardMarkers[m_selectedIndex].transform);

			// Update visuals
			Refresh();
		}
	}

    /// <summary>
    /// The player selected a reward item in the scroll container
    /// </summary>
    /// <param name="_index"></param>
    public void OnRewardSelected (int _index)
    {
		m_selectedIndex = _index;

		// Scroll to the reward
		ScrollToItem(m_rewardMarkers[m_selectedIndex].transform);

		// Update visuals
		Refresh();

	}


}