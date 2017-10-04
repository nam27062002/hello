using System;
using UnityEngine;

[RequireComponent(typeof(PopupController))]
public class PopupTermsAndConditions : MonoBehaviour {
	public const string PATH = "UI/Popups/Message/PF_PopupTermsAndConditions";
	public const string KEY = "LegalVersionAgreed";
	public const int LEGAL_VERSION = 1;

    private PopupController m_popupController;
   
    private bool HasBeenAccepted { get; set; }

    private float TimeAtOpen { get; set; }

    void Awake() {
        HasBeenAccepted = false;
        TimeAtOpen = Time.unscaledTime;

        m_popupController = GetComponent<PopupController>();
        m_popupController.OnClosePreAnimation.AddListener(OnClose);
    }

    void OnDestroy() {
        if (m_popupController != null) { 
            m_popupController.OnClosePreAnimation.RemoveListener(OnClose);
        }
    }

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
        HasBeenAccepted = true;
        PlayerPrefs.SetInt(KEY, LEGAL_VERSION);
		GetComponent<PopupController>().Close(true);
	}

    private void OnClose() {
        int duration = Convert.ToInt32(Time.unscaledTime - TimeAtOpen);
        HDTrackingManager.Instance.Notify_LegalPopupClosed(duration, HasBeenAccepted);
    }
}
