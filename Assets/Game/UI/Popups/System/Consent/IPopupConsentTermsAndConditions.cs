// IPopupConsentTermsAndConditions.cs
// Hungry Dragon
//
// Copyright (c) 2018 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Auxiliar static class to centralize the usage of the Terms Of Use (TOU) / End 
/// User License Agreement (EULA) / Privacy Policy (PP) buttons.
/// </summary>
public class IPopupConsentTermsAndConditions {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Link to the Privacy Policy page.
	/// </summary>
	public static void OpenPrivacyPolicy() {
#if UNITY_IOS
	    GameSettings.OpenUrl(GameSettings.PRIVACY_POLICY_IOS_URL);
#else
        GameSettings.OpenUrl(GameSettings.PRIVACY_POLICY_ANDROID_URL);
#endif

    }

    /// <summary>
    /// Link to the End User License Agreement page.
    /// </summary>
    public static void OpenEULA() {
		GameSettings.OpenUrl(GameSettings.EULA_URL);
	}

	/// <summary>
	/// Open Terms of Use.
	/// </summary>
	public static void OpenTOU() {
		GameSettings.OpenUrl(GameSettings.TERMS_OF_USE_URL);
	}
}
