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
		Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);
	}

	private void OnDisable() {
		Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
		Messenger.RemoveListener(MessengerEvents.PLAYER_ENTERING_AREA, OnAreaEnter);
		Messenger.RemoveListener<float>(MessengerEvents.PLAYER_LEAVING_AREA, OnAreaLeave);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_ENDED, this);
	}
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.GAME_LEVEL_LOADED:
            {
                Register();
            }break;
            case BroadcastEventType.GAME_ENDED:
            {
                Unregister();
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
			// [AOC] Based on Google's feedback for 1.22, open the pause popup instead
			//PopupManager.OpenPopupInstant(PopupExitRunConfirmation.PATH);
			if ( InstanceManager.gameHUD.CanPause() )
				InstanceManager.gameHUD.OnPauseButton();
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
