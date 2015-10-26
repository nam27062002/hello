using UnityEngine;
using System.Collections.Generic;

public class InflammableDecoration : MonoBehaviour {
	
	private GameObject m_view;
	private GameObject m_viewBurned;

	private FireNode[] m_fireNodes;
	private bool m_burned;


	// Use this for initialization
	void Start () {
		m_view = transform.FindChild("view").gameObject;
		m_viewBurned = transform.FindChild("view_burned").gameObject;
		m_fireNodes = transform.GetComponentsInChildren<FireNode>();
		m_burned = false;
	}
	
	// Update is called once per frame
	void Update () {	
		if (m_burned) {
			m_view.SetActive(false);
			m_viewBurned.SetActive(true);
		} else {
			m_burned = true;
			for (int i = 0; i < m_fireNodes.Length && m_burned; i++) {
				m_burned = m_burned && m_fireNodes[i].IsBurned();
			}
		}
	}
}
