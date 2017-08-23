using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(NavigationScreen))]
public class NavigationBackButtonHandler : BackButtonHandler {
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Target Screen
	[SerializeField] protected MenuScreens m_targetScreen = MenuScreens.NONE;

	//
	private MenuScreensController m_navigationSystem = null;


	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get a reference to the navigation system, which in this particular case should be a component in the menu scene controller
		m_navigationSystem = InstanceManager.sceneController.GetComponent<MenuScreensController>();
	}

	private void OnEnable() {
		Register();
	}

	private void OnDisable() {
		Unregister();
	}

	public override void Trigger() {
		m_navigationSystem.GoToScreen((int)m_targetScreen);
	}
}
