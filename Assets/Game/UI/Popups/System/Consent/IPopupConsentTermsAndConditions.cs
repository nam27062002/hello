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
	/// <summary>
	/// Opens the URL after a short delay.
	/// </summary>
	/// <param name="_url">URL to be opened.</param>
	protected static void OpenUrlDelayed(string _url) {
		// Add some delay to give enough time for SFX to be played before losing focus
		UbiBCN.CoroutineManager.DelayedCall(
			() => {
				Application.OpenURL(_url);
			}, 0.15f
		);
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Link to the Privacy Policy page.
	/// </summary>
	public static void OpenPrivacyPolicy() {
		string privacyPolicyUrl = "https://legal.ubi.com/privacypolicy/" + LocalizationManager.SharedInstance.Culture.Name; // Standard iso name: "en-US", "en-GB", "es-ES", "pt-BR", "zh-CN", etc.
		OpenUrlDelayed(privacyPolicyUrl);
	}

	/// <summary>
	/// Link to the End User License Agreement page.
	/// </summary>
	public static void OpenEULA() {
		string eulaUrl = "https://legal.ubi.com/eula/" + LocalizationManager.SharedInstance.Culture.Name;   // Standard iso name: "en-US", "en-GB", "es-ES", "pt-BR", "zh-CN", etc.
		OpenUrlDelayed(eulaUrl);
	}

	/// <summary>
	/// Open Terms of Use.
	/// </summary>
	public static void OpenTOU() {
		string touUrl = "https://legal.ubi.com/termsofuse/" + LocalizationManager.SharedInstance.Culture.Name;  // Standard iso name: "en-US", "en-GB", "es-ES", "pt-BR", "zh-CN", etc.
		OpenUrlDelayed(touUrl);
	}
}
