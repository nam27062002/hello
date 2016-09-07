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

	private CircleAreaBounds m_area;
	public CircleAreaBounds area { get { return m_area; } }

	private List<FireNode> m_neighbours;
	private State m_state;

	private float m_resistance;
	private float m_timer;

	private Vector3 m_fireSpriteScale;
	private Vector3 m_fireSpriteDestinationScale;

	private GameObject m_fireSprite;
	private GameCamera m_newCamera;

	private Reward m_reward;

	private bool m_canBurn = false;
	public bool canBurn { get { return m_canBurn; } }

	Vector3 m_lastBreathDirection;
	public Vector3 lastBreathHitDiretion { get { return m_lastBreathDirection; } }

	// Use this for initialization
	void Start () {
		FirePropagationManager.Insert(this);

		m_newCamera = Camera.main.GetComponent<GameCamera>();
		m_reward = new Reward();
		m_reward.coins = 0;
		m_reward.origin = "firenode";

		m_area = new CircleAreaBounds(transform.position, m_hitRadius);

		// get two closets neighbours
		FindNeighbours();

		gameObject.SetActive(false);
	}

	public void Init(int _goldReward, bool _canBurn) {
		m_reward.coins = _goldReward;
		m_canBurn = _canBurn;
		Reset();
		if (!m_canBurn) {
			enabled = false;
		}
	}

	public void Reset() {
		StopFire();

		m_resistance = m_resistanceMax;
		m_state = State.Idle;
		m_lastBreathDirection = Vector3.up;
		m_fireSpriteScale = Vector3.zero;
	}

	public void UpdateLogic() {
		if (m_fireSprite != null)
			m_fireSprite.transform.position = transform.position;
		
		switch(m_state)
		{
			case State.Burning:
			{
				//check if we have to render the particle

				bool isInsideActivationMaxArea = m_newCamera.IsInsideActivationMaxArea(transform.position);

				if (m_fireSprite != null) 
				{
					m_fireSprite.transform.localScale = m_fireSpriteScale;

					if (!isInsideActivationMaxArea) 
					{
						StopFire();
					}
				} 
				else if (isInsideActivationMaxArea) 
				{
					StartFire();
				}

				m_fireSpriteScale = Vector3.Lerp(m_fireSpriteScale, m_fireSpriteDestinationScale, Time.smoothDeltaTime * 1.5f);

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
			case State.Burned:
			{
				if (m_fireSprite != null) {
					m_fireSprite.transform.localScale = Vector3.Lerp(m_fireSprite.transform.localScale, Vector3.zero, Time.smoothDeltaTime);

					if (m_fireSprite.transform.localScale.x < 0.1f) {
						StopFire();
					}
				}
			} break;
		}
	}

	public bool IsBurned() {
		return m_state > State.Damaged;//  && m_timer < m_burningTime * 0.5f;
	}

	public bool IsDamaged() {
		return m_state >= State.Damaged;
	}

	public void Burn(float _damage, Vector2 _direction, bool _dragonBreath) {
		if (m_state == State.Idle || m_state == State.Damaged) {	
			if (_dragonBreath) {
				GameObject hitParticle = ParticleManager.Spawn(m_breathHitParticle, transform.position + Vector3.back * 2);				
				if (hitParticle != null && m_hitParticleMatchDirection) {
					Vector3 angle = new Vector3(0, 90, 0);
					m_lastBreathDirection = _direction;
					if (_direction.x < 0) {
						angle.y *= -1;
					}

					hitParticle.transform.rotation = Quaternion.Euler(angle);
				}
			}

			m_resistance -= _damage;
			m_state = State.Damaged;

			if (m_resistance <= 0) {
				m_state = State.Burning;
				m_timer = m_burningTime;
				Messenger.Broadcast<Transform, Reward>(GameEvents.ENTITY_BURNED, transform, m_reward);
			}
		}
	}

	private void StartFire() {
		FirePropagationManager.InsertBurning(transform);
		if (m_fireSprite == null) {
			m_fireSprite = PoolManager.GetInstance("PF_FireNewProc");

			m_fireSprite.transform.position = transform.position;
			m_fireSprite.transform.localScale = m_fireSpriteScale;
			m_fireSpriteDestinationScale = transform.localScale * Random.Range( 0.55f, 1.45f);
			m_fireSprite.transform.localRotation = transform.localRotation;

			/*if (Random.Range(0,100) > 50) {
				m_fireSprite.transform.Rotate(Vector3.up, 180, Space.Self);
				// Move child!!
				if (m_fireSprite.transform.childCount > 0) {
					Vector3 p = transform.localPosition;
					p.z = -p.z;
					transform.localPosition = p;
				}
			}

			m_fireSprite.GetComponent<Animator>().Play("burn", 0 , Random.Range(1f, 2f));
			*/
		}
	}

	public void InstaBurnForReward() {
		if ( m_state < State.Burning) {
			// Insta reward
			m_state = State.Burned;
			//FirePropagationManager.Remove(this);
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
