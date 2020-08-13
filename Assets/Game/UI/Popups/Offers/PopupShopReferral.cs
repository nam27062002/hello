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

    private bool m_inviteAlreadyPressed = false;
    public bool inviteAlreadyPressed
    {
        get { return m_inviteAlreadyPressed; }
        set { m_inviteAlreadyPressed = value; }
    }

    // cache
    private OfferPackReferral m_pack = null;
    private PopupController m_loadingPopup = null;


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
        Messenger.AddListener<FGOL.Server.Error>(MessengerEvents.REFERRAL_REWARDS_CLAIM_RESPONSE_RECEIVED, OnClaimResponseReceived);
    }

    /// <summary>
    /// Destructor.
    /// </summary>
    private void OnDestroy()
    {
        // Unsubscribe from external events
        Messenger.RemoveListener(MessengerEvents.REFERRAL_REWARDS_CLAIMED, ApplyRewards);
        Messenger.RemoveListener<FGOL.Server.Error>(MessengerEvents.REFERRAL_REWARDS_CLAIM_RESPONSE_RECEIVED, OnClaimResponseReceived);

        // If we had loaded a popup, destroy it now
        if(m_loadingPopup != null) {
            GameObject.Destroy(m_loadingPopup.gameObject);
		}
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

        inviteAlreadyPressed = false; 
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
        for (int i = 0; i < maxFriends ; i++)
        {
            GameObject friendIcon;

            // Create a friend icon
            if (i < friendsCount)
            {
                if (IsMilestone(i + 1))
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

            // Is this reward in the list of unlocked rewards (ready to be claimed)
            bool readyToClaim = UsersManager.currentUser.unlockedReferralRewards.
                                Find(r => r.referralRewardSku == reward.referralRewardSku) != null;

            // Find the state of the reward (claimed, ready to claim, etc)
            OfferPackReferralReward.State state;
            if (readyToClaim)
            {
                state = OfferPackReferralReward.State.READY_TO_CLAIM;
            }
            else
            {
                if (friendsCount >= reward.friendsRequired)
                {
                    state = OfferPackReferralReward.State.CLAIMED;
                }
                else
                {
                    state = OfferPackReferralReward.State.NOT_AVAILABLE;
                }
            }

            // Initialize reward preview
            rewardPreview.GetComponent<OfferItemSlotReferral>().InitFromItem(reward, state);

            // Calculate the reward preview horizontal position, so it matches the friend icon in the progression bar
            float unitWidth = (1f / maxFriends);
            float anchorX = unitWidth * (reward.friendsRequired -1) + unitWidth * .5f; // Add .5f to center the reward

            RectTransform rect = rewardPreview.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(anchorX, rect.anchorMin.y);
            rect.anchorMax = new Vector2(anchorX, rect.anchorMax.y);
        }

        // Buttons
        bool canClaim = UsersManager.currentUser.unlockedReferralRewards.Count > 0;

        m_buttonClaim.SetActive(canClaim);
        m_buttonInvite.SetActive(!canClaim && !inviteAlreadyPressed);
        m_buttonInviteMore.SetActive(!canClaim && inviteAlreadyPressed);

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
        while (ReferralManager.instance.pendingRewards.Count > 0)
        {
            OfferPackReferralReward next = ReferralManager.instance.pendingRewards.Dequeue();
            UsersManager.currentUser.PushReward(next.reward);
        }

        // Save current profile state in case the open egg flow is interrupted
        PersistenceFacade.instance.Save_Request(true);

        // Close all open popups (including this one)
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
        // Send tracking event
        HDTrackingManager.Instance.Notify_ReferralPopup(HDTrackingManager.EReferralPopupName.InfoPopup,
                                                        HDTrackingManager.EReferralAction.Invite);

        ReferralManager.instance.InviteFriends(HDTrackingManager.EReferralOrigin.Shop);

        Clear();
        
        inviteAlreadyPressed = true;

        // Refresh the popup with the "INVITE MORE" button
        Refresh();
    }


    /// <summary>
    /// The user has pressed the INVITE MORE button
    /// </summary>
    public void OnInviteMoreButtonPressed()
    {
        // Send tracking event
        HDTrackingManager.Instance.Notify_ReferralPopup(HDTrackingManager.EReferralPopupName.InfoPopup,
                                                        HDTrackingManager.EReferralAction.InviteMore);

        ReferralManager.instance.InviteFriends(HDTrackingManager.EReferralOrigin.Shop);

        Clear();

        inviteAlreadyPressed = true;

        // Refresh the popup with the "INVITE MORE" button
        Refresh();
    }


    /// <summary>
    /// The user has pressed the CLAIM reward button
    /// </summary>
	public void OnClaimButtonPressed()
    {
        // Send tracking event
        HDTrackingManager.Instance.Notify_ReferralPopup(HDTrackingManager.EReferralPopupName.InfoPopup,
                                                        HDTrackingManager.EReferralAction.Claim);

        ReferralManager.instance.ReclaimAllFromServer();

        // Prevent spamming by showing a ui locker
        if(m_loadingPopup == null) {
            m_loadingPopup = PopupManager.LoadPopup(PopupLoading.PATH_LITE);
		}
        m_loadingPopup.Open();
    }


    /// <summary>
    /// The user has pressed the close popup button
    /// </summary>
    public void OnCloseButtonPressed()
    {
        // Send tracking event
        HDTrackingManager.Instance.Notify_ReferralPopup(HDTrackingManager.EReferralPopupName.InfoPopup,
                                                        HDTrackingManager.EReferralAction.Close);

    }

    /// <summary>
    /// The response from the server has arrived.
    /// </summary>
    /// <param name="_error">The error, if any.</param>
    private void OnClaimResponseReceived(FGOL.Server.Error _error) {
        // Hide UI blocker
        if(m_loadingPopup != null && !m_loadingPopup.isOpen) {
            m_loadingPopup.Close(false);    // Don't destroy in case we need to retry
		}

        // If there was an error, show some feedback
        if(_error != null) {
            UIFeedbackText txt = UIFeedbackText.CreateAndLaunch(
                LocalizationManager.SharedInstance.Localize("TID_GEN_ERROR") + "\n" + _error.ToString(),
                new Vector2(0.5f, 0.5f),
                this.GetComponentInParent<Canvas>().transform as RectTransform
            );
            txt.text.color = UIConstants.ERROR_MESSAGE_COLOR;
            txt.duration = 3f;  // Text might be quite long, make it last a bit longer
        }
    }
}