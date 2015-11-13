using UnityEngine;
using System.Collections;

public class PathSpawner : Spawner {

	private PathIA m_path;

	// Use this for initialization
	override protected void Start() {		
		base.Start();			
		m_path = GetComponent<PathIA>();
	}
	
	// Update is called once per frame
	override protected void ExtendedSpawn() {
		

	}
}
