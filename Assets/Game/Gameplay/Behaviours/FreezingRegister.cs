using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreezingRegister : MonoBehaviour {

	public float m_radius;
	// Use this for initialization
	void Start () {
		FreezingObjectsRegistry.instance.Register( transform, m_radius);
	}
	
	// Update is called once per frame
	void OnDestroy () {
		if ( FreezingObjectsRegistry.isInstanceCreated )
			FreezingObjectsRegistry.instance.Unregister( transform );
	}

	private void OnDrawGizmos() {
		Gizmos.color = new Color(0, 0, 1, 0.1f);
		Gizmos.DrawSphere( transform.position, m_radius);
	}
}
