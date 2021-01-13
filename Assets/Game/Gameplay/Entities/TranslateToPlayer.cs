using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslateToPlayer : MonoBehaviour {

	Transform m_transform;
	Transform m_target;
	public float m_speed = 5.0f;
	// Use this for initialization
	void Start () {
		m_transform = transform;
		m_target = InstanceManager.player.dragonEatBehaviour.mouth;
	}
	
	// Update is called once per frame
	void Update () {
		m_transform.position = Vector3.Lerp(m_transform.position, m_target.position, m_speed * Time.deltaTime);
	}
}
