//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
public class MenuPlayScreenScene : MenuScreenScene {
	//------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES											//
	//------------------------------------------------------------------//
	[SerializeField] private MenuDragonLoader m_dragonPreview;



	//------------------------------------------------------------------//
	// GENERIC METHODS													//
	//------------------------------------------------------------------//
	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnScreenTransitionStart);
		Messenger.AddListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnScreenTransitionEnd);
	}

	/// <summary>
	/// 
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		// Unsubscribe from external events.
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_START, OnScreenTransitionStart);
		Messenger.RemoveListener<MenuScreen, MenuScreen>(MessengerEvents.MENU_SCREEN_TRANSITION_END, OnScreenTransitionEnd);
	}



	//------------------------------------------------------------------//
	// CALLBACKS														//
	//------------------------------------------------------------------//
	private void OnScreenTransitionStart(MenuScreen _from, MenuScreen _to) {		
		if (_to == MenuScreen.PLAY) {
			m_dragonPreview.RefreshDragon();
		}
	}

	private void OnScreenTransitionEnd(MenuScreen _from, MenuScreen _to) {		
		if (_from == MenuScreen.PLAY) {
			m_dragonPreview.UnloadDragon();
		}
	}
}