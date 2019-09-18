using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDevice : ISpawnable, IBroadcastListener {

	private AutoSpawnBehaviour m_autoSpawner;
	private InflammableDecoration m_inflammable;

	protected DeviceOperatorSpawner m_operatorSpawner;
	protected bool m_operatorAvailable;

	protected bool m_enabled;


    protected virtual void Awake() {
		m_autoSpawner = GetComponent<AutoSpawnBehaviour>();
		m_operatorSpawner = GetComponent<DeviceOperatorSpawner>();
		m_inflammable = GetComponent<InflammableDecoration>();

        m_operatorAvailable = false;
		m_enabled = false;

        // Subscribe to external events
        Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
        Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);
        Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);
    }

    override public void Spawn(ISpawner _spawner) {
        OnRespawn();
    }

	/// <summary>
	/// Component disabled.
	/// </summary>
	protected virtual void OnDestroy() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_ENDED, this);
	}

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.GAME_LEVEL_LOADED:
            case BroadcastEventType.GAME_AREA_ENTER:
            {
                OnAreaLoaded();
            }break;
            case BroadcastEventType.GAME_ENDED:
            {
                OnAreaExit();
            }break;
        }
    }

    // Update is called once per frame
    override public void CustomUpdate() { 
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
    protected virtual void OnRespawn() { }
    protected virtual void OnOperatorDead() {}
	protected virtual void OnOperatorSpawned() {}

	protected virtual void OnAreaLoaded() {
		m_enabled = true;
        m_operatorAvailable = false;
    }

	protected virtual void OnAreaExit() {
		m_enabled = false;
	}
}
