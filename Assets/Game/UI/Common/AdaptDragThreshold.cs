using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(UnityEngine.EventSystems.EventSystem))]
public class AdaptDragThreshold : MonoBehaviour {

	void Awake()
	{
		float inchToMove = 0.5f / 2.54f;// We want 0.5 cm to move before dragging
		int pixels = Mathf.CeilToInt( inchToMove * Screen.dpi );
		GetComponent<EventSystem>().pixelDragThreshold = pixels;
	}
}
