using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireNode : MonoBehaviour, IQuadTreeItem {
	private enum State {
		Extinguished = 0,
		Spreading,
		Burning,
		Extinguish
	};

	private ParticleData m_feedbackParticle;
	private ParticleData m_burnParticle;
	private bool m_feedbackParticleMatchDirection = false;
	private float m_hitRadius = 0f;

	private Bounds m_bounds;
	private Rect m_rect;
	public Rect boundingRect { get { return m_rect; } }

	private CircleAreaBounds m_area;
	public CircleAreaBounds area { get { return m_area; } }

	Vector3 m_lastBreathDirection;
	public Vector3 lastBreathHitDiretion { get { return m_lastBreathDirection; } }

	private Decoration m_decoration;
	private DragonTier m_lastBurnTier;
	public DragonTier lastBurnTier{ get{ return m_lastBurnTier; } }

	private List<FireNode> m_neighbours;
	private List<float> m_neihboursDistance;

	private bool m_canStartSmoke;

	private float m_timer;

	private GameObject m_fireSprite;
	private GameCamera m_newCamera;

	private State m_state;
	private State m_nextState;


	// Use this for initialization
	void Start() {
		m_bounds = new Bounds(transform.position, Vector3.one * m_hitRadius * 2f);
		m_rect = new Rect((Vector2)transform.position, Vector2.zero);

		m_newCamera = Camera.main.GetComponent<GameCamera>();

		m_area = new CircleAreaBounds(transform.position, m_hitRadius);

		// get two closets neighbours
		FindNeighbours();

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
		m_lastBreathDirection = Vector3.up;

		SetNeighboursDistance();

		m_canStartSmoke = true;

		m_timer = 0f;
	}

	public void Disable() {
		m_state = State.Extinguish;
	}

	public bool IsSpreadingFire() {
		return m_state == State.Spreading;
	}

	public bool IsBurning() {
		return m_state == State.Burning;
	}

	public bool IsExtinguishing() {
		return m_state == State.Extinguish;
	}

	public void Burn(Vector2 _direction, bool _dragonBreath, DragonTier _tier) {
		if (m_state == State.Extinguished) {
			ZoneManager.ZoneEffect effect = InstanceManager.zoneManager.GetFireEffectCode(m_decoration, _tier);
			m_lastBurnTier = _tier;

			if (effect >= ZoneManager.ZoneEffect.M) {
				if (effect == ZoneManager.ZoneEffect.L) {
					m_nextState = State.Extinguish;
				} else {
					m_nextState = State.Spreading;
				}
			} else {
				// Dragon can't burn this thing, so lets put a few feedback particles
				if (_dragonBreath && m_timer <= 0f) {
					GameObject hitParticle = ParticleManager.Spawn(m_feedbackParticle, transform.position);
					if (hitParticle != null && m_feedbackParticleMatchDirection) {
						Vector3 angle = (_direction.x < 0)? Vector3.down : Vector3.up;

						hitParticle.transform.rotation = Quaternion.Euler(angle * 90f);
						m_lastBreathDirection = _direction;
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

		switch (m_state) {
			case State.Extinguished:
				m_timer -= Time.deltaTime;
				if (m_timer <= 0f) {
					m_timer = 0f;
				}
				break;

			case State.Spreading:
				ToogleEffect();

				bool allBurned = true;
				for (int i = 0; i < m_neihboursDistance.Count; i++) {
					if (m_neighbours[i].IsSpreadingFire()) {
						m_neihboursDistance[i] = 0;
					} else {
						allBurned = false;
						if (m_neihboursDistance[i] > 0) {
							m_neihboursDistance[i] -= 0.25f;						
						} else {
							m_neighbours[i].Burn(Vector2.zero, false, m_lastBurnTier);
						}
					}
				}

				if (allBurned) {
					m_nextState = State.Burning;
				}
				break;

			case State.Burning:
				ToogleEffect();
				break;

			case State.Extinguish:
				if (m_timer > 0) {
					m_timer -= Time.deltaTime;
					if (m_timer <= 0) {
						StopFireEffect();
					} else if (m_timer < 3f) {
						StartSmokeEffect();
					}
				}
				break;
		}
	}

	private void ChangeState() {
		switch (m_nextState) {
			case State.Spreading:
				StartFireEffect();
				break;

			case State.Burning:
				break;

			case State.Extinguish:
				if (m_fireSprite != null) {
					m_fireSprite.GetComponentInChildren<Animator>().SetBool("burn", false);
				}
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
		FirePropagationManager.InsertBurning(transform);
		if (m_fireSprite == null) {
			m_fireSprite = ParticleManager.Spawn(m_burnParticle);

			if (m_fireSprite != null) {
				m_fireSprite.GetComponentInChildren<Animator>(false).SetBool("burn", true);
				m_fireSprite.transform.position = transform.position;
				m_fireSprite.transform.localScale = transform.localScale * Random.Range( 0.55f, 1.45f);
				m_fireSprite.transform.localRotation = transform.localRotation;
			}
		}
	}

	private void StopFireEffect() {
		FirePropagationManager.RemoveBurning(transform);
		if (m_fireSprite != null) {
			m_fireSprite.SetActive(false);
			ParticleManager.ReturnInstance(m_fireSprite);
		}
		m_fireSprite = null;
	}

	private void StartSmokeEffect() {
		if (m_canStartSmoke) {
			GameObject smoke = ParticleManager.Spawn("SmokeParticle", transform.position);
			if (smoke != null)
				smoke.GetComponent<DisableInSeconds>().activeTime = 2f;
		}

		m_canStartSmoke = false;
	}

	private void FindNeighbours() {
		m_neighbours = new List<FireNode>();
		m_neihboursDistance = new List<float>();
		FireNode[] nodes = transform.parent.GetComponentsInChildren<FireNode>(true);
		
		for (int i = 0; i < nodes.Length; i++) {
			if (nodes[i] != null && nodes[i] != this) {
				m_neighbours.Add(nodes[i]);
				m_neihboursDistance.Add(0);
			}
		}
	}

	private void SetNeighboursDistance() {
		for (int i = 0; i < m_neighbours.Count; i++) {			
			m_neihboursDistance[i] = Vector3.SqrMagnitude(transform.position - m_neighbours[i].transform.position);
		}
	}


	//------------------------------------------------------------------------------
	void OnDrawGizmosSelected() {
		Gizmos.color = Colors.WithAlpha(Colors.magenta, 0.75f);
		Gizmos.DrawSphere(transform.position, 0.5f);

		Gizmos.color = Colors.fuchsia;
		Gizmos.DrawWireSphere(transform.position, m_hitRadius);
	}
}
