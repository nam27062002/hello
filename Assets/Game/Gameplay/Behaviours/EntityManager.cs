﻿using UnityEngine;
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
		Rect r = new Rect();
		r.size = Vector2.one * _radius * 2f;
		r.center = _center;

		for (int i = 0; i < m_entities.Count; i++) {
			Entity e = m_entities[i];
			if (e != null) {				
				if (e.IntersectsWith(r)) {
					entities.Add(e);
				}
			}
		}

		return entities.ToArray();
	}

	public Entity GetEntityInRangeNearest2D(Vector2 _center, float _radius, DragonTier _tier) {
		float minDistSqr = _radius * _radius;
		Entity nearestEntity = null;

		for (int i = 0; i < m_entities.Count; i++) {
			Entity e = m_entities[i];
			if (e != null) {
				float distSqr = Vector2.SqrMagnitude((Vector2)e.transform.position - _center);
				if (distSqr <= _radius * _radius && e.def.edibleFromTier <= _tier) {
					if (distSqr <= minDistSqr) {
						minDistSqr = distSqr;
						nearestEntity = e;
					}
				}
			}
		}

		return nearestEntity;
	}
}