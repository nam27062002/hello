using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(NavigationScreen))]
public class NavigationBackButtonHandler : BackButtonHandler {
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Target Screen
	[SerializeField] protected MenuScreen m_targetScreen = MenuScreen.NONE;

	//
	private MenuTransitionManager m_transitionManager = null;


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		m_transitionManager = InstanceManager.menuSceneController.transitionManager;
	}

	private void OnEnable() {
		Register();
	}

	private void OnDisable() {
		Unregister();
	}

	public override void Trigger() {
		m_transitionManager.GoToScreen(m_targetScreen, true);
	}
}
