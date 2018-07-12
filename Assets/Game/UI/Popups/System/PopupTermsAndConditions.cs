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
        // Show loading until we know country or age

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
		OpenUrlDelayed(privacyPolicyUrl);
	}

	public void OnEulaButton() {
		string eulaUrl = "https://legal.ubi.com/eula/" + LocalizationManager.SharedInstance.Culture.Name;	// Standard iso name: "en-US", "en-GB", "es-ES", "pt-BR", "zh-CN", etc.
		OpenUrlDelayed(eulaUrl);
	}

	public void OnTermsOfUseButton() {
		string touUrl = "https://legal.ubi.com/termsofuse/" + LocalizationManager.SharedInstance.Culture.Name;	// Standard iso name: "en-US", "en-GB", "es-ES", "pt-BR", "zh-CN", etc.
		OpenUrlDelayed(touUrl);
	}

	/// <summary>
	/// Opens the URL after a short delay.
	/// </summary>
	/// <param name="_url">URL to be opened.</param>
	private void OpenUrlDelayed(string _url) {
		// Add some delay to give enough time for SFX to be played before losing focus
		UbiBCN.CoroutineManager.DelayedCall(
			() => {
				Application.OpenURL(_url);
			}, 0.15f
		);
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
