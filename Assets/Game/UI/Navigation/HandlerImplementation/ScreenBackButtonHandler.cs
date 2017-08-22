using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(NavigationScreen))]
public class ScreenBackButtonHandler : BackButtonHandler {
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Events
	[SerializeField] private UnityEvent OnBackButton = new UnityEvent();

	//
	private NavigationScreen m_screen = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_screen = GetComponent<NavigationScreen>();
		m_screen.OnShow.AddListener(Register);
		m_screen.OnHide.AddListener(Unregister);
	}

	public override void Trigger() {
		OnBackButton.Invoke();
	}
}
