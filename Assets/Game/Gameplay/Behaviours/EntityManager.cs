using UnityEngine;
using System.Collections.Generic;

public class EntityManager : SingletonMonoBehaviour<EntityManager> {

	private List<Entity> m_entities;
	private List<Entity> m_searchList;
	private Rect m_area;

	void Awake() {
		m_entities = new List<Entity>();
		m_searchList = new List<Entity>();
	}

	public void Register(Entity _entity) {
		m_entities.Add(_entity);
	}

	public void Unregister(Entity _entity) {
		m_entities.Remove(_entity);
	}

	public Entity[] GetEntitiesInRange2D(Vector2 _center, float _radius) {
		m_searchList.Clear();

		for (int i = 0; i < m_entities.Count; i++) {
			Entity e = m_entities[i];
			if (e != null) {				
				if (e.IntersectsWith(_center, _radius)) {
					m_searchList.Add(e);
				}
			}
		}

		return m_searchList.ToArray();
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

	/**
	*
	*/
	public Entity[] GetEntitiesIn( Vector2 position, Vector2 dir, float amplitude, float length)
	{
		m_searchList.Clear();
		float halfAmplitude = amplitude/2.0f;
		float angle = Mathf.Atan2( dir.y, dir.x);
		float aaa = Mathf.Rad2Deg * angle;
		for (int i = 0; i < m_entities.Count; i++) 
		{
			Entity e = m_entities[i];
			if (e != null) 
			{				
				Vector2 entityPos = (Vector2)e.transform.position;
				Vector2 inversePos = entityPos - position;
				inversePos = inversePos.RotateRadians( -angle );
				if ( inversePos.x >= 0 && inversePos.x <= length )
				{
					if ( inversePos.y >= -halfAmplitude && inversePos.y <= halfAmplitude )
					{
						m_searchList.Add( e );
					}
				}
			}
		}

		return m_searchList.ToArray();
	}
}