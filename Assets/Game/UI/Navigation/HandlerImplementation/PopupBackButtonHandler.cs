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
	PopupController m_popup = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_popup = GetComponent<PopupController>();
		m_popup.OnOpenPreAnimation.AddListener(Register);
		m_popup.OnClosePostAnimation.AddListener(Unregister);
	}

	public override void Trigger() {
		if (enabled)
			OnBackButton.Invoke();
	}
}
