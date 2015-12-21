using UnityEngine;
using System.Collections;

public class CircleArea2D : MonoBehaviour, Area {

	public Vector2 offset;
	public float radius = 5f;
	
	public Color color = new Color(0.76f, 0.23f, 0.13f, 0.2f);

	public AreaBounds bounds {
		get {
			// Vector3 center = transform.position + (Vector3)offset;
			return new CircleAreaBounds(center, radius);
		}
	}

	public Vector3 center
	{
		get
		{
			return transform.position + (Vector3)offset;
		}
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
		Vector2 distance;

		distance.x = Mathf.Abs(center.x - r.center.x);
		distance.y = Mathf.Abs(center.y - r.center.y);

		// too far away
    	if (distance.x > (r.width/2.0f + radius)) { return false; }
    	if (distance.y > (r.height/2.0f + radius)) { return false; }

    	// center inside rectangle
    	if (distance.x <= (r.width/2.0f)) { return true; } 
    	if (distance.y <= (r.height/2.0f)) { return true; }

    	// Other cases
    	// sqr magnitude
    	float cornerDistance_sq = Mathf.Pow( (distance.x - r.width/2), 2) + Mathf.Pow((distance.y - r.height/2), 2);

    	return (cornerDistance_sq <= (radius*radius));
		
	}
}
