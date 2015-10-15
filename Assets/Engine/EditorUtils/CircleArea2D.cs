using UnityEngine;
using System.Collections;

public class CircleArea2D : MonoBehaviour, Area {

	public Vector2 offset;
	public float radius = 25f;
	
	public Color color = new Color(0.76f, 0.23f, 0.13f, 0.2f);

	public AreaBounds bounds {
		get {
			Vector3 center = transform.position + (Vector3)offset;
			return new CircleAreaBounds(center, radius);
		}
	}
}
