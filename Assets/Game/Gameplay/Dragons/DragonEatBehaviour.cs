using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragonEatBehaviour : MonoBehaviour {
		
	struct PreyData {		
		public float absorbTimer;
		public float eatingAnimationTimer;
		public Vector3 absorbPosition;
		public EdibleBehaviour prey;
	};

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------	

	[SerializeField]private float m_absorbTime;
	[SerializeField]private float m_minEatAnimTime;
	[SerializeField]private float m_eatDistance;
	public float eatDistanceSqr { get { return m_eatDistance * m_eatDistance; } }

	private List<PreyData> m_prey;// each prey that falls near the mouth while running the eat animation, will be swallowed at the same time

	private float m_eatingTimer;
	private float m_eatingTime;
	private bool m_slowedDown;

	private Transform m_mouth;
	private Vector3 m_tongueDirection;
	private Animator m_animator;
	private DragonPlayer m_dragon;
	private DragonBoostBehaviour m_dragonBoost;
			
	private List<GameObject> m_bloodEmitter;

	
	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Awake () {
	
		m_eatingTimer = 0;

		m_mouth = GetComponent<DragonMotion>().tongue;
		m_tongueDirection = GetComponent<DragonMotion>().tongue.position - GetComponent<DragonMotion>().head.position;
		m_tongueDirection.Normalize();

		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_dragon = GetComponent<DragonPlayer>();
		m_dragonBoost = GetComponent<DragonBoostBehaviour>();

		m_prey = new List<PreyData>();
		m_bloodEmitter = new List<GameObject>();

		m_slowedDown = false;
	}

	void OnDisable() {

		m_eatingTimer = 0;
		if (m_slowedDown) {
			m_dragon.SetSpeedMultiplier(1f);
			m_dragonBoost.ResumeBoost();
			m_slowedDown = false;
		}

		for (int i = 0; i < m_prey.Count; i++) {			
			if (m_prey[i].prey != null) {
				Swallow(m_prey[i].prey);
			}
		}
		
		m_prey.Clear();

		if (m_animator && m_animator.isInitialized) {
			m_animator.SetBool("eat", false);
		}
	}

	public bool IsEating() {

		return enabled && m_prey.Count > 0;
	}

	// Update is called once per frame
	void Update() {
			
		if (enabled && m_prey.Count > 0) {

			m_eatingTimer -= Time.deltaTime;
			if (m_eatingTimer <= 0) {
				m_eatingTimer = 0;
			}

			bool empty = true;
			for (int i = 0; i < m_prey.Count; i++) {

				if (m_prey[i].prey != null) {
					PreyData prey = m_prey[i];

					prey.absorbTimer -= Time.deltaTime;
					prey.eatingAnimationTimer -= Time.deltaTime;
					
					float t = 1 - Mathf.Max(0, m_prey[i].absorbTimer / m_absorbTime);
					
					// swallow entity
					prey.prey.transform.position = Vector3.Lerp(prey.absorbPosition, m_mouth.position, t);
					prey.prey.transform.rotation = Quaternion.Lerp(prey.prey.transform.rotation, Quaternion.AngleAxis(-90f, m_tongueDirection), 0.25f);
										
					// remaining time eating
					if (m_prey[i].eatingAnimationTimer < 0) {
						Swallow(prey.prey);
						prey.prey = null;
					}

					m_prey[i] = prey;
					empty = false;
				}
			}

			if (empty) {
				m_prey.Clear();

				if (m_slowedDown) {
					m_dragon.SetSpeedMultiplier(1f);
					m_dragonBoost.ResumeBoost();
					m_slowedDown = false;
				}

				m_animator.SetBool("eat", false);
			}
		}

		if (m_bloodEmitter.Count > 0) {
			bool empty = true;
			Vector3 bloodPos = m_mouth.position;
			bloodPos.z = -1f;

			for (int i = 0; i < m_bloodEmitter.Count; i++) {
				if (m_bloodEmitter[i] != null && m_bloodEmitter[i].activeInHierarchy) {
					m_bloodEmitter[i].transform.position = bloodPos;
					empty = false;
				} else {
					m_bloodEmitter[i] = null;
				}
			}

			if (empty) {
				m_bloodEmitter.Clear();
			}
		}
	}

	public bool Eat(EdibleBehaviour _prey) {
		if (enabled && m_eatingTimer <= 0) {
			if (_prey.edibleFromTier <= m_dragon.data.tier) {
				// Yes!! Eat it!!
				m_eatingTimer = m_eatingTime = (m_dragon.data.bite.value * _prey.size);

				if (m_eatingTime >= 0.5f) {
					m_dragonBoost.StopBoost();
					m_dragon.SetSpeedMultiplier(0.25f);
					m_slowedDown = true;
				}

				PreyData preyData = new PreyData();

				preyData.absorbTimer = m_absorbTime;
				preyData.eatingAnimationTimer = Mathf.Max(m_minEatAnimTime, m_eatingTimer);
				preyData.absorbPosition = _prey.transform.position;
				preyData.prey = _prey;

				m_prey.Add(preyData);
					
				m_animator.SetBool("eat", true);

				if (m_eatingTime >= 0.5f || m_prey.Count > 2) {
					m_animator.SetTrigger("eat crazy");
				}

				Vector3 bloodPos = m_mouth.position;
				bloodPos.z = -50f;
				m_bloodEmitter.Add(ParticleManager.Spawn("bloodchurn-large", bloodPos));

				return true;
			}
		}

		return false;
	}

	private void Swallow(EdibleBehaviour _prey) {

		Reward reward = _prey.OnSwallow(m_eatingTime);
		m_dragon.AddLife(reward.health);
		m_dragon.AddFury(reward.fury);
	}
}