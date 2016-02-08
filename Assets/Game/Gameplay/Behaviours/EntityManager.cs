using UnityEngine;
using System.Collections.Generic;

public class EntityManager : SingletonMonoBehaviour<EntityManager> {

	private List<Entity> m_entities;

	void Awake() {
		m_entities = new List<Entity>();
	}

	public void Register(Entity _entity) {
		m_entities.Add(_entity);
	}

	public void Unregister(Entity _entity) {
		m_entities.Remove(_entity);
	}

	public Entity[] GetEntitiesInRange2D(Vector2 _center, float _radius) {
		List<Entity> entities = new List<Entity>();

		for (int i = 0; i < m_entities.Count; i++) {
			if (m_entities[i] != null) {
				float distSqr = Vector2.SqrMagnitude((Vector2)m_entities[i].transform.position - _center);
				if (distSqr <= _radius * _radius) {
					entities.Add(m_entities[i]);
				}
			}
		}

		return entities.ToArray();
	}

	public Entity GetEntityInRangeNearest2D(Vector2 _center, float _radius) {
		float minDistSqr = _radius * _radius;
		Entity nearestEntity = null;

		for (int i = 0; i < m_entities.Count; i++) {
			if (m_entities[i] != null) {
				float distSqr = Vector2.SqrMagnitude((Vector2)m_entities[i].transform.position - _center);
				if (distSqr <= _radius * _radius) {
					if (distSqr <= minDistSqr) {
						minDistSqr = distSqr;
						nearestEntity = m_entities[i];
					}
				}
			}
		}

		return nearestEntity;
	}
}