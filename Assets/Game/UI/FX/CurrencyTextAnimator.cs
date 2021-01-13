// CurrencyTextAnimator.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/09/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Specialization of the number text animator for currencies.
/// </summary>
public class CurrencyTextAnimator : NumberTextAnimator {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private UserProfile.Currency m_currency = UserProfile.Currency.NONE;
	[SerializeField] private UIConstants.IconAlignment m_alignment = UIConstants.IconAlignment.RIGHT;	// Typical HUD top-right counter
	
	//------------------------------------------------------------------------//
	// PARENT OVERRIDES														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Format a given value into the desired format.
	/// </summary>
	/// <returns>The formatted string.</returns>
	/// <param name="_value">The value to be formatted.</param>
	override protected string FormatValue(long _value) {
		// Attach currency icon as defined in the setup
		// UIConstants does the job for us!
		return UIConstants.GetIconString(_value, m_currency, m_alignment);
	}
}