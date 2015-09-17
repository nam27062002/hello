using UnityEngine;
using System.Collections;

public class DragonHealthBehaviour : MonoBehaviour {


	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private DragonPlayer m_dragon;

	
	//-----------------------------------------------
	// Methods
	//-----------------------------------------------

	// Use this for initialization
	void Start() {
		m_dragon = GetComponent<DragonPlayer>();
		Messenger.AddListener<bool>(GameEvents.FURY_RUSH_TOGGLED, OnBreath);
	}

	void OnDestroy() {
		Messenger.RemoveListener<bool>(GameEvents.FURY_RUSH_TOGGLED, OnBreath);
	}
	
	// Update is called once per frame
	void Update() {
		m_dragon.AddLife(-Time.deltaTime * m_dragon.data.healthDrainPerSecond);	
	}

	public void ReceiveDamage(float _value) {
		if (enabled)
			m_dragon.AddLife(-_value);
	}

	private void OnBreath(bool _enabled) {
		enabled = !_enabled;
	}
}
