using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComplexDevice : Initializable, IBroadcastListener {

	private AutoSpawnBehaviour m_autoSpawner;
	private InflammableDecoration m_inflammable;

	protected DeviceOperatorSpawner[] m_operatorSpawner;
	protected bool[] m_operatorAvailable;
	protected int m_operatorCount;

	protected bool m_enabled;


	void Awake() {
		m_autoSpawner = GetComponent<AutoSpawnBehaviour>();
		m_operatorSpawner = GetComponents<DeviceOperatorSpawner>();
		m_inflammable = GetComponent<InflammableDecoration>();

		m_operatorCount = m_operatorSpawner.Length;
		m_operatorAvailable = new bool[m_operatorCount];

		for (int i = 0; i < m_operatorCount; ++i) {
			m_operatorAvailable[i] = false;
		}

		m_enabled = false;
	}

	public override void Initialize() {
		for (int i = 0; i < m_operatorCount; ++i) {
			m_operatorAvailable[i] = false;
		}
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	protected virtual void OnEnable() {
		// Subscribe to external events
		Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
        Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);
		Messenger.AddListener(MessengerEvents.GAME_ENDED, OnAreaExit);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	protected virtual void OnDisable() {
		// Unsubscribe from external events
        Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);
		Messenger.RemoveListener(MessengerEvents.GAME_ENDED, OnAreaExit);
	}

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.GAME_AREA_ENTER:
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
				for (int i = 0; i < m_operatorCount; ++i) {
					m_operatorAvailable[i] = false;
				}

				return;
			}

			for (int i = 0; i < m_operatorCount; ++i) {
				if (m_operatorAvailable[i]) {
					if (m_operatorSpawner[i].IsOperatorDead()) {
						m_operatorAvailable[i] = false;
						OnOperatorDead(i);
					}
				} else {
					// check respawn conditions
					if (m_operatorSpawner[i].IsRespawing() && m_operatorSpawner[i].CanRespawn()) {
						m_operatorSpawner[i].Respawn();
					}
				
					if (!m_operatorSpawner[i].IsOperatorDead()) {				
						m_operatorAvailable[i] = true;
						OnOperatorSpawned(i);
					}
				}
			}

			ExtendedUpdate();
		}
	}

	protected virtual void ExtendedUpdate() {}

	protected virtual void OnRespawning() {}
	protected virtual void OnOperatorDead(int _index) {}
	protected virtual void OnOperatorSpawned(int _index) {}

	protected virtual void OnAreaLoaded() {
		m_enabled = true;
	}

	protected virtual void OnAreaExit() {
		m_enabled = false;
	}
}
