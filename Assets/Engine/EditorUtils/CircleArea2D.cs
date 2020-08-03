using UnityEngine;
using System.Collections;

public class CircleArea2D : MonoBehaviour, Area {

	public Vector2 offset;
	public float radius = 5f;	
	public Color color = new Color(0.76f, 0.23f, 0.13f, 0.2f);

	//
	//Helpers
	private CircleAreaBounds m_bounds;
	private Vector2 m_distance;
	private Vector3 m_center;
	private Transform m_transform;

	//

	public AreaBounds bounds {
		get {
			if (m_bounds == null) {
				m_bounds = new CircleAreaBounds(center, radius);
			} else {
				m_bounds.UpdateBounds(center, GameConstants.Vector3.one * radius * 2f);
			}

			return m_bounds;
		}
	}

	public Vector3 center { 
		get {
            if (m_transform == null) {
                m_transform = transform;
            }			
			m_center = m_transform.position;
			m_center.x += offset.x;
			m_center.y += offset.y;
			return m_center;
		}
	}

	private void Awake() {
		m_transform = transform;
		m_center = m_transform.position;
	}

	public float DistanceSqr( Vector3 position )
	{
		Vector2 v = (center - position);
		float sqrMagnitude = v.sqrMagnitude;
		if ( sqrMagnitude > radius * radius )
		{
			return sqrMagnitude - (radius * radius);
		}
		return 0;

	}

	public bool Overlaps( Rect r )
	{
		m_distance.x = Mathf.Abs(center.x - r.center.x);
		m_distance.y = Mathf.Abs(center.y - r.center.y);

		// too far away
		if (m_distance.x > (r.width/2.0f + radius)) { return false; }
		if (m_distance.y > (r.height/2.0f + radius)) { return false; }

    	// center inside rectangle
		if (m_distance.x <= (r.width/2.0f)) { return true; } 
		if (m_distance.y <= (r.height/2.0f)) { return true; }

    	// Other cases
    	// sqr magnitude
		float cornerDistance_sq = Mathf.Pow( (m_distance.x - r.width/2), 2) + Mathf.Pow((m_distance.y - r.height/2), 2);

    	return (cornerDistance_sq <= (radius*radius));
		
	}

	public bool Overlaps(Vector2 _center, float _radius)
	{
        return MathTest.TestCircleVsCircle(_center, _radius, this.center, this.radius);
	}

	public bool Overlaps( CircleArea2D _circle )
	{
		return Overlaps(_circle.center, _circle.radius);
	}

	public bool IsInside( Vector2 _point )
	{
		Vector3 c = center;

		Vector2 v;
		v.x = c.x;
		v.y = c.y;

		v = (v - _point);
		float sqrMagnitude = v.sqrMagnitude;
		return ( sqrMagnitude <= radius * radius );
	}

	public bool OverlapsSegment( Vector2 a, Vector2 b)
	{
		Vector2 closestPoint = ClosestPointInSegment( a, b );
		return IsInside( closestPoint );
	}

	public float SqrDistanceToSegment( Vector2 a, Vector2 b )
	{
		Vector2 closestPoint = ClosestPointInSegment( a, b );
		return  ((Vector2) center - closestPoint).sqrMagnitude;;		
	}

	public Vector2 ClosestPointInSegment(Vector2 a, Vector2 b)
	{
		Vector2 aToCenter;
		aToCenter.x = center.x - a.x;
		aToCenter.y = center.y - a.y;

		Vector2 aToB = b-a;

		Vector2 closestPoint = Vector2.zero;

		float k = Vector2.Dot(aToCenter, aToB);
		if ( k <= 0.0f )
		{
			closestPoint = a;
		}
		else
		{
			float denom = Vector2.Dot( aToB, aToB );
			if ( k >= denom )
			{
				closestPoint = b;
			}
			else
			{
				k = k/denom;
				closestPoint = a + k * aToB;	
			}

		}
		return closestPoint;
	}
}
