using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RBodyMotionTEST : MonoBehaviour {

	public Rigidbody m_body;


	
	// Update is called once per frame
	void Update () {
		m_body.velocity = Vector3.right * 2f;	
	}
}
