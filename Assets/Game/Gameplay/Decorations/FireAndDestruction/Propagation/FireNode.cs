﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireNode : MonoBehaviour, IQuadTreeItem {
	private enum State {
		Extinguished = 0,
		Spreading,
		Burning,
		Extinguish
	};

	private Decoration m_decoration;
	private Transform m_transform;
	private GameCamera m_newCamera;

	private ParticleData m_feedbackParticle;
	private ParticleData m_burnParticle;
	private FireProcController m_fireSprite;

	private bool m_feedbackParticleMatchDirection = false;
	private float m_hitRadius = 0f;

	private Bounds m_bounds;
	private Rect m_rect;
	public Rect boundingRect { get { return m_rect; } }

	private CircleAreaBounds m_area;
	public CircleAreaBounds area { get { return m_area; } }

	private DragonTier m_breathTier;
	public DragonTier breathTier { get{ return m_breathTier; } }

	private List<FireNode> m_neighbours;
	private List<float> m_neihboursFireResistance;


	private float m_timer;
	private float m_powerTimer;


	private State m_state;
	private State m_nextState;


	// Use this for initialization
	void Start() {
		m_transform = transform;

		m_bounds = new Bounds(m_transform.position, Vector3.one * m_hitRadius * 2f);
		m_rect = new Rect((Vector2)m_transform.position, Vector2.zero);

		m_newCamera = Camera.main.GetComponent<GameCamera>();
		m_area = new CircleAreaBounds(m_transform.position, m_hitRadius);

		gameObject.SetActive(false);
	}

	public void Init(Decoration _decoration, ParticleData _burnParticle, ParticleData _feedbackParticle, bool _feedbackParticleMatchDirection, float _hitRadius) {		
		m_decoration = _decoration;

		m_burnParticle = _burnParticle;
		m_feedbackParticle = _feedbackParticle;

		m_feedbackParticleMatchDirection = _feedbackParticleMatchDirection;
		m_hitRadius = _hitRadius;

		Reset();

		FirePropagationManager.Insert(this);
	}

	public void Reset() {
		StopFireEffect();

		m_state = State.Extinguished;
		m_nextState = m_state;

		if (m_neighbours == null) {
			FindNeighbours();
		}
		SetNeighboursDistance();

		m_timer = 0f;
	}

	public void Disable() { m_state = State.Extinguish; }

	public bool IsSpreadingFire() 	{ return m_state == State.Spreading;  }
	public bool IsBurning() 		{ return m_state == State.Burning; 	  }
	public bool IsExtinguishing() 	{ return m_state == State.Extinguish; }


	public void Burn(Vector2 _direction, bool _dragonBreath, DragonTier _tier) {
		if (m_state == State.Extinguished) {
			ZoneManager.ZoneEffect effect = InstanceManager.zoneManager.GetFireEffectCode(m_decoration, _tier);
			m_breathTier = _tier;

			if (effect >= ZoneManager.ZoneEffect.M) {
				if (effect == ZoneManager.ZoneEffect.L) {
					m_nextState = State.Extinguish;
				} else {
					m_nextState = State.Spreading;
				}
			} else {
				// Dragon can't burn this thing, so lets put a few feedback particles
				if (_dragonBreath && m_timer <= 0f) {
					GameObject hitParticle = m_feedbackParticle.Spawn(m_transform.position);
					if (hitParticle != null && m_feedbackParticleMatchDirection) {
						Vector3 angle = (_direction.x < 0)? Vector3.down : Vector3.up;

						hitParticle.transform.rotation = Quaternion.Euler(angle * 90f);
					}
					m_timer = 0.5f;
				}
			}
		}
	}

	public void Extinguish() {
		if (m_state == State.Burning) {
			m_nextState = State.Extinguish;
		}
	}

	public void UpdateLogic() {
		if (m_state != m_nextState) {
			ChangeState();
		}

		float dt = Time.deltaTime * (1f + ((int)m_breathTier * 0.4f));

		if (m_powerTimer < 1f) {
			m_powerTimer += dt;
			if (m_powerTimer > 1f) {
				m_powerTimer = 1f;
			}
		}

		switch (m_state) {
			case State.Extinguished:
				m_timer -= dt;
				if (m_timer <= 0f) {
					m_timer = 0f;
				}
				break;

			case State.Spreading:
				ToogleEffect();
			
				if (m_fireSprite != null) {
					m_fireSprite.SetPower(m_powerTimer * 6f);
				}

				bool allBurned = true;
				for (int i = 0; i < m_neihboursFireResistance.Count; i++) {
					if (m_neighbours[i].IsSpreadingFire()) {
						m_neihboursFireResistance[i] = 0;
					} else {
						allBurned = false;
						if (m_neihboursFireResistance[i] > 0.1f) {
							m_neihboursFireResistance[i] *= 0.5f;						
						} else {
							m_neighbours[i].Burn(Vector2.zero, false, m_breathTier);
						}
					}
				}

				if (allBurned) {
					m_nextState = State.Burning;
				}
				break;

			case State.Burning:				
				if (m_fireSprite != null) {
					m_fireSprite.SetPower(m_powerTimer * 6f);
				}

				ToogleEffect();
				break;

			case State.Extinguish:
				if (m_fireSprite != null) {
					m_fireSprite.SetPower(6f + (m_powerTimer * (-6f)));
				}

				if (m_timer > 0) {
					m_timer -= dt;
					if (m_timer <= 0) {
						StopFireEffect();
					}
				}
				break;
		}
	}

	private void ChangeState() {
		switch (m_nextState) {
			case State.Spreading:
				m_powerTimer = 0f;
				StartFireEffect();
				break;

			case State.Burning:
				break;

			case State.Extinguish:
				m_powerTimer = 0f;
				m_timer = 6f;
				break;
		}

		m_state = m_nextState;
	}

	private void ToogleEffect() {
		// Show / Hide fire effect if this node is inside Camera or not
		bool isInsideActivationMaxArea = m_newCamera.IsInsideActivationMaxArea(m_bounds);

		if (m_fireSprite != null) {
			if (!isInsideActivationMaxArea) {
				StopFireEffect();
			}
		} else if (isInsideActivationMaxArea) {
			StartFireEffect();
		}
	}

	private void StartFireEffect() {
		FirePropagationManager.InsertBurning(this);
		if (m_fireSprite == null) {
			GameObject go = m_burnParticle.Spawn(m_transform.position);

			if (go != null) {
				m_fireSprite = go.GetComponent<FireProcController>();

				if (m_state == State.Spreading) {
					m_fireSprite.SetPower(m_powerTimer * 6f);				
				} else if (m_state == State.Extinguish) {
					m_fireSprite.SetPower(6f + (m_powerTimer * (-6f)));
				}
			}
		}
	}

	private void StopFireEffect() {
		FirePropagationManager.RemoveBurning(this);
		if (m_fireSprite != null) {
			m_fireSprite.gameObject.SetActive(false);
			m_burnParticle.ReturnInstance(m_fireSprite.gameObject);
		}
		m_fireSprite = null;
	}

	private void FindNeighbours() {
		m_neighbours = new List<FireNode>();
		m_neihboursFireResistance = new List<float>();
		FireNode[] nodes = m_transform.parent.GetComponentsInChildren<FireNode>(true);
		
		for (int i = 0; i < nodes.Length; i++) {
			if (nodes[i] != null && nodes[i] != this) {
				m_neighbours.Add(nodes[i]);
				m_neihboursFireResistance.Add(0);
			}
		}
	}

	private void SetNeighboursDistance() {
		for (int i = 0; i < m_neighbours.Count; i++) {
			m_neihboursFireResistance[i] = Vector3.SqrMagnitude(m_transform.position - m_neighbours[i].transform.position);
		}
	}


	//------------------------------------------------------------------------------
	public void OnDrawGizmosSelected() {
		Gizmos.color = Colors.WithAlpha(Colors.magenta, 0.5f);
		Gizmos.DrawSphere(transform.position, 0.5f);

		Gizmos.color = Colors.fuchsia;
		Gizmos.DrawWireSphere(transform.position, m_hitRadius);
	
		if (m_neighbours == null || m_neighbours.Count == 0) {
			FindNeighbours();
		}

		for (int i = 0; i < m_neighbours.Count; i++) {
			Gizmos.color = Colors.WithAlpha(Colors.magenta, 0.15f);
			Gizmos.DrawSphere(m_neighbours[i].transform.position, 0.5f);
		}
	}
}
