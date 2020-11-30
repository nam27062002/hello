
// Hungry Dragon
// 
// Created by Jm Olea
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Step for the results screen.
/// </summary>
public class ResultsScreenStepSoloQuest : ResultsScreenStepQuest
{
	//------------------------------------------------------------------------//
	// Override parent                  									  //
	//------------------------------------------------------------------------//

	/// <summary>
	/// Check whether this step must be displayed or not based on the run results.
	/// </summary>
	/// <returns><c>true</c> if the step must be displayed, <c>false</c> otherwise.</returns>
	override public bool MustBeDisplayed()
	{
		// Never during FTUX
		// By this point the gamesPlayed var has already been increased, so we must actually count one less game
		if (UsersManager.currentUser.gamesPlayed - 1 < GameSettings.ENABLE_QUESTS_AT_RUN)
		{
			return false;
		}

		BaseQuestManager questManager = HDLiveDataManager.quest;

		if (
				HDLiveDataManager.instance.SoloQuestIsAvailable() &&
                questManager.EventExists() &&
				questManager.IsRunning() &&
				questManager.isActive &&
				questManager.GetQuestData().remainingTime.TotalSeconds > 0
		)
		{
			if (questManager.GetRunScore() > 0)
				return true;
		}
		return false;
	}


	/// <summary>
	/// Trigger the active panel animation.
	/// </summary>
	protected override void LaunchActivePanelAnimation()
	{
		// Kill any existing tween
		string tweenId = "ResultsScreenStepSoloQuest.ActivePanel";
		DOTween.Kill(tweenId);

		// Init some stuff
		m_continueEnabled = false;
		m_tapToContinueAnim.Hide();

		// Sequentially update values
		float scoreAnimDuration = 1.5f;
		m_activePanelSequence = DOTween.Sequence()
			.SetId(tweenId)
			.AppendInterval(1f)

			// Trigger Score Anim
			.AppendCallback(() => {
				// Bar anim
				if (m_eventPanelActive != null)
				{
					m_eventPanelActive.MoveScoreTo(
						m_questManager.GetQuestData().m_globalScore,
						m_questManager.GetQuestData().m_globalScore + m_questManager.GetRunScore(),
						scoreAnimDuration
					);
				}

				// FX anim
				if (m_scoreTransferFXFrom != null && m_scoreTransferFXTo != null)
				{
					ParticlesTrailFX scoreTransferFX = ParticlesTrailFX.LoadAndLaunch(
						"UI/FX/PF_ScoreTransferFX",
						this.GetComponentInParent<Canvas>().transform,
						m_scoreTransferFXFrom.position + new Vector3(0f, 0f, -0.5f),        // Offset Z so the coins don't collide with the UI elements
						m_scoreTransferFXTo.position + new Vector3(0f, 0f, -0.5f)
					);
					scoreTransferFX.totalDuration = scoreAnimDuration;
				}

				// SFX
				if (!string.IsNullOrEmpty(m_transferSFX))
				{
					AudioController.Play(m_transferSFX);
				}
			})
			.AppendInterval(scoreAnimDuration)

			// Tap to continue
			.AppendCallback(() => {
				// Allow continue
				m_continueEnabled = true;
				m_tapToContinueAnim.Show();
				m_activePanelSequence = null;
			});
	}
}