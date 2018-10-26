using UnityEngine;
using UnityEngine.Events;

public class QuitPlayBackButtonHandler : BackButtonHandler, IBroadcastListener {

	bool m_changingArea = false;
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	private void OnEnable() {
		Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
		Messenger.AddListener(MessengerEvents.PLAYER_ENTERING_AREA, OnAreaEnter);
		Messenger.AddListener<float>(MessengerEvents.PLAYER_LEAVING_AREA, OnAreaLeave);
		Messenger.AddListener(MessengerEvents.GAME_ENDED, Unregister);
	}

	private void OnDisable() {
		Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
		Messenger.RemoveListener(MessengerEvents.PLAYER_ENTERING_AREA, OnAreaEnter);
		Messenger.RemoveListener<float>(MessengerEvents.PLAYER_LEAVING_AREA, OnAreaLeave);
		Messenger.RemoveListener(MessengerEvents.GAME_ENDED, Unregister);
	}
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.GAME_LEVEL_LOADED:
            {
                Register();
            }break;
        }
    }

	public void OnAreaEnter()
	{
		m_changingArea = false;
	}
	public void OnAreaLeave(float estimatedLeavingTime)
	{
		m_changingArea = true;
	}

	public override void Trigger() {
		if (!m_changingArea)
		{
			PopupManager.OpenPopupInstant(PopupExitRunConfirmation.PATH);
		}
		/*
		if (GameSettings.Get(GameSettings.SHOW_EXIT_RUN_CONFIRMATION_POPUP)) {
			PopupManager.OpenPopupInstant(PopupExitRunConfirmation.PATH);
		} else {
			if (InstanceManager.gameSceneController != null) {
				InstanceManager.gameSceneController.EndGame(true);
			}
		}*/
	}
}
