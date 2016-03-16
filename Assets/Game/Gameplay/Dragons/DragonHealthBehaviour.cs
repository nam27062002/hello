using UnityEngine;
using System.Collections.Generic;

public class DragonHealthBehaviour : MonoBehaviour {


	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private DragonPlayer m_dragon;
	private Animator m_animator;

	private GameSceneControllerBase m_gameController;

	private float m_healthDrainPerSecond;
	private List<TimeDrain> m_healthDrainIncForTime;

	private float m_elapsedSecondsCheckPoint;
	private int m_nextIncrement;
	
	//-----------------------------------------------
	// Methods
	//-----------------------------------------------

	// Use this for initialization
	void Start() {
		m_dragon = GetComponent<DragonPlayer>();
		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_gameController = InstanceManager.GetSceneController<GameSceneControllerBase>();

		m_healthDrainPerSecond = m_dragon.data.def.GetAsFloat("healthDrain");
		m_healthDrainIncForTime = GameSettings.healthDrainIncForTime;

		m_elapsedSecondsCheckPoint = 0;
		m_nextIncrement = 0;
	}
		
	// Update is called once per frame
	void Update() {
		if (m_healthDrainIncForTime.Count > 0) {
			float secondsDelta = m_gameController.elapsedSeconds - m_elapsedSecondsCheckPoint;
			if (secondsDelta >= m_healthDrainIncForTime[m_nextIncrement].seconds) {
				m_healthDrainPerSecond += m_healthDrainIncForTime[m_nextIncrement].drainIncrement;

				m_elapsedSecondsCheckPoint = m_gameController.elapsedSeconds;
				if (m_nextIncrement < (m_healthDrainIncForTime.Count - 1)) {
					m_nextIncrement++;
				}
			}
		}
		m_dragon.AddLife(-Time.deltaTime * m_healthDrainPerSecond);
	}

	public bool IsAlive() {
		return m_dragon.IsAlive();
	}

	public void ReceiveDamage(float _value, Transform _source = null) {
		if (enabled) {
			m_animator.SetTrigger("damage");// receive damage?
			m_dragon.AddLife(-_value);
			Messenger.Broadcast<float, Transform>(GameEvents.PLAYER_DAMAGE_RECEIVED, _value, _source);
		}
	}

}
