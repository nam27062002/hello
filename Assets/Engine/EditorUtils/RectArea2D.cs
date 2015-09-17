﻿using UnityEngine;
using System.Collections;

public class RectArea2D : MonoBehaviour, Area {

	public Vector2 offset;
	public Vector2 size = new Vector2(100f, 100f);

	public Color color = new Color(0.76f, 0.23f, 0.13f, 0.2f);
	
	public AreaBounds bounds {
		get {
			Vector3 center = transform.position + (Vector3)offset;
			return new RectAreaBounds(center, size);
		}
	}
}
