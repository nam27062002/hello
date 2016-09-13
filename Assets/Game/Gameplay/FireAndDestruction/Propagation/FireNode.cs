using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireNode : MonoBehaviour, IQuadTreeItem {

	enum State {
		Idle,
		Damaged,
		Burning,
		Burned
	};

	[SerializeField] private string m_breathHitParticle = "PF_FireHit";
	[SerializeField] private bool m_hitParticleMatchDirection = false;
	[SeparatorAttribute]
	[SerializeField] private float m_hitRadius = 0f;
	[SerializeField] private float m_resistanceMax = 25f;
	[SerializeField] private float m_burningTime = 10f;
	[SerializeField] private float m_damagePerSecond = 6f;

	public float burningTime
	{
		get{ return m_burningTime; }
	}


	private Rect m_rect;
	public Rect boundingRect { get { return m_rect; } }

	private CircleAreaBounds m_area;
	public CircleAreaBounds area { get { return m_area; } }

	private List<FireNode> m_neighbours;
	private State m_state;

	private float m_resistance;
	private float m_timer;
	private float m_particleTimer;

	private float m_firePower;
	private float m_firePowerDest;

	private GameObject m_fireSprite;
	private Material m_fireMaterial;
	private GameCamera m_newCamera;

	private Reward m_reward;

	private ZoneManager.ZoneEffect m_zoneEffect;

	Vector3 m_lastBreathDirection;
	public Vector3 lastBreathHitDiretion { get { return m_lastBreathDirection; } }



	// Use this for initialization
	void Start () {
		m_rect = new Rect((Vector2)transform.position, Vector2.zero);

		m_newCamera = Camera.main.GetComponent<GameCamera>();
		m_reward = new Reward();
		m_reward.coins = 0;
		m_reward.origin = "firenode";

		m_area = new CircleAreaBounds(transform.position, m_hitRadius);

		// get two closets neighbours
		FindNeighbours();

		gameObject.SetActive(false);
	}

	public void Init(int _goldReward, ZoneManager.ZoneEffect _effect) {
		m_reward.coins = _goldReward;
		m_zoneEffect = _effect;
		Reset();

		FirePropagationManager.Insert(this);
	}

	public void Reset() {
		StopFire();

		m_resistance = m_resistanceMax;
		m_state = State.Idle;
		m_lastBreathDirection = Vector3.up;
		m_firePower = 0f;
		m_firePowerDest = 0f;

		m_particleTimer = 0f;
	}

	public void UpdateLogic() {
		if (m_fireSprite != null)
			m_fireSprite.transform.position = transform.position;
		
		switch(m_state) {
			case State.Burning: {
				//check if we have to render the particle
				bool isInsideActivationMaxArea = m_newCamera.IsInsideActivationMaxArea(transform.position);

				if (m_fireSprite != null) {
					m_fireMaterial.SetFloat("_Power", m_firePower);

					if (!isInsideActivationMaxArea) {
						StopFire();
					}
				} else if (isInsideActivationMaxArea) {
					StartFire();
				}

				m_firePower = Mathf.Lerp(m_firePower, m_firePowerDest, Time.smoothDeltaTime * 1.5f);

				//burn near nodes and fuel them
				if (m_timer > 0) {
					m_timer -= Time.deltaTime;
					for (int i = 0; i < m_neighbours.Count; i++) {
							m_neighbours[i].Burn(m_damagePerSecond * Time.deltaTime, Vector2.zero, false); // what amount of damage should
					}
				} else {
					m_timer = 5;
					StartSmoke(m_timer);

					m_state = State.Burned;
				}
			} break;

			case State.Burned: {
				if (m_fireSprite != null) {
					m_firePower = Mathf.Lerp(m_firePower, 0f, Time.smoothDeltaTime);
					m_fireMaterial.SetFloat("_Power", m_firePower);

					if (m_fireSprite.transform.localScale.x < 0.1f) {
						StopFire();
					}
				}
			} break;

			default:
				m_particleTimer -= Time.deltaTime;
				if (m_particleTimer <= 0f) {
					m_particleTimer = 0f;
				}
				break;
		}
	}

	public bool IsBurned() {
		return m_state > State.Damaged; //&& m_timer < m_burningTime * 0.5f;
	}

	public bool IsDamaged() {
		return m_state >= State.Damaged;
	}

	public void Burn(float _damage, Vector2 _direction, bool _dragonBreath) {
		if (m_state == State.Idle || m_state == State.Damaged) {	
			if (_dragonBreath) {
				if (m_particleTimer <= 0f) {
					GameObject hitParticle = ParticleManager.Spawn(m_breathHitParticle, transform.position + Vector3.back * 2);				
					if (hitParticle != null && m_hitParticleMatchDirection) {
						Vector3 angle = new Vector3(0, 90, 0);
						m_lastBreathDirection = _direction;
						if (_direction.x < 0) {
							angle.y *= -1;
						}

						hitParticle.transform.rotation = Quaternion.Euler(angle);
					}
					m_particleTimer = 0.5f;
				}
			}

			if (m_zoneEffect >= ZoneManager.ZoneEffect.M) {
				// The M effect burns the house
				m_resistance -= _damage;
				m_state = State.Damaged;

				if (m_resistance <= 0) {
					m_state = State.Burning;
					m_timer = m_burningTime;
					Messenger.Broadcast<Transform, Reward>(GameEvents.ENTITY_BURNED, transform, m_reward);
				}
			}
		}
	}

	private void StartFire() {
		FirePropagationManager.InsertBurning(transform);
		if (m_fireSprite == null) {
			m_fireSprite = PoolManager.GetInstance("PF_FireNewProc");

			m_fireSprite.transform.position = transform.position;
			m_fireSprite.transform.localScale = transform.localScale * Random.Range( 0.55f, 1.45f);
			m_fireSprite.transform.localRotation = transform.localRotation;

			Renderer fireSpr = m_fireSprite.GetFirstComponentInChildren<Renderer>();
			m_fireMaterial = fireSpr.material;
			m_fireMaterial.SetFloat("_Seed", Random.Range(0f, 1f));
			m_fireMaterial.SetFloat("_Power", 0f);

			m_firePower = 0f;
			m_firePowerDest = 4.8f;
		}
	}

	public void InstaBurnForReward() {
		if ( m_state < State.Burning) {
			// Insta reward
			m_state = State.Burned;
			Messenger.Broadcast<Transform, Reward>(GameEvents.ENTITY_BURNED, transform, m_reward);
		}
	}

	private void StopFire() {
		FirePropagationManager.RemoveBurning( transform );		
		if (m_fireSprite != null) {
			m_fireSprite.SetActive(false);
			PoolManager.ReturnInstance( m_fireSprite );
		}
		m_fireSprite = null;
		m_fireMaterial = null;
	}

	public void StartSmoke(float _time) {
		GameObject smoke = PoolManager.GetInstance("SmokeParticle");
		smoke.transform.position = transform.position;
		smoke.GetComponent<DisableInSeconds>().activeTime = _time;
		smoke.GetComponent<ParticleSystem>().Play();
	}

	private void FindNeighbours() {
		m_neighbours = new List<FireNode>();
		FireNode[] nodes = transform.parent.GetComponentsInChildren<FireNode>(true);
		
		for (int i = 0; i < nodes.Length; i++) {
			if (nodes[i] != null && nodes[i] != this) {
				m_neighbours.Add(nodes[i]);
			}
		}
	}

	void OnDrawGizmosSelected() {
		Gizmos.color = Colors.WithAlpha(Colors.magenta, 0.75f);
		Gizmos.DrawSphere(transform.position, 0.5f);

		Gizmos.color = Colors.fuchsia;
		Gizmos.DrawWireSphere(transform.position, m_hitRadius);
	}
}
