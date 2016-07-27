using UnityEngine;
using System.Collections;

public class CurseWeaponEffect : MonoBehaviour {
	
	[SerializeField] private float m_damage;
	[SerializeField] private float m_duration;

	void OnTriggerEnter(Collider _other) {
		if (_other.tag == "Player") {
			DragonHealthBehaviour dragon = InstanceManager.player.GetComponent<DragonHealthBehaviour>();
			if (dragon != null) {
				dragon.Curse(m_damage, m_duration);
			}
		}
	}
}
