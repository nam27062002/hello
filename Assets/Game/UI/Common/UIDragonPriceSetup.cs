// UIPriceSetup.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 30/11/2018.
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar class to easily setup a price button for a dragon.
/// </summary>
[Serializable]
public class UIDragonPriceSetup {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[Comment("All elements optional")]
	public Localizer actionText = null;
	public TextMeshProUGUI priceText = null;
	public TextMeshProUGUI previousPriceText = null;
	[Space]
	[SerializeField] private Animator m_baseAnimator = null;
	[Tooltip("If defined, it will replace the base animator when a discount is active.")]
	[SerializeField] private AnimatorOverrideController m_animatorControllerForDiscount = null;
	[SerializeField] private GameObject[] m_toActivateOnDiscount = new GameObject[0];

	// Internal
	private RuntimeAnimatorController m_animatorControllerBackup = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialize with the given dragon data.
	/// </summary>
	/// <param name="_data">Dragon data.</param>
	/// <param name="_currency">Currency. Only SC and HC supported for now.</param>
	public void InitFromData(IDragonData _data, UserProfile.Currency _currency) {
		// Aux vars
		bool discountActive = _data.HasPriceModifier(_currency);

		// Final price
		if(priceText != null) {
			priceText.text = UIConstants.GetIconString(
				_data.GetPriceModified(_currency), 
				_currency, 
				UIConstants.IconAlignment.LEFT
			);
		}

		// Previous price - only show if there is a discount
		if(previousPriceText != null) {
			previousPriceText.text = StringUtils.FormatNumber(_data.GetPrice(_currency));
			previousPriceText.gameObject.SetActive(discountActive);
		}

		// Action text
		if(actionText != null) {
			if(discountActive) {
				actionText.Localize("TID_DRAGON_GET_NOW_DISCOUNT");
			} else {
				actionText.Localize("TID_DRAGON_GET_NOW");
			}
		}

		// Discount decos
		for(int i = 0; i < m_toActivateOnDiscount.Length; ++i) {
			if(m_toActivateOnDiscount[i] != null) {
				m_toActivateOnDiscount[i].SetActive(discountActive);
			}
		}

		// Animators
		if(m_baseAnimator != null && m_animatorControllerForDiscount != null) {
			// Backup original animator if not done already
			if(m_animatorControllerBackup == null) {
				m_animatorControllerBackup = m_baseAnimator.runtimeAnimatorController;
			}

			// Set the proper runtime animator controller based on whether the dragon is discounted or not
			if(discountActive) {
				m_baseAnimator.runtimeAnimatorController = m_animatorControllerForDiscount;
			} else {
				m_baseAnimator.runtimeAnimatorController = m_animatorControllerBackup;
			}
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}