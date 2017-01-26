using UnityEngine;
using System.Collections;

public class EntityGroupController : MonoBehaviour 
{
	[SerializeField] private AI.Group.Formation m_formation;
	public AI.Group flock = new AI.Group();

	private GameObject[] m_entities;
	public GameObject[] entities { get { return m_entities; } }


	// Use this for initialization
	public void Init(int _maxEntities) {
		flock = new AI.Group();
		flock.SetFormation(m_formation);
        m_entities = new GameObject[_maxEntities];
	}

	public void Add(GameObject _entity) {
		for (int i = 0; i < m_entities.Length; i++) {
			if (m_entities[i] == null) {
				m_entities[i] = _entity;
				break;
			}
		}
	}

	public void Remove(GameObject _entity) {
		for (int i = 0; i < m_entities.Length; i++) {
			if (m_entities[i] == _entity) {
				m_entities[i] = null;
				break;
			}
		}
	}


	void OnDrawGizmosSelected() {
		if (flock == null) {
			flock = new AI.Group();
		}
			
		for (int i = 0; i < 100; i++) {
			switch(m_formation) {
				case AI.Group.Formation.SunFlower:
					Gizmos.DrawSphere(transform.position + flock.GetSunflowerPosAt(i), 0.15f);
					break;

				case AI.Group.Formation.Triangle:
					Gizmos.DrawSphere(transform.position + flock.GetTrianglePosAt(i), 0.15f);
					break;
			}

		}
	}
}
