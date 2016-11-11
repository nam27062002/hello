using UnityEngine;
using System.Collections;

public class FireTypeAutoSelector : MonoBehaviour {
	[SerializeField] private GameObject m_fireRush; 
	[SerializeField] private GameObject m_superFireRush;

	void OnEnable() {
		if (InstanceManager.player != null) {
			if (InstanceManager.player.IsSuperFuryOn()) {
				m_fireRush.SetActive(false);
				m_superFireRush.SetActive(true);
			} else {
				m_fireRush.SetActive(true);
				m_superFireRush.SetActive(false);
			}	
		}
	}
}
