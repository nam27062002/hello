﻿using UnityEngine;
using System.Collections.Generic;

public class FireNode : MonoBehaviour {

	enum State {
		Idle,
		Damaged,
		Burning,
		Burned
	};

	[SerializeField] private float m_resistance = 50f;
	[SerializeField] private float m_burningTime = 10f;
	[SerializeField] private float m_damagePerTick = 0.75f;
	[SerializeField] private float m_maxDistanceLinkNode = 5f;
	[SerializeField] private Range m_fireScale = new Range(2.25f, 2.75f);


	private List<FireNode> m_neighbours;
	private State m_state;
	
	private float m_timer;

	private GameObject m_fireSprite;
	private float m_spriteScale;


	// Use this for initialization
	void Start () {
	
		FirePropagationManager.Insert(transform);

		// get two closets neighbours
		m_neighbours = new List<FireNode>();
		FireNode[] nodes = transform.parent.GetComponentsInChildren<FireNode>();

		for (int i = 0; i < nodes.Length; i++) {
			if (nodes[i] != null && nodes[i] != this) {
				float d = (nodes[i].transform.position - transform.position).sqrMagnitude;

				if (d <= m_maxDistanceLinkNode * m_maxDistanceLinkNode) {
					m_neighbours.Add(nodes[i]);
				}
			}
		}

		m_state = State.Idle;
		m_fireSprite = null;

		m_spriteScale = m_fireScale.GetRandom();
	}

	void Update() {

		if (m_state == State.Burning) {	
			//check if we have to render the particle
			Vector2 pos = transform.position;
			Vector2 cameraPos = Camera.main.transform.position;
			float d = (pos - cameraPos).sqrMagnitude;

			if (d < 20f * 20f) 	StartFire();
			else 	 			StopFire();

			if (m_fireSprite != null) {
				m_fireSprite.transform.localScale = Vector3.Lerp(m_fireSprite.transform.localScale, Vector3.one * m_spriteScale, Time.smoothDeltaTime * 1.5f);
			}

			//burn near nodes and fuel them
			m_timer -= Time.deltaTime;
			if (m_timer > 0) {
				for (int i = 0; i < m_neighbours.Count; i++) {
					m_neighbours[i].Burn(m_damagePerTick); // what amount of damage should
				}
			} else {
				m_state = State.Burned;
			}
		} else if (m_state == State.Burned) {
			if (m_fireSprite != null) {
				m_fireSprite.transform.localScale = Vector3.Lerp(m_fireSprite.transform.localScale, Vector3.zero, Time.smoothDeltaTime);

				if (m_fireSprite.transform.localScale.x < 0.1f) {
					StopFire();
				}
			}
		}
	}

	public bool IsBurned() {
		return m_state > State.Damaged && m_timer < m_burningTime * 0.5f;
	}

	public void Burn(float _damage) {

		if (m_state == State.Idle || m_state == State.Damaged) {
			m_resistance -= _damage;
			m_state = State.Damaged;

			if (m_resistance <= 0) {
				m_state = State.Burning;
				m_timer = m_burningTime;

				FirePropagationManager.Remove(transform);
			}
		}
	}


	private void StartFire() {
		if (m_fireSprite == null) {
			m_fireSprite = PoolManager.GetInstance("FireSprite");
			m_fireSprite.transform.position = transform.position;
			m_fireSprite.transform.localScale = Vector3.zero;
		}
	}

	private void StopFire() {		
		if (m_fireSprite != null) {
			m_fireSprite.SetActive(false);
		}
		m_fireSprite = null;
	}

	/// <summary>
	/// Raises the draw gizmos event.
	/// </summary>
	void OnDrawGizmos() {

		Gizmos.color = new Color(0.69f, 0.09f, 0.12f);

		if (m_state == State.Damaged) {
			Gizmos.color = Color.yellow;
		} else if (m_state == State.Burning) {
			Gizmos.color = Color.magenta;
		} else if (m_state == State.Burned) {
			Gizmos.color = Color.black;
		}

		Gizmos.DrawSphere(transform.position, 0.5f);

		if (m_neighbours != null) {
			for (int i = 0; i < m_neighbours.Count; i++) {
				if (m_state != State.Burning) {
					Color color = Gizmos.color;
					color.a = 0.2f;
					Gizmos.color = color;
				}

				Gizmos.DrawLine(transform.position, m_neighbours[i].transform.position);
			}
		}
	}
}
