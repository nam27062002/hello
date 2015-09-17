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
			m_spawner.RemoveEntity(gameObject);
			m_spawner = null;
		}
	}

	public void Spawn(Spawner _spawner, AreaBounds _bounds) {
		
		m_spawner = _spawner;
		m_area = _bounds;
		
		Spawn();
	}
	
	public void Spawn(Vector3 _position, Spawner _spawner) {
		
		m_area = new CircleAreaBounds(_position, 0);
		m_spawner = _spawner;
		
		Spawn();
	}

	private void Spawn() {
		
		transform.position = m_area.RandomInside();

		Initializable[] components = GetComponents<Initializable>();
		
		foreach (Initializable component in components) {
			
			component.SetAreaBounds(m_area);
			component.Initialize();
		}
	}
}