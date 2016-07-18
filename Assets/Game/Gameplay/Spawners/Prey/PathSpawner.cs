using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PathController))]
public class PathSpawner : Spawner {

	private PathController m_path;

	// Use this for initialization
	void Awake() {
		m_path = GetComponent<PathController>();
	}
	
	// Update is called once per frame
	override protected void ExtendedSpawn() {
		Vector3 position = m_path.GetRandom();
		for (int i = 0; i < m_entities.Length; i++) {
			if (m_entities[i] != null) {
				m_entities[i].transform.position = position;
				
				FollowPathBehaviour pathBehaviour = m_entities[i].GetComponent<FollowPathBehaviour>();
				if (pathBehaviour != null) {
					pathBehaviour.SetPath(m_path);
				}

				FleePathBehaviour fleePathBehaviour = m_entities[i].GetComponent<FleePathBehaviour>();
				if (fleePathBehaviour != null) {
					fleePathBehaviour.SetPath(m_path);
				}
			}
		}
	}

	override protected AreaBounds GetArea() {
		m_path = GetComponent<PathController>();
		return m_path.GetBounds();
	}

	// On spawn we move a little all the entities
	override protected Vector3 RandomStartDisplacement()
	{
		Vector3 random = Vector3.zero;
		random.x = Random.Range(-2.0f, 2.0f);
		random.z = Random.Range(-2.0f, 2.0f);
		return random;
	}
}
