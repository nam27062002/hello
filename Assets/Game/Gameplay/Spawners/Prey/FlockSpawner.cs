using UnityEngine;
using System.Collections;

[RequireComponent(typeof(EntityGroupController))]
[RequireComponent(typeof(FlockController))]
public class FlockSpawner : Spawner 
{
	protected FlockController m_flockController;

	override protected void Start()
	{
		m_flockController = GetComponent<FlockController>();
		if (m_flockController) {
			// this spawner has a flock controller! let's setup it
			m_flockController.Init();
		}

		base.Start();
	}

	/*override protected void ExtendedUpdateLogic() {
		if (m_flockController) {
			m_flockController.NextPositionAtSpeed(0f);
		}
	}*/

	/*override protected void ExtendedSpawn() {
		if (m_flockController) {
			for (int i = 0; i < m_entities.Length; i++) {
				if (m_entities[i] != null) {
					FlockBehaviour behaviour = m_entities[i].GetComponent<FlockBehaviour>();
					if (behaviour != null) {
						behaviour.SetFlock(m_flockController);
					}
				}
			}
		}
	}*/

}