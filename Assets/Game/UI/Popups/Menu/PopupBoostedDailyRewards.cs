// PopupBoostedDailyRewards.cs
// 
// Created by Alger Ortín Castellví on 12/02/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Daily rewards popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupBoostedDailyRewards : PopupDailyRewards {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Menu/PF_PopupBoostedDailyRewards";
    


	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the popup with current Boosted Daily Rewards Sequence.
	/// </summary>
	/// <param name="_dismissButtonAllowed">Allow dismiss button?</param>
	protected override void InitWithCurrentData(bool _dismissButtonAllowed) {
        
        // First check if the boosted daily rewards are active
        if (!WelcomeBackManager.instance.IsBoostedDailyRewardActive())
            return;
    
        // Update TIDs in buttons with reward multiplier
        float multiplier = WelcomeBackManager.instance.boostedDailyRewardAdMultiplier;
        string textMultiplier = LocalizationManager.SharedInstance.Localize("TID_DAILY_LOGIN_CLAIM_MULTIPLIER", 
                                                                multiplier.ToString("0"));
        m_doubleAdButton.GetComponentInChildren<Localizer>().Set(textMultiplier);
        m_doubleButton.GetComponentInChildren<Localizer>().Set(textMultiplier);
        
		// Aux vars
        m_sequence = WelcomeBackManager.instance.boostedDailyRewards;
		DailyReward currentReward = m_sequence.GetNextReward();
		bool canCollect = m_sequence.CanCollectNextReward();

		// Initialize rewards
		for(int i = 0; i < m_rewardSlots.Length; ++i) {
			// Skip if slot is not valid
			if(m_rewardSlots[i] == null) continue;

			// Hide slot if reward is not valid
			if(i >= m_sequence.rewards.Length || m_sequence.rewards[i] == null) {
				m_rewardSlots[i].gameObject.SetActive(false);
				continue;
			} else {
				m_rewardSlots[i].gameObject.SetActive(true);
			}

			// Figure out reward state
			DailyReward reward = m_sequence.rewards[i];
			DailyRewardView.State state = DailyRewardView.State.IDLE;
			if(reward.collected) {
				// Reward already collected
				state = DailyRewardView.State.COLLECTED;
			} else if(reward == currentReward) {
				// Current reward! Can it be collected?
				if(canCollect) {
					state = DailyRewardView.State.CURRENT;
				} else {
					state = DailyRewardView.State.COOLDOWN;
				}
			}

			// Initialize reward view!
			m_rewardSlots[i].InitFromData(reward, i, state);
		}

        // Does the player has the Remove ads feature?
        bool removeAds = UsersManager.currentUser.removeAds.IsActive;

		// Is this the seventh day?
		bool finalRewardDay = ( m_sequence.rewardIdx == DailyRewardsSequence.SEQUENCE_SIZE - 1 );

		// Initialize buttons
		m_collectButton.SetActive(canCollect && ( !removeAds || finalRewardDay ) );
		m_doubleAdButton.SetActive(canCollect && currentReward.canBeDoubled && !removeAds && !finalRewardDay);
		m_dismissButton.SetActive(!canCollect && _dismissButtonAllowed);
        m_doubleButton.SetActive(canCollect && currentReward.canBeDoubled && removeAds && !finalRewardDay);
    }
    
    
    //------------------------------------------------------------------------//
    // CALLBACK            													  //
    //------------------------------------------------------------------------//
    
    /// <summary>
    /// A rewarded ad has finished.
    /// </summary>
    /// <param name="_success">Has the ad been successfully played?</param>
    public override void OnAdRewardCallback(bool _success) {
        if(_success) {
            // Success!

            float multiplier = WelcomeBackManager.instance.boostedDailyRewardAdMultiplier;
            
            // Launch the rewards flow, multiplied reward
            CollectNextReward(multiplier);
        }
    }

    /// <summary>
    /// The player collects the multiplied reward by using the Remove Ads feature
    /// </summary>
    public void OnMultiplierButton ()
    {
        float multiplier = WelcomeBackManager.instance.boostedDailyRewardAdMultiplier;
        
        // Launch the rewards flow, multiplied reward
        CollectNextReward(multiplier);
    }

}