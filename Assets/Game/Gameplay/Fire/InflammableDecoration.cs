using UnityEngine;
using System.Collections.Generic;

public class InflammableDecoration : MonoBehaviour {

	[CommentAttribute("Add an explosion effect when this object is burned out.")]
	[SerializeField] private GameObject m_explosionPrefab = null;

	private GameObject m_view;
	private GameObject m_viewBurned;

	private FireNode[] m_fireNodes;
	private bool m_burned;


	// Use this for initialization
	void Start () {
		if (m_explosionPrefab != null) {
			PoolManager.CreatePool(m_explosionPrefab, 5, false);
		}

		m_view = transform.FindChild("view").gameObject;
		m_viewBurned = transform.FindChild("view_burned").gameObject;
		m_fireNodes = transform.GetComponentsInChildren<FireNode>();
		m_burned = false;

		int coins = 0;

		if (GetComponent<PreyStats>() != null) {
			coins = GetComponent<PreyStats>().reward.coins;
		}

		int coinsPerNode = coins / m_fireNodes.Length;

		for (int i = 0; i < m_fireNodes.Length - 1; i++) {
			m_fireNodes[i].Init(coinsPerNode);
		}

		m_fireNodes[m_fireNodes.Length - 1].Init(coins - (coinsPerNode * (m_fireNodes.Length - 1)));
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

			if (m_burned && m_explosionPrefab != null) {
				GameObject explosion = PoolManager.GetInstance(m_explosionPrefab.name);
				if (explosion != null) {
					Animator anim = explosion.GetComponent<Animator>();
					anim.SetTrigger("explode");

					Renderer renderer = m_view.GetComponent<Renderer>();

					if (renderer != null) {
						Vector3 pos = renderer.bounds.min;
						pos.x = renderer.bounds.center.x;
						explosion.transform.position = pos;
					} else {
						explosion.transform.position = transform.position;
					}
				}
			}
		}
	}
}
