using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PreyStats))]
public class HittableBehaviour : MonoBehaviour {

	[SerializeField] private float m_healthRegen = 0.05f;
	private PreyStats m_prey;

	// Use this for initialization
	void Start() {
		m_prey = GetComponent<PreyStats>();
	}

	void FixedUpdate() {
		//m_prey.AddLife(m_healthRegen);
	}

	public void OnHit(float _damage) {
		/*if (m_prey.health > 0) {
			m_prey.AddLife(-_damage);
			if (m_prey.health <= 0) {
				// Get the reward to be given from the prey stats
				Reward reward = m_prey.GetOnKillReward();
				
				// Dispatch global event
				Messenger.Broadcast<Transform, Reward>(GameEvents.ENTITY_DESTROYED, this.transform, reward);
				
				gameObject.SetActive(false);
			}
		}*/
	}
}
