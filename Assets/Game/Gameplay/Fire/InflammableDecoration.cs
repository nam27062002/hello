using UnityEngine;
using System.Collections.Generic;

public class InflammableDecoration : MonoBehaviour {
	
	private SpriteRenderer m_renderer;

	private FireNode[] m_fireNodes;
	private bool m_burned;


	// Use this for initialization
	void Start () {
		m_renderer = GetComponent<SpriteRenderer>();
		m_fireNodes = transform.GetComponentsInChildren<FireNode>();
		m_burned = false;
	}
	
	// Update is called once per frame
	void Update () {	
		if (m_burned) {
			m_renderer.color = Color.Lerp(m_renderer.color, Color.black, Time.smoothDeltaTime * 1.5f);
		} else {
			m_burned = true;
			for (int i = 0; i < m_fireNodes.Length && m_burned; i++) {
				m_burned = m_burned && m_fireNodes[i].IsBurned();
			}
		}
	}
}
