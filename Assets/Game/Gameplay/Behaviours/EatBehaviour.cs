﻿using UnityEngine;
using System.Collections.Generic;

public abstract class EatBehaviour : MonoBehaviour {
	struct PreyData {		
		public float absorbTimer;
		public float eatingAnimationTimer;
		public Transform startParent;
		public Vector3 startScale;
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

	protected DragonTier m_tier;
	protected float m_biteSkill;

	private float m_eatingTimer;
	private float m_eatingTime;
	protected bool m_slowedDown;

	protected Transform m_mouth;
	protected Vector3 m_tongueDirection;
	protected Animator m_animator;

	protected MotionInterface m_motion;

	private List<GameObject> m_bloodEmitter;

	bool m_almostEat = false;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Awake () {
		m_eatingTimer = 0;
		m_animator = transform.FindChild("view").GetComponent<Animator>();

		m_prey = new List<PreyData>();
		m_bloodEmitter = new List<GameObject>();

		m_slowedDown = false;
	}

	void OnDisable() {
		m_eatingTimer = 0;
		if (m_slowedDown) {
			SlowDown(false);
		}

		for (int i = 0; i < m_prey.Count; i++) {	
			if (m_prey[i].prey != null) {
				PreyData prey = m_prey[i];
				prey.prey.transform.parent = prey.startParent;
				Swallow(m_prey[i].prey);
				prey.prey = null;
				prey.startParent = null;
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
		if (m_eatingTimer <= 0) {
			FindSomethingToEat();
		} else {
			m_eatingTimer -= Time.deltaTime;
			if (m_eatingTimer <= 0) {
				m_eatingTimer = 0;
			}
		}

		if (m_prey.Count > 0) {	
			Chew();
		}

		UpdateBlood();

		m_animator.SetBool("almostEat", m_almostEat);
		m_almostEat = false;
	}

	public void AlmostEat(EdibleBehaviour _prey) {
		m_almostEat = true;
	}

	private void Eat(EdibleBehaviour _prey, float _biteResistance) {
		_prey.OnEat();

		// Yes!! Eat it!!
		m_eatingTimer = m_eatingTime = (m_biteSkill * _biteResistance);

		if (m_eatingTime >= 0.5f) {
			SlowDown(true);
		}

		PreyData preyData = new PreyData();

		preyData.startParent = _prey.transform.parent;
		_prey.transform.parent = transform;
		preyData.startScale = _prey.transform.localScale;
		preyData.absorbTimer = m_absorbTime;
		preyData.eatingAnimationTimer = Mathf.Max(m_minEatAnimTime, m_eatingTimer);
		preyData.prey = _prey;

		m_prey.Add(preyData);

		m_animator.SetBool("eat", true);

		if (m_eatingTime >= 0.5f || m_prey.Count > 2) {
			m_animator.SetTrigger("eat crazy");
		}

		Vector3 bloodPos = m_mouth.position;
		bloodPos.z = -50f;
		m_bloodEmitter.Add(ParticleManager.Spawn("bloodchurn-large", bloodPos));
	}

	private void Swallow(EdibleBehaviour _prey) {
		_prey.OnSwallow();
	}

	private void FindSomethingToEat() {
		Entity[] preys = EntityManager.instance.GetEntitiesInRange2D(m_mouth.position, m_eatDistance);

		for (int e = 0; e < preys.Length; e++) {
			if (preys[e].def.edibleFromTier <= m_tier) {
				Entity entity = preys[e];
				Vector3 heading = entity.transform.position - m_mouth.position;
				float dot = Vector3.Dot(heading.normalized, m_motion.direction);

				// check distance to dragon mouth
				if (dot > 0) {
					Eat(entity.GetComponent<EdibleBehaviour>(), entity.def.biteResistance);
					return;

					/*
					float distanceSqr = m_Bounds.DistanceSqr( m_dragonMouth.transform.position );
					if (distanceSqr <= m_dragonEat.eatDistanceSqr) { 
						Eat(entity, entity.def.biteResistance);
					}
					*/
				}
			}
		}
	}

	private void Chew() {
		bool empty = true;
		for (int i = 0; i < m_prey.Count; i++) {
			if (m_prey[i].prey != null) {
				PreyData prey = m_prey[i];

				prey.absorbTimer -= Time.deltaTime;
				prey.eatingAnimationTimer -= Time.deltaTime;

				float t = 1 - Mathf.Max(0, m_prey[i].absorbTimer / m_absorbTime);

				// swallow entity
				prey.prey.transform.position = Vector3.Lerp(prey.prey.transform.position, m_mouth.position, t);
				prey.prey.transform.localScale = Vector3.Lerp(prey.prey.transform.localScale, prey.startScale * 0.75f, t);
				prey.prey.transform.rotation = Quaternion.Lerp(prey.prey.transform.rotation, Quaternion.AngleAxis(-90f, m_tongueDirection), 0.25f);

				// remaining time eating
				if (m_prey[i].eatingAnimationTimer <= 0) 
				{
					prey.prey.transform.parent = prey.startParent;
					Swallow(prey.prey);
					prey.prey = null;
					prey.startParent = null;
				}

				m_prey[i] = prey;
				empty = false;
			}
		}

		if (empty) {
			m_prey.Clear();

			if (m_slowedDown) {
				SlowDown(false);
			}

			m_animator.SetBool("eat", false);
		}
	}

	private void UpdateBlood() {
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

	protected abstract void SlowDown(bool _enable);
}
