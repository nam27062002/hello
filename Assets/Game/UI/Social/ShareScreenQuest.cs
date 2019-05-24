// ShareScreenQuest.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 16/04/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

#define RARITY_GRADIENT

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Individual layout for a pet share screen.
/// </summary>
public class ShareScreenQuest : IShareScreen {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Space]
	[SerializeField] private GlobalEventsPanelActive m_questDataPanel = null;
	[SerializeField] private GameObject m_contributionGroup = null;
	[SerializeField] private TextMeshProUGUI m_contributionText = null;
	[SerializeField] private Localizer m_remainingTimeText = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this screen with current quest data (from HDQuestManager).
	/// </summary>
	/// <param name="_shareLocationSku">Location where this screen is triggered.</param>
	/// <param name="_refCamera">Reference camera. Its properties will be copied to the scene's camera.</param>
	/// <param name="_showContribution">Whether to show or hide the contribution field.</param>
	public void Init(string _shareLocationSku, Camera _refCamera, bool _showContribution) {
		// Set location and camera
		SetLocation(_shareLocationSku);
		SetRefCamera(_refCamera);

		// Aux vars
		HDQuestManager quest = HDLiveDataManager.quest;

		// Initialize UI elements
		// Quest data
		if(m_questDataPanel != null) {
			m_questDataPanel.Refresh();
		}

		// Show contribution group?
		if(m_contributionGroup != null) {
			// Show?
			long runScore = quest.GetRunScore();
			bool show = _showContribution && quest.EventExists() && runScore > 0;
			m_contributionGroup.SetActive(show);

			// Contribution text
			if(show && m_contributionText != null) {
				// Formatted as the quest type requires
				m_contributionText.text = quest.FormatScore(runScore);
			}

			// Refresh progression counting the run score
			if(show && m_questDataPanel != null) {
				m_questDataPanel.MoveScoreTo(quest.m_questData.m_globalScore + runScore, 0f);	// Instant, no animation
			}
		}

		// Remaining text
		if(m_remainingTimeText != null) {
			double remainingSeconds = quest.m_questData.remainingTime.TotalSeconds;
			m_remainingTimeText.gameObject.SetActive(remainingSeconds > 0);
			if(remainingSeconds > 0) {
				m_remainingTimeText.Localize(
					m_remainingTimeText.tid,
					TimeUtils.FormatTime(
						System.Math.Max(0, remainingSeconds),   // Don't go below 0!
						TimeUtils.EFormat.WORDS_WITHOUT_0_VALUES, 1
					)
				);
			}
		}
	}

    
}