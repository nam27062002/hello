using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PopupController))]
public class PopupTermsAndConditions : MonoBehaviour {
	public const string PATH = "UI/Popups/Message/PF_PopupTermsAndConditions";
	public const string KEY = "LegalVersionAgreed";
	public const int LEGAL_VERSION = 1;

	public void OnPrivacyPolicyButton() {
		string privacyPolicyUrl = "https://legal.ubi.com/privacypolicy/" + LocalizationManager.SharedInstance.Culture.Name;	// Standard iso name: "en-US", "en-GB", "es-ES", "pt-BR", "zh-CN", etc.
		Application.OpenURL(privacyPolicyUrl);
	}

	public void OnEulaButton() {
		string privacyPolicyUrl = "https://legal.ubi.com/eula/" + LocalizationManager.SharedInstance.Culture.Name;	// Standard iso name: "en-US", "en-GB", "es-ES", "pt-BR", "zh-CN", etc.
		Application.OpenURL(privacyPolicyUrl);
	}

	public void OnTermsOfUseButton() {
		string privacyPolicyUrl = "https://legal.ubi.com/termsofuse/" + LocalizationManager.SharedInstance.Culture.Name;	// Standard iso name: "en-US", "en-GB", "es-ES", "pt-BR", "zh-CN", etc.
		Application.OpenURL(privacyPolicyUrl);
	}

	public void OnAccept() {
		PlayerPrefs.SetInt(KEY, LEGAL_VERSION);
		GetComponent<PopupController>().Close(true);
	}
}
