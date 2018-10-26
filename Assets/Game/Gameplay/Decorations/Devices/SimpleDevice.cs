using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDevice : Initializable, IBroadcastListener {

	private AutoSpawnBehaviour m_autoSpawner;
	private InflammableDecoration m_inflammable;

	protected DeviceOperatorSpawner m_operatorSpawner;
	protected bool m_operatorAvailable;

	protected bool m_enabled;


	void Awake() {
		m_autoSpawner = GetComponent<AutoSpawnBehaviour>();
		m_operatorSpawner = GetComponent<DeviceOperatorSpawner>();
		m_inflammable = GetComponent<InflammableDecoration>();

		m_operatorAvailable = false;
		m_enabled = false;
	}

	public override void Initialize() {
		m_operatorAvailable = false;
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	protected virtual void OnEnable() {
		// Subscribe to external events
		Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
		Messenger.AddListener(MessengerEvents.GAME_AREA_ENTER, OnAreaLoaded);
		Messenger.AddListener(MessengerEvents.GAME_ENDED, OnAreaExit);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	protected virtual void OnDisable() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
		Messenger.RemoveListener(MessengerEvents.GAME_AREA_ENTER, OnAreaLoaded);
		Messenger.RemoveListener(MessengerEvents.GAME_ENDED, OnAreaExit);
	}

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.GAME_LEVEL_LOADED:
            {
                OnAreaLoaded();
            }break;
        }
    }
    
	// Update is called once per frame
	void Update () {
		if (m_enabled) {
			if (m_inflammable.IsBurning() 
			|| 	m_autoSpawner.state == AutoSpawnBehaviour.State.Respawning) {	// if respawning we wait
				OnRespawning();
				m_operatorAvailable = false;

				return;
			}

			if (m_operatorAvailable) {
				if (m_operatorSpawner.IsOperatorDead()) {
					m_operatorAvailable = false;
					OnOperatorDead();
				
					return;
				}
			} else {
				// check respawn conditions
				if (m_operatorSpawner.IsRespawing() && m_operatorSpawner.CanRespawn()) {
					m_operatorSpawner.Respawn();
				}

				if (!m_operatorSpawner.IsOperatorDead()) {				
					m_operatorAvailable = true;
					OnOperatorSpawned();
				}

				return;
			}

			ExtendedUpdate();
		}
	}

	protected virtual void ExtendedUpdate() {}

	protected virtual void OnRespawning() {}
	protected virtual void OnOperatorDead() {}
	protected virtual void OnOperatorSpawned() {}

	protected virtual void OnAreaLoaded() {
		m_enabled = true;
	}

	protected virtual void OnAreaExit() {
		m_enabled = false;
	}
}
