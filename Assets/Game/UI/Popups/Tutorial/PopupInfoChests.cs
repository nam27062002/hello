// PopupInfoChests.cs
// 
// Created by Alger Ortín Castellví on 25/04/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Eggs info popup.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupInfoChests : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/Tutorial/PF_PopupInfoChests";

	[System.Serializable]
	public class RewardInfo {
		public TextMeshProUGUI rewardText = null;
		public GameObject collectedMark = null;
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private Localizer m_collectedCountText = null;
	[SerializeField] private RewardInfo[] m_rewardInfos = new RewardInfo[5];

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The popup is about to open.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Initialize reward texts
		for(int i = 0; i < m_rewardInfos.Length; ++i) {
			// Get reward for this chest
			Chest.RewardData rewardData = ChestManager.GetRewardData(i + 1);

			// Initialize text
			if(m_rewardInfos[i].rewardText != null) {
				// Different icon based on reward type
				if(rewardData == null) {
					m_rewardInfos[i].rewardText.text = string.Empty;
				} else {
					if(rewardData.type == Chest.RewardType.PC) {
						m_rewardInfos[i].rewardText.text = UIConstants.GetIconString(rewardData.amount, UIConstants.IconType.PC, UIConstants.IconAlignment.LEFT);
					} else {
						m_rewardInfos[i].rewardText.text = UIConstants.GetIconString(rewardData.amount, UIConstants.IconType.COINS, UIConstants.IconAlignment.LEFT);
					}
				}
			}

			// Mark as collected?
			if(m_rewardInfos[i].collectedMark != null) {
				m_rewardInfos[i].collectedMark.SetActive(i < ChestManager.collectedChests);
			}
		}

		// Initialize collected counter
		if(m_collectedCountText != null) {
			m_collectedCountText.Localize(
				"TID_FRACTION", 
				StringUtils.FormatNumber(ChestManager.collectedChests), 
				StringUtils.FormatNumber(ChestManager.NUM_DAILY_CHESTS)
			);
		}
	}
}
