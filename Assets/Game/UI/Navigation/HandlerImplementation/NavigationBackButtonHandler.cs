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
	protected MenuScreensController m_navigationSystem = null;
	private NavigationScreen m_screen = null;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get a reference to the navigation system, which in this particular case should be a component in the menu scene controller
		m_navigationSystem = InstanceManager.sceneController.GetComponent<MenuScreensController>();

		m_screen = GetComponent<NavigationScreen>();
		m_screen.OnShow.AddListener(Register);
		m_screen.OnHide.AddListener(Unregister);
	}

	public override void Trigger() {
		m_navigationSystem.GoToScreen((int)m_targetScreen);
	}
}
