// PopupRemoveAdsOffer.cs
// Hungry Dragon
// 
// Created by Jose Maria Olea
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for the Featured Offer popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupShopReferral : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Economy/PF_PopupShopReferral";

	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private ShopReferralPill m_referralPill = null;

	// Buttons
	[SerializeField] private GameObject m_buttonInvite;
	[SerializeField] private GameObject m_buttonInviteMore;
	[SerializeField] private GameObject m_buttonClaim;

	[Header("Rewards")]
	[SerializeField] private Transform m_rewardsContainer;
	[SerializeField] private GameObject m_referralRewardPrefab;

	[Header("Friends progression bar")]
	[SerializeField] private Transform m_friendsProgressionBar;

	[SerializeField] private GameObject m_friendIconDisabled;
	[SerializeField] private GameObject m_friendIconActive;
	[SerializeField] private GameObject m_friendIconHighlighted;



    //debug
	public int friends = 8;

	// cache
	private OfferPackReferral m_pack = null;

	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Subscribe to external events
		//m_singleItemLayoutPill.OnPurchaseSuccess.AddListener(OnPurchaseSuccessful);
	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events
		//m_singleItemLayoutPill.OnPurchaseSuccess.RemoveListener(OnPurchaseSuccessful);
	}

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//


    /// <summary>
    /// Initialize the popup with a given pack's data.
    /// </summary>
    /// <param name="_pack">Pack.</param>
    public void InitFromOfferPack(OfferPack _pack) {

        // Make sure the offer pack is a referral offer
        if (! ( _pack is OfferPackReferral))
        {
			Debug.LogError("The offer pack " + _pack + " is not of type referral");
			return;
        }

		// Store pack
		m_pack = (OfferPackReferral) _pack;

		Clear();

		// Delay refresh a couple of frames, so the popup has tiem to open.
        // Otherwise there are problems with the DisableOnPopup component
		UbiBCN.CoroutineManager.DelayedCallByFrames( () =>
        {
		    Refresh();
	    }, 2);
		

	}

    /// <summary>
    /// Clear all the visuals and prepare for a new refresh
    /// </summary>
    public void Clear ()
    {
        // Clear the progression bar (remove mockups icons)
		m_friendsProgressionBar.DestroyAllChildren(true);

        // Clear the rewards panel
		m_rewardsContainer.DestroyAllChildren(true);
    }


    /// <summary>
    /// Update the visuals
    /// </summary>
    public void Refresh()
    {
		OfferPackReferralReward lastReward = (OfferPackReferralReward) m_pack.items[m_pack.items.Count - 1];
		int maxFriends = lastReward.friendsRequired;

		// Friends progresion bar
		for (int i=0; i< maxFriends + 1; i++)
        {
			GameObject friendIcon;

			// Create a friend icon
			if (i <= friends)
            {
				friendIcon = Instantiate(m_friendIconActive);
			}
            else
            {
				friendIcon = Instantiate(m_friendIconDisabled);
			}

			friendIcon.transform.SetParent(m_friendsProgressionBar, false);
        }

        // Reward items
        foreach (OfferPackReferralReward reward in m_pack.items)
        {

            GameObject rewardPreview = Instantiate(m_referralRewardPrefab);
			rewardPreview.transform.SetParent(m_rewardsContainer, false);

            // Initialize reward preview
			rewardPreview.GetComponent<OfferItemSlot>().InitFromItem(reward);

			// Calculate the reward preview horizontal position, so it matches the friend icon in the progression bar
			float unitWidth = (1f / (maxFriends + 1));
			float anchorX = unitWidth * reward.friendsRequired + unitWidth * .5f; // Add .5f to center the reward

			RectTransform rect = rewardPreview.GetComponent<RectTransform>();
			rect.anchorMin = new Vector2(anchorX, rect.anchorMin.y);
			rect.anchorMax = new Vector2(anchorX, rect.anchorMax.y);
		}
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The popup has been opened.
	/// </summary>
	public void OnShowPostAnimation() {
		// Update pack's view tracking
		m_pack.NotifyPopupDisplayed();
	}


    /// <summary>
    /// The user has pressed the INVITE button
    /// </summary>
    public void OnInviteButtonPressed ()
    {

    }


    /// <summary>
    /// The user has pressed the CLAIM reward button
    /// </summary>
	public void OnClaimButtonPressed ()
	{

	}

}