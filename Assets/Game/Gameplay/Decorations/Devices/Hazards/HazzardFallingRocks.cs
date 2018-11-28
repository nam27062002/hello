using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;


public class HazzardFallingRocks : MonoBehaviour, IBroadcastListener {

	[Serializable]
	private class SpawnData {
		public int x;
		public float seconds;
		public float speed;
	}


	//-----------------------------------------------------------
	[SerializeField] private string m_projectileName;
	[SerializeField] private float m_damage;
	[SeparatorAttribute]
	[SerializeField] private HazzardTrigger[] m_triggers;
	[SeparatorAttribute]
	[SerializeField] private SpawnData[] m_spawnData;
	[SeparatorAttribute]
	[SerializeField] private bool m_resetOnDisable;
	[SerializeField] private int m_maxSpawns = 1;


	//-----------------------------------------------------------
	private bool m_enabled;
	private int m_spawnCount;
	private float[] m_timers;

	private PoolHandler m_poolHandler;


	//-----------------------------------------------------------
	// Use this for initialization
	private void Awake() {
		Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);
	}

	private void Start() {
		m_enabled = false;
		m_spawnCount = 0;

		InitTimers();
		CreatePool();
	}

	private void OnDestroy() {
		Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);
	}

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.GAME_AREA_ENTER:
            {
                CreatePool();
            }break;
        }
    }
    
	void CreatePool() {
		m_poolHandler = PoolManager.RequestPool(m_projectileName, "Game/Projectiles/", m_spawnData.Length);
	}

	private void InitTimers() {
		m_timers = new float[m_spawnData.Length];
		ResetTimers();
	}

	private void ResetTimers() {
		for (int i = 0; i < m_spawnData.Length; ++i) {
			m_timers[i] = Math.Max(0.1f, m_spawnData[i].seconds);
		}
	}

	// Update is called once per frame
	private void Update() {
		int numTriggers = 0;
		for (int i = 0; i < m_triggers.Length; ++i) {
			if (m_triggers[i].isTriggered)
				numTriggers++;
		}

		if ((numTriggers % 2) == 1) {
			Activate();
		} else {
			Deactivate();
		}

		if (m_enabled) {
			for (int i = 0; i < m_timers.Length; ++i) {
				if (m_timers[i] > 0f) {
					m_timers[i] -= Time.deltaTime;
					if (m_timers[i] <= 0f) {
						GameObject projectileGO = m_poolHandler.GetInstance();

						if (projectileGO != null) {
							IProjectile projectile = projectileGO.GetComponent<IProjectile>();
							if (projectile != null) {
								projectile.AttachTo(transform, GameConstants.Vector3.right * m_spawnData[i].x);
								projectile.ShootTowards(GameConstants.Vector3.down, m_spawnData[i].speed, m_damage, null);
							} else {
								projectileGO.transform.position = transform.position + GameConstants.Vector3.right * m_spawnData[i].x;
							}
						}
					}
				}
			}
		}
	}

	private void Activate() {
		m_enabled = true;
	}

	private void Deactivate() {
		m_enabled = false;

		bool allSpawned = true;
		for (int i = 0; i < m_timers.Length; ++i) {
			if (m_timers[i] > 0f) {
				allSpawned = false;
			}
		}

		if (allSpawned) {
			m_spawnCount++;

			if (m_spawnCount < m_maxSpawns) {
				ResetTimers();
			}
		} else if (m_resetOnDisable) {
			ResetTimers();
		}
	}

	private void OnDrawGizmos() {
		Gizmos.color = (m_enabled)? Colors.green : Colors.magenta;
		Gizmos.DrawSphere(transform.position, 1f);

		if (m_timers == null || m_timers.Length != m_spawnData.Length) {
			InitTimers();
		}

		Vector3 offset = new Vector3();
		for (int i = 0; i < m_spawnData.Length; ++i) {
			offset.x = m_spawnData[i].x;
			offset.y = 1.5f + m_spawnData[i].seconds;
			offset.z = -20f;
		
			Gizmos.color = (m_timers[i] > 0f)? Colors.yellow : Colors.white;
			Gizmos.DrawSphere(transform.position + offset, 0.5f);
		}
	}
}
