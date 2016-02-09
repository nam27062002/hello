using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireNode : MonoBehaviour {

	enum State {
		Idle,
		Damaged,
		Burning,
		Burned
	};

	[SerializeField] private float m_resistanceMax = 25f;
	[SerializeField] private float m_burningTime = 10f;
	[SerializeField] private float m_damagePerSecond = 6f;


	private List<FireNode> m_neighbours;
	private State m_state;

	private int m_goldReward;
	private float m_resistance;
	private float m_timer;

	private Vector3 m_fireSpriteScale;
	private Vector3 m_fireSpriteDestinationScale;

	private GameObject m_fireSprite;
	private GameCameraController m_camera;

	private ParticleSystem m_smoke;


	// Use this for initialization
	void Start () {
		m_camera = GameObject.Find("PF_GameCamera").GetComponent<GameCameraController>();

		// get two closets neighbours
		FindNeighbours();
	}

	public void Init(int _goldReward) {
		m_goldReward = _goldReward;
		
		Reset();
	}

	public void Reset() {
		StopFire();
		StopSmoke();
		FirePropagationManager.Insert(transform);

		m_resistance = m_resistanceMax;
		m_state = State.Idle;

		m_fireSpriteScale = Vector3.zero;
	}

	void Update() {
		switch(m_state)
		{
			case State.Burning:
			{
				//check if we have to render the particle
				if (m_fireSprite != null) {
					m_fireSprite.transform.localScale = m_fireSpriteScale;
					if (!m_camera.IsInsideActivationMaxArea(transform.position)) {
						StopFire();
					}
				} else if (m_camera.IsInsideActivationMaxArea(transform.position)) {
					StartFire();		
				}

				m_fireSpriteScale = Vector3.Lerp(m_fireSpriteScale, m_fireSpriteDestinationScale, Time.smoothDeltaTime * 1.5f);

				//burn near nodes and fuel them
				if (m_timer > 0) {
					m_timer -= Time.deltaTime;
					for (int i = 0; i < m_neighbours.Count; i++) {
						m_neighbours[i].Burn(m_damagePerSecond * Time.deltaTime); // what amount of damage should
					}
				} else {
					m_timer = 5;
					m_state = State.Burned;
					StartSmoke();
				}
			}break;
			case State.Burned:
			{
				if (m_fireSprite != null) {
					m_fireSprite.transform.localScale = Vector3.Lerp(m_fireSprite.transform.localScale, Vector3.zero, Time.smoothDeltaTime);

					if (m_fireSprite.transform.localScale.x < 0.1f) {
						StopFire();
					}
				}

				if (m_smoke != null)
				{
					m_timer -= Time.deltaTime;
					if ( m_timer < 0 )
						StopSmoke();
				}


			}break;
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

				Reward reward = new Reward();
				reward.coins = m_goldReward;
				
				FirePropagationManager.Remove(transform);

				Messenger.Broadcast<Transform, Reward>(GameEvents.ENTITY_BURNED, transform, reward);
			}
		}
	}

	private void StartFire() {
		if (m_fireSprite == null) {
			m_fireSprite = PoolManager.GetInstance("FireSprite");

			m_fireSprite.transform.position = transform.position;
			m_fireSprite.transform.localScale = m_fireSpriteScale;
			m_fireSpriteDestinationScale = transform.localScale * Random.Range( 0.55f, 1.45f);
			m_fireSprite.transform.localRotation = transform.localRotation;

			if ( Random.Range(0,100) > 50 )
			{
				m_fireSprite.transform.Rotate(Vector3.up, 180, Space.Self);
				// Move child!!
				Transform child_0 = m_fireSprite.transform.GetChild(0);
				if ( child_0 )
				{
					Vector3 p = transform.localPosition;
					p.z = -p.z;
					transform.localPosition = p;
				}
			}

			m_fireSprite.GetComponent<Animator>().Play("burn", 0 , Random.Range(1f, 2f));
		}
	}

	private void StopFire() {		
		if (m_fireSprite != null) {
			m_fireSprite.SetActive(false);
		}
		m_fireSprite = null;
	}

	public void StartSmoke()
	{
		if ( m_smoke == null )
		{
			GameObject smoke = PoolManager.GetInstance("SmokeParticle");
			smoke.transform.position = transform.position;
			m_smoke = smoke.GetComponent<ParticleSystem>();
			m_smoke.Play();
		}
	}

	private void StopSmoke()
	{
		if ( m_smoke != null )
		{
			m_smoke.Stop();
			StartCoroutine( WaitEndSmokeToDeactivate( m_smoke ));
		}
		m_smoke = null;
	}

	IEnumerator WaitEndSmokeToDeactivate( ParticleSystem ps )
	{
		while( ps.particleCount > 0 )
		{
			yield return null;
		}
		ps.gameObject.SetActive(false);
		yield return null;
	}

	private void FindNeighbours() {
		m_neighbours = new List<FireNode>();
		FireNode[] nodes = transform.parent.GetComponentsInChildren<FireNode>();
		
		for (int i = 0; i < nodes.Length; i++) {
			if (nodes[i] != null && nodes[i] != this) {
				m_neighbours.Add(nodes[i]);
			}
		}
	}
}
