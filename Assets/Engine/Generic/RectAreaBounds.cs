using UnityEngine;

public class RectAreaBounds : AreaBounds {
	private Bounds m_bounds;
	private Vector3 m_size;
	private Vector3 m_extents;

	public RectAreaBounds(Vector3 _center, Vector2 _size) {
		m_size = _size;
		m_size.z = 10f;
		m_extents = m_size * 0.5f;
		m_bounds = new Bounds(_center, m_size);
	}

	public RectAreaBounds(Vector3 _center, Vector3 _size) {	
		m_size = _size;
		m_size.z = 10f;
		m_extents = m_size * 0.5f;
		m_bounds = new Bounds(_center, m_size);
	}

	public void UpdateBounds(Vector3 _center, Vector3 _size) {	
		m_size = _size;
		m_size.z = 10f;
		m_extents = m_size * 0.5f;
		m_bounds.center = _center;
		m_bounds.size = _size;
	}

	public Bounds bounds { get { return m_bounds; } }
	public Vector3 center { get { return m_bounds.center; } }

	public float sizeX  { get { return m_size.x; } }
	public float sizeY { get { return m_size.y; } } 

	public float extentsX { get { return m_extents.x; } }
	public float extentsY { get { return m_extents.y; } }

	public void SetMinMax(Vector3 _min, Vector3 _max) {
		m_bounds.SetMinMax(_min, _max);
	}

	public Vector3 RandomInside() {
		Vector3 offset = Vector3.zero;
		offset.x = Random.Range(-m_extents.x, m_extents.x);
		offset.y = Random.Range(-m_extents.y, m_extents.y);
		offset.z = 0;//Random.Range(-m_extents.z, m_extents.z);

		return m_bounds.center + offset;
	}
		
	public bool Contains(Vector3 _point) {	
		_point.z = 0;
		return m_bounds.Contains(_point);
	}

	public void Encapsulate( Collider[] _colliders)
	{
		int size = _colliders.Length;
		for( int i = 0; i<size; ++i )
		{
			m_bounds.Encapsulate(_colliders[i].bounds);
		}
		m_size = m_bounds.size;
		m_extents = m_bounds.extents;
	} 

	public void DrawGizmo() {		
		Color color = Color.white;
		color.a = 0.25f;

		Gizmos.color = color;
		Gizmos.DrawCube(m_bounds.center, m_size);
	}
}