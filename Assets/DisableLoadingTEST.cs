using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisableLoadingTEST : MonoBehaviour {

	// Use this for initialization
	void Start () {
		LoadingScreen.Toggle(false);
		Camera c = GetComponent<Camera>();
		c.eventMask = 0;
	}
}
