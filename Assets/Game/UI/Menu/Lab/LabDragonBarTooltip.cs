// LabDragonBarTooltip.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 19/10/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Specialization of the generic UI Tooltip for the Lab Dragon Bar.
/// </summary>
public class LabDragonBarTooltip : UITooltipMultidirectional {
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed references
	[Separator("LabDragonBarTooltip")]
	[SerializeField] private TextMeshProUGUI m_unlockLevelText = null;
	
	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize the level at which the element is obtained
	/// Call it right after the Init().
	/// </summary>
	/// <param name="_level">Level at which the element is obtained.</param>
	/// <param name="_levelReached">Has the level been reached?</param>
	public void SetUnlockLevel(int _level, bool _levelReached) {
		// Check textfield
		if(m_unlockLevelText == null) return;

		// Always show for now
		m_unlockLevelText.gameObject.SetActive(true);

		// Set text
		m_unlockLevelText.text = LocalizationManager.SharedInstance.Localize(
			"TID_LAB_TOOLTIP_UNLOCK_LEVEL",
			StringUtils.FormatNumber(_level)
		);
	}


	/// <summary>
    /// Initialize the tooltip with the given texts and icon.
    /// If the tooltip has no textfields or icon assigned, will be ignored.
    /// If a text or icon is left empty, its corresponding game object will be disabled.
    /// </summary>
    /// <param name="_title">Title string.</param>
    /// <param name="_text">Text string.</param>
    /// <param name="_icon">Icon sprite.</param>
	public override void Init(string _title, string _text, Sprite _icon) {
		// Base initialization
		base.Init(_title, _text, _icon);

		// Hide extra textfields until they're initialized
		if(m_unlockLevelText != null) {
			m_unlockLevelText.gameObject.SetActive(false);
		}

	}
}