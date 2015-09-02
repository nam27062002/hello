using UnityEngine;
using System.Collections;

public class CircleArea2D : MonoBehaviour {

	public Vector2 offset;
	public float radius = 50f;
	
	public Color color = new Color(0.76f, 0.23f, 0.13f, 0.2f);

	public Bounds bounds {
		get {
			Vector3 center = transform.position + (Vector3)offset;
			return new Bounds(center, new Vector3(radius * 2f, radius * 2f, 0f));
		}
	}
}
