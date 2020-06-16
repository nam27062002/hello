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
public class PopupShopReferral : MonoBehaviour
{
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

    [SerializeField] private FriendCounter m_friendCounter;

    [Header("Rewards")]
    [SerializeField] private Transform m_rewardsContainer;
    [SerializeField] private GameObject m_referralRewardPrefab;

    [Header("Friends progression bar")]
    [SerializeField] private Transform m_friendsProgressionBar;

    [SerializeField] private GameObject m_friendIconDisabled;
    [SerializeField] private GameObject m_friendIconActive;
    [SerializeField] private GameObject m_friendIconHighlighted;


    // cache
    private OfferPackReferral m_pack = null;


    //------------------------------------------------------------------------//
    // GENERIC METHODS														  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// Initialization.
    /// </summary>
    private void Awake()
    {
        // Subscribe to external events
        Messenger.AddListener(MessengerEvents.REFERRAL_REWARDS_CLAIMED, ApplyRewards);

    }

    /// <summary>
    /// Destructor.
    /// </summary>
    private void OnDestroy()
    {
        // Unsubscribe from external events
        Messenger.RemoveListener(MessengerEvents.REFERRAL_REWARDS_CLAIMED, ApplyRewards);
    }

    //------------------------------------------------------------------------//
    // OTHER METHODS														  //
    //------------------------------------------------------------------------//


    /// <summary>
    /// Initialize the popup with a given pack's data.
    /// </summary>
    /// <param name="_pack">Pack.</param>
    public void InitFromOfferPack(OfferPack _pack)
    {

        // Make sure the offer pack is a referral offer
        if (!(_pack is OfferPackReferral))
        {
            Debug.LogError("The offer pack " + _pack + " is not of type referral");
            return;
        }

        // Store pack
        m_pack = (OfferPackReferral)_pack;

        Clear();

        if (m_friendCounter != null)
        {
            m_friendCounter.InitFromOfferPack(m_pack);
        }

        // Delay refresh a couple of frames, so the popup has tiem to open.
        // Otherwise there are problems with the DisableOnPopup component
        UbiBCN.CoroutineManager.DelayedCallByFrames(() =>
       {
           Refresh();
       }, 2);


    }

    /// <summary>
    /// Clear all the visuals and prepare for a new refresh
    /// </summary>
    public void Clear()
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
        OfferPackReferralReward lastReward = (OfferPackReferralReward)m_pack.items[m_pack.items.Count - 1];
        int maxFriends = lastReward.friendsRequired;

        // Friends progression is cyclic so when reaches the final milestone, it starts from the begining
        int friendsCount = UsersManager.currentUser.totalReferrals;
        if (friendsCount > maxFriends)
            friendsCount = (friendsCount - 1) % maxFriends + 1;


        // Friends progresion bar
        for (int i = 0; i < maxFriends + 1; i++)
        {
            GameObject friendIcon;

            // Create a friend icon
            if (i <= friendsCount)
            {
                if (IsMilestone(i))
                {
                    friendIcon = Instantiate(m_friendIconHighlighted);
                }
                else
                {
                    friendIcon = Instantiate(m_friendIconActive);
                }
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

            // Find the state of the reward (claimed, ready to claim, etc)
            OfferPackReferralReward.State state;
            if (friendsCount == reward.friendsRequired)
            {
                state = OfferPackReferralReward.State.READY_TO_CLAIM;
            }
            else if (friendsCount > reward.friendsRequired)
            {
                state = OfferPackReferralReward.State.CLAIMED;
            }
            else
            {
                state = OfferPackReferralReward.State.NOT_AVAILABLE;
            }

            // Initialize reward preview
            rewardPreview.GetComponent<OfferItemSlotReferral>().InitFromItem(reward, state);

            // Calculate the reward preview horizontal position, so it matches the friend icon in the progression bar
            float unitWidth = (1f / (maxFriends + 1));
            float anchorX = unitWidth * reward.friendsRequired + unitWidth * .5f; // Add .5f to center the reward

            RectTransform rect = rewardPreview.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(anchorX, rect.anchorMin.y);
            rect.anchorMax = new Vector2(anchorX, rect.anchorMax.y);
        }
    }

    /// <summary>
    /// Check if there is a reward milestone for the current amount of friends
    /// </summary>
    /// <param name="_friendsAmount">The amount of friends to check</param>
    /// <returns>True if it's a milestone</returns>
	private bool IsMilestone(int _friendsAmount)
    {
        foreach (OfferPackReferralReward reward in m_pack.items)
        {
            if (reward.friendsRequired == _friendsAmount)
                return true;

        }

        return false;
    }


    /// <summary>
    ///  Transition to pending rewards screen and give all the claimed rewards to the player
    /// </summary>
    private void ApplyRewards ()
    {
        // Push all the pending rewards
        while (ReferralManager.Instance.pendingRewards.Count > 0)
        {
            OfferPackReferralReward next = ReferralManager.Instance.pendingRewards.Dequeue();
            UsersManager.currentUser.PushReward(next.reward);
        }

        // Close all open popups
        PopupManager.Clear(true);

        // Move to the rewards screen
        PendingRewardScreen scr = InstanceManager.menuSceneController.GetScreenData(MenuScreen.PENDING_REWARD).ui.GetComponent<PendingRewardScreen>();
        scr.StartFlow(false);   // No intro
        InstanceManager.menuSceneController.GoToScreen(MenuScreen.PENDING_REWARD);

    }

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// The popup has been opened.
    /// </summary>
    public void OnShowPostAnimation()
    {
        // Update pack's view tracking
        m_pack.NotifyPopupDisplayed();
    }


    /// <summary>
    /// The user has pressed the INVITE button
    /// </summary>
    public void OnInviteButtonPressed()
    {

    }


    /// <summary>
    /// The user has pressed the CLAIM reward button
    /// </summary>
	public void OnClaimButtonPressed()
    {

    }

}