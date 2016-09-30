// DisguisePowerIcon.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 03/05/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple controller for a disguise power icon.
/// </summary>
public class DisguisePowerIcon : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed References
	[SerializeField] private Image m_powerIcon = null;
	[SerializeField] private GameObject m_lockIcon = null;
	[SerializeField] private Text m_shortDescriptionText = null;

	// Exposed Setup
	[Space]
	[SerializeField][Range(0, 1)] private float m_tooltipArrowOffset = 0.5f;

	// Data
	private DefinitionNode m_powerDef = null;
	public DefinitionNode powerDef {
		get { return m_powerDef; }
	}

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Check required fields
		Debug.Assert(m_powerIcon != null, "Required field!");
		Debug.Assert(m_lockIcon != null, "Required field!");
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize this button with the data from the given definition.
	/// </summary>
	/// <param name="_powerDef">Power definition.</param>
	/// <param name="_locked">Whether the power is locked or not.</param>
	public void InitFromDefinition(DefinitionNode _powerDef, bool _locked) {
		// Ignore if given definition is not valid
		if(_powerDef == null) return;

		// Save definition
		m_powerDef = _powerDef;

		// Power icon
		if(m_powerIcon != null) {
			// Load power icons spritesheet
			Sprite[] allIcons = Resources.LoadAll<Sprite>("UI/Popups/Disguises/powers/icons_powers");

			// Pick target icon
			string iconName = _powerDef.GetAsString("icon");
			m_powerIcon.sprite = Array.Find<Sprite>(allIcons, (_sprite) => { return _sprite.name == iconName; });
		}

		// Short description
		if(m_shortDescriptionText != null) {
			m_shortDescriptionText.text = DragonPowerUp.GetDescription(_powerDef, true);	// Custom formatting depending on powerup type, already localized
		}

		// Lock
		SetLocked(_locked);
	}

	/// <summary>
	/// Sets the lock state of the power.
	/// </summary>
	/// <param name="_locked">Whether the power is locked or not.</param>
	public void SetLocked(bool _locked) {
		// Lock icon
		m_lockIcon.SetActive(_locked);

		// Image color
		m_powerIcon.color = _locked ? Color.gray : Color.white;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A tooltip is about to be opened.
	/// If the trigger is attached to this power icon, initialize tooltip with this
	/// button's power def.
	/// Link it via the inspector.
	/// </summary>
	/// <param name="_tooltip">The tooltip about to be opened.</param>
	/// <param name="_trigger">The button which triggered the event.</param>
	public void OnTooltipOpen(UITooltip _tooltip, UITooltipTrigger _trigger) {
		// Make sure the trigger that opened the tooltip is linked to this icon
		if(_trigger != this.GetComponent<UITooltipTrigger>()) return;

		// Tooltip will take care of the rest
		DisguisePowerTooltip powerTooltip = _tooltip.GetComponent<DisguisePowerTooltip>();
		if(powerTooltip != null) {
			// Initialize
			powerTooltip.InitFromDefinition(m_powerDef);

			// Set lock state
			powerTooltip.SetLocked(m_lockIcon.activeSelf);	// Use lock icon visibility to determine whether power is locked or not

			// [AOC] With the new layout, set tooltip's position to spawn from the same icon's Y.
			//powerTooltip.transform.SetPosY(this.transform.position.y);
		}

		// Set arrow offset to make it point to this icon
		_tooltip.SetArrowOffset(m_tooltipArrowOffset);
	}
}