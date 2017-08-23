using UnityEngine;
using UnityEngine.Events;

public class GenericBackButtonHandler : BackButtonHandler {
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Events
	[SerializeField] private UnityEvent OnBackButton = new UnityEvent();

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	private void OnEnable() {
		Register();
	}

	private void OnDisable() {
		Unregister();
	}

	private void OnDestroy() {
		Unregister();
	}

	public override void Trigger() {
		OnBackButton.Invoke();
	}
}
