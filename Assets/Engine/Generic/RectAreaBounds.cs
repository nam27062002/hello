﻿using UnityEngine;

public struct RectAreaBounds : AreaBounds {
	private Bounds m_bounds;

	public RectAreaBounds(Vector3 _center, Vector3 _size) {
		
		m_bounds = new Bounds(_center, _size);
	}

	public Bounds bounds { get { return m_bounds; } }

	public Vector3 RandomInside() {
		Vector3 offset = Vector3.zero;
		offset.x = Random.Range(-m_bounds.extents.x, m_bounds.extents.x);
		offset.y = Random.Range(-m_bounds.extents.y, m_bounds.extents.y);
		offset.z = Random.Range(-m_bounds.extents.z, m_bounds.extents.z);

		return m_bounds.center + offset;
	}
		
	public bool Contains(Vector3 _point) {		
		_point.z = m_bounds.center.z;
		return m_bounds.Contains(_point);
	}
}