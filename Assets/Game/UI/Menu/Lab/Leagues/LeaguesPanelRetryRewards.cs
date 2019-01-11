// LeaguesPanelRetryRewards.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/01/2019.
// Copyright (c) 2019 Ubisoft. All rights reserved.

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
/// Panel corresponding to leagues requiring a retry on the rewards.
/// </summary>
public class LeaguesPanelRetryRewards : LeaguesScreenPanel {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private TextMeshProUGUI m_errorText = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the panel with a specific error code.
	/// </summary>
	/// <param name="_errorCode">Error code.</param>
	public void SetError(HDLiveDataManager.ComunicationErrorCodes _errorCode) {
		switch(_errorCode) {
			case HDLiveDataManager.ComunicationErrorCodes.NET_ERROR: {
				m_errorText.text = LocalizationManager.SharedInstance.Localize("TID_NET_ERROR");
			} break;

			case HDLiveDataManager.ComunicationErrorCodes.NO_RESPONSE: {
				m_errorText.text = LocalizationManager.SharedInstance.Localize("TID_NO_RESPONSE");
			} break;

			default: {
				m_errorText.text = LocalizationManager.SharedInstance.Localize("TID_EVENT_RESULTS_UNKNOWN_ERROR");
			} break;
		}
	}
}