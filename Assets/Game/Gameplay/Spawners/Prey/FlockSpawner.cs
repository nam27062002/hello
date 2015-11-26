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

		for (int i = 0; i < m_entities.Length; i++) {
			if (m_entities[i] != null) {
				PreyMotion motion = m_entities[i].GetComponent<PreyMotion>();
				if (motion != null) {
					motion.AttachFlock(m_flockController);
				}
			}
		}
	}
}