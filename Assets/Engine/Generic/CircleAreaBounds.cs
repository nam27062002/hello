using UnityEngine;

public class CircleAreaBounds : AreaBounds {
	private Bounds m_bounds;
	private float m_radius;
	private float m_diameter;

	public CircleAreaBounds(Vector3 _center, float _radius) {	
		m_radius = _radius;
		m_diameter = _radius * 2f;
		m_bounds = new Bounds(_center, new Vector3(m_diameter, m_diameter, m_diameter));
	}

	public void UpdateBounds(Vector3 _center, Vector3 _size) {	
		m_diameter = _size.x;
		m_radius = _size.x * 0.5f;
		m_bounds.center = _center;
		m_bounds.size = _size;
	}

	public Bounds bounds { get { return m_bounds; } }
	public Vector3 center { get { return m_bounds.center; } }

	public float sizeX  { get { return m_diameter; } }
	public float sizeY { get { return m_diameter; } } 

	public float extentsX { get { return m_radius; } }
	public float extentsY { get { return m_radius; } }

	public Vector3 RandomInside() {
		Vector2 offset = Random.insideUnitCircle * m_radius;
		return m_bounds.center + (Vector3)offset;
	}

	public bool Contains(Vector3 _point) {
		_point.z = m_bounds.center.z;
		return m_bounds.Contains(_point);
	}

	public void DrawGizmo() {
		Color color = Color.white;
		color.a = 0.25f;

		Gizmos.color = color;
		Gizmos.DrawSphere(m_bounds.center, Mathf.Max(0.5f, m_radius));
	}
}