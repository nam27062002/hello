using UnityEngine;
using System.Collections;

[RequireComponent(typeof(FlockController))]
public class FlockSpawner : Spawner {
	
	private FlockController m_flockController;

	// Use this for initialization
	override protected void Start() {

		base.Start();

		m_flockController = GetComponent<FlockController>();
		m_flockController.Init(m_quantity.max);
	}

	override protected void ExtendedSpawn() {

		Vector3 position = m_area.RandomInside();
		for (int i = 0; i < m_entities.Length; i++) {
			if (m_entities[i] != null) {
				m_entities[i].transform.position = position;

				Flock bird = m_entities[i].GetComponent<Flock>();
				if (bird != null) {
					bird.AttachFlock(m_flockController);
				}
			}
		}
	}
}