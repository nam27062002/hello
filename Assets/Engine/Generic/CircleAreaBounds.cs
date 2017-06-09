using UnityEngine;

public class CircleAreaBounds : AreaBounds {
	private Bounds m_bounds;
	private float m_radius;
	public float radius
	{
		get { return m_radius; }
	}

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

	public bool IsInside(Vector2 _point) {
		Vector3 c = center;

		Vector2 v;
		v.x = c.x;
		v.y = c.y;

		v = (v - _point);
		float sqrMagnitude = v.sqrMagnitude;
		return (sqrMagnitude <= m_radius * m_radius);
	}

	public bool Overlaps(Rect _r) {
		Vector2 distance;

		distance.x = Mathf.Abs(center.x - _r.center.x);
		distance.y = Mathf.Abs(center.y - _r.center.y);

		// too far away
		if (distance.x > (_r.width/2.0f + m_radius)) { return false; }
		if (distance.y > (_r.height/2.0f + m_radius)) { return false; }

		// center inside rectangle
		if (distance.x <= (_r.width/2.0f)) { return true; } 
		if (distance.y <= (_r.height/2.0f)) { return true; }

		// Other cases
		// sqr magnitude
		float cornerDistance_sq = Mathf.Pow( (distance.x - _r.width/2), 2) + Mathf.Pow((distance.y - _r.height/2), 2);

		return (cornerDistance_sq <= (m_radius * m_radius));
	}

	public bool Overlaps( Vector2 _center, float _radius)
	{
		float sqrMagnitude = ((Vector2)this.center - _center).sqrMagnitude;
		float test = (_radius + m_radius);
		test = test * test;
		if ( sqrMagnitude <= test )
		{
			return true;
		}
		return false;
	}

	public bool OverlapsSegment(Vector2 _a, Vector2 _b) {
		Vector2 aToCenter;
		aToCenter.x = center.x - _a.x;
		aToCenter.y = center.y - _a.y;

		Vector2 aToB = _b - _a;
		Vector2 closestPoint = Vector2.zero;

		float k = Vector2.Dot( aToCenter, aToB);
		if (k < 0) {
			closestPoint = _a;
		} else {
			float magnitude = aToB.magnitude;
			k = k / magnitude;
			if ( k < magnitude )
			{
				closestPoint = _a + aToB.normalized * k;
			}
			else
			{
				closestPoint = _b;
			}
		}

		return IsInside(closestPoint);
	}

	public void DrawGizmo() {
		Color color = Color.white;
		color.a = 0.25f;

		Gizmos.color = color;
		Gizmos.DrawSphere(m_bounds.center, Mathf.Max(0.5f, m_radius));
	}
}