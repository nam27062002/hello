using UnityEngine;
using System.Collections;

public class SpawnBehaviour : MonoBehaviour {
	
	
	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private AreaBounds m_area;
	private Spawner m_spawner;
	
	
	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	void OnDisable() {
		if (m_spawner) {
			m_spawner.RemoveEntity(gameObject, false);
			m_spawner = null;
		}
	}

	public void Spawn(Spawner _spawner, Vector3 _position, AreaBounds _bounds) {
		
		m_spawner = _spawner;
		m_area = _bounds;
		
		transform.position = _position;

		Initializable[] components = GetComponents<Initializable>();

		foreach (Initializable component in components) {
			component.SetAreaBounds(m_area);
			component.Initialize();
		}
	}
}