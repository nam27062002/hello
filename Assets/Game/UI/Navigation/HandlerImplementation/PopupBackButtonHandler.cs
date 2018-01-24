using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PopupController))]
public class PopupBackButtonHandler : BackButtonHandler {
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Events
	[SerializeField] private UnityEvent OnBackButton = new UnityEvent();

	//
	private bool m_enabled;
	private PopupController m_popup = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_enabled = false ;

		m_popup = GetComponent<PopupController>();
		m_popup.OnOpenPreAnimation.AddListener(Register);
		m_popup.OnOpenPostAnimation.AddListener(OnOpenPostAnimation);
		m_popup.OnClosePreAnimation.AddListener(OnClosePreAnimation);
		m_popup.OnClosePostAnimation.AddListener(Unregister);
	}

	public override void Trigger() {
		if (enabled && m_enabled)
			OnBackButton.Invoke();
	}

	private void OnOpenPostAnimation() {
		m_enabled = true;
	}

	private void OnClosePreAnimation() {
		m_enabled = false;
	}
}
