// LeaguesRewardScreen.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/01/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Controller for the leagues reward screen.
/// </summary>
public class LeaguesRewardScreen : IRewardScreen {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private enum CustomStep {
		SEASON_RESULT = Step.FINISH + 1,		// Since we cannot do Enum inheritance, make sure it has a different value than the default steps

		NUM_STEPS
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Abstract properties implementation
	protected override MenuScreen screenID {
		get { return MenuScreen.LEAGUES_REWARD; }
	}

	protected override int numSteps {
		get { return (int)CustomStep.NUM_STEPS; }
	}

	// Step screens
	[Space]
	[SerializeField] private ShowHideAnimator m_introScreen = null;
	[SerializeField] private ShowHideAnimator m_resultScreen = null;
	[SerializeField] private ShowHideAnimator m_rewardScreen = null;

	// Individual elements references
	[Space]
	[SerializeField] private Localizer m_leagueNameText = null;
	[SerializeField] private TextMeshProUGUI m_rankText = null;
	[SerializeField] private Image m_leagueIcon = null;
	[Space]
	[SerializeField] private Animator m_introScreenAnim = null;
	[SerializeField] private Animator m_resultScreenAnim = null;

	// Internal references
	private HDSeasonData m_season = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// ABSTRACT METHODS IMPLEMENTATION										  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Get the screen associated to a specific step.
	/// </summary>
	/// <returns>The screen linked to the given step.</returns>
	/// <param name="_step">Step whose screen we want.</param>
	protected override ShowHideAnimator GetStepScreen(int _step) {
		switch(_step) {
			case (int)Step.INTRO: return m_introScreen;
			case (int)CustomStep.SEASON_RESULT: return m_resultScreen;
			case (int)Step.REWARD: return m_rewardScreen;
		}
		return null;
	}

	/// <summary>
	/// The flow will start as soon as possible.
	/// Used to initialize and prepare everything.
	/// </summary>
	protected override void OnStartFlow() {
		// Store current season data for faster access
		m_season = HDLiveDataManager.league.season;

		// Initialize visual info
		RefreshLeagueInfo(m_season.currentLeague);

		if(m_rankText != null) {

            if (LocalizationManager.SharedInstance.GetCurrentLanguageSKU() == "lang_chinese" ||
                LocalizationManager.SharedInstance.GetCurrentLanguageSKU() == "lang_chinese_trad")
            {
                // In chinese the ordinal symbol is at the same height than the number [HDK-4654]
                m_rankText.text = UIUtils.FormatOrdinalNumber(
                m_season.currentLeague.leaderboard.playerRank + 1,
                UIUtils.OrdinalSuffixFormat.DEFAULT
                );
            } 
            else
            {
                m_rankText.text = UIUtils.FormatOrdinalNumber(
                m_season.currentLeague.leaderboard.playerRank + 1,
                UIUtils.OrdinalSuffixFormat.SUPERSCRIPT
                );

            }
		}

		// Select promotion/demotion objets
		HDSeasonData.Result result = m_season.result;
	}

	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Called when advancing step to select the step to go to.
	/// Override for custom steps.
	/// </summary>
	/// <returns>The next step.</returns>
	protected override int SelectNextStep() {
		// Override some parent transitions to include extra steps
		switch(m_step) {
			case (int)Step.INTRO: {
				return (int)CustomStep.SEASON_RESULT;
			} 

			case (int)CustomStep.SEASON_RESULT: {
				return (int)Step.REWARD;
			} 
		}

		// For the rest of steps, let parent decide
		return base.SelectNextStep();
	}

	/// <summary>
	/// Called when launching a new step. Check <c>m_step</c> to know which state is being launched.
	/// Override for custom steps.
	/// </summary>
	/// <param name="_prevStep">Previous step.</param>
	/// <param name="_newStep">The step we're launching.</param>
	protected override void OnLaunchNewStep(int _prevStep, int _newStep) {
		// Let parent do its stuff
		base.OnLaunchNewStep(_prevStep, _newStep);

		// Perform extra stuff depending on new step
		switch(_newStep) {
			case (int)Step.INTRO: {
				// Trigger UI animation
				m_introScreenAnim.SetTrigger("in");
			} break;

			case (int)CustomStep.SEASON_RESULT: {
				// Trigger UI animation
				m_resultScreenAnim.SetInteger("result", (int)m_season.result);   // Animator uses the same indexes as the enum to select its state
				m_resultScreenAnim.SetTrigger("in");
			} break;

			case (int)Step.FINISH: {
				// Save!
				PersistenceFacade.instance.Save_Request();

				// Go back to leagues screen
				InstanceManager.menuSceneController.GoToScreen(MenuScreen.LEAGUES);
			} break;
		}
	}

	//------------------------------------------------------------------------//
	// INTERNAL METHODS													 	  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Toggle a collection of Game Objects on or off.
	/// </summary>
	/// <param name="_objs">Objects to be toggled.</param>
	/// <param name="_toggle">Whether to activate or deactivate the objects.</param>
	private void ToggleObjects(ref GameObject[] _objs, bool _toggle) {
		// Just iterate over the objects and set their active state
		for(int i = 0; i < _objs.Length; ++i) {
			_objs[i].SetActive(_toggle);
		}
	}

	/// <summary>
	/// Refresh league icon and name with the given league data.
	/// </summary>
	/// <param name="_league">Data to be used.</param>
	private void RefreshLeagueInfo(HDLeagueData _league) {
		if(m_leagueIcon != null) {
			m_leagueIcon.sprite = Resources.Load<Sprite>(UIConstants.LEAGUE_ICONS_PATH + _league.icon);
		}

		if(m_leagueNameText != null) {
			m_leagueNameText.Localize(_league.tidName);
		}
	}

    //------------------------------------------------------------------------//
    // CALLBACKS															  //
    //------------------------------------------------------------------------//
    /// <summary>
    /// THe collect rewards button has been pressed.
    /// </summary>
	public void OnCollectRewardsButton() {
        // Ignore if we're still animating some step (prevent spamming)
        if(m_state == State.ANIMATING) return;

        // Go to leagues Reward Screen
        UsersManager.currentUser.PushReward(m_season.reward);
        m_season.RequestFinalize();

        // Immediately save persistence in case the rewards opening gets interrupted
        HDLiveDataManager.instance.SaveEventsToCache();
        PersistenceFacade.instance.Save_Request(true);

		// Trigger UI animation
		m_resultScreenAnim.SetTrigger("out");

		// Prevent button spamming
		SetAnimatingState(State.ANIMATING);

        // Next step!
		// [AOC] Animation will do it (for a better sync)
       // AdvanceStep();
    }

	/// <summary>
	/// Animation tells us to switch league info.
	/// </summary>
	public void OnSwitchLeagueInfo() {
		RefreshLeagueInfo(m_season.nextLeague);
	}
}
