using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleDevice : Initializable {

	private AutoSpawnBehaviour m_autoSpawner;
	private InflammableDecoration m_inflammable;

	protected DeviceOperatorSpawner m_operatorSpawner;
	protected bool m_operatorAvailable;


	void Awake() {
		m_autoSpawner = GetComponent<AutoSpawnBehaviour>();
		m_operatorSpawner = GetComponent<DeviceOperatorSpawner>();
		m_inflammable = GetComponent<InflammableDecoration>();

		m_operatorAvailable = false;
	}

	public override void Initialize() {
		m_operatorAvailable = false;
	}

	// Update is called once per frame
	void Update () {
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
			if (m_operatorSpawner.CanRespawn()) {
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

	protected virtual void ExtendedUpdate() {}

	protected virtual void OnRespawning() {}
	protected virtual void OnOperatorDead() {}
	protected virtual void OnOperatorSpawned() {}
}
