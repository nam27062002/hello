using UnityEngine;
using System.Collections;

public class SpawnBehaviour : MonoBehaviour {
	
	
	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private Bounds m_bounds;
	
	
	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	public void Spawn(Bounds _bounds) {
		
		m_bounds = _bounds;
		
		Spawn();
	}
	
	public void Spawn(Vector3 _position) {
		
		m_bounds = new Bounds(_position, Vector3.zero);
		
		Spawn();
	}
	
	private void Spawn() {
		
		Initializable[] components = GetComponents<Initializable>();
		
		foreach (Initializable component in components) {
			
			component.Initialize();
		}
	}
}