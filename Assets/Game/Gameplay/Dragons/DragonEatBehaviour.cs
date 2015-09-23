using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragonEatBehaviour : MonoBehaviour {


	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------	

	[SerializeField]private float m_absorbTime;
	[SerializeField]private float m_minEatAnimTime;

	private List<float> m_absorbTimer;
	private List<float> m_eatingAnimationTimer; 
	private List<EdibleBehaviour> m_prey;// each prey that falls near the mouth while running the eat animation, will be swallowed at the same time

	private float m_eatingTimer;
	private float m_eatingTime;


	private Transform m_mouth;
	private Transform m_tongue;
	private Animator m_animator;
	private DragonPlayer m_dragon;
			
	private GameObject m_bloodEmitter;

	
	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Start () {
	
		m_eatingTimer = 0;

		m_mouth = transform.FindSubObjectTransform("fire");
		m_tongue = transform.FindSubObjectTransform("tongue_02");

		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_dragon = GetComponent<DragonPlayer>();

		m_prey = new List<EdibleBehaviour>();
		m_absorbTimer = new List<float>();
		m_eatingAnimationTimer = new List<float>();

		m_bloodEmitter = null;
	}

	void OnDisable() {

		m_eatingTimer = 0;

		for (int i = 0; i < m_prey.Count; i++) {			
			if (m_prey[i] != null) {
				Swallow(m_prey[i]);
			}
		}
		
		m_prey.Clear();
		m_absorbTimer.Clear();
		m_eatingAnimationTimer.Clear();

		m_animator.SetBool("big_prey", false);
		m_animator.SetBool("bite", false);
	}

	public bool IsEating() {

		return enabled && m_eatingTimer > 0;
	}

	// Update is called once per frame
	void Update() {
			
		if (enabled && m_prey.Count > 0) {

			m_eatingTimer -= Time.deltaTime;
			if (m_eatingTimer <= 0) {
				m_eatingTimer = 0;
			}

			Vector3 playerMouthDir = (m_tongue.position - m_mouth.position);
			float d = playerMouthDir.magnitude;
			playerMouthDir.Normalize();

			bool empty = true;
			for (int i = 0; i < m_prey.Count; i++) {

				if (m_prey[i] != null) {

					m_absorbTimer[i] -= Time.deltaTime;
					m_eatingAnimationTimer[i] -= Time.deltaTime;
					
					float t = 1 - Mathf.Max(0, m_absorbTimer[i] / m_absorbTime);
					
					// swallow entity
					Bounds bounds = m_prey[i].GetComponent<Collider>().bounds;
					Vector3 targetPosition = m_mouth.position + (m_prey[i].transform.position - bounds.center) + playerMouthDir * d * 0.5f;
					
					m_prey[i].transform.position = Vector3.Lerp(m_prey[i].transform.position, targetPosition, t);
					m_prey[i].transform.rotation = Quaternion.Lerp(m_prey[i].transform.rotation, Quaternion.AngleAxis(-90f, playerMouthDir), 0.25f);
					
					// remaining time eating
					if (m_eatingAnimationTimer[i] < 0) {
						Swallow(m_prey[i]);
						m_prey[i] = null;
					}

					empty = false;
				}
			}

			if (empty) {
				m_prey.Clear();
				m_absorbTimer.Clear();
				m_eatingAnimationTimer.Clear();
				
				m_animator.SetBool("big_prey", false);
				m_animator.SetBool("bite", false);
			}
		}

		if (m_bloodEmitter != null && m_bloodEmitter.activeInHierarchy) {
			Vector3 bloodPos = m_mouth.position;
			bloodPos.z = -50f;
			m_bloodEmitter.transform.position = bloodPos;
		} else {
			m_bloodEmitter = null;
		}		
	}

	void OnTriggerStay(Collider _other) {

		if (enabled && m_eatingTimer <= 0) {
			// Can object be eaten?
			EdibleBehaviour prey = _other.gameObject.GetComponent<EdibleBehaviour>();

			if (prey != null && prey.edibleFromTier <= m_dragon.data.tier) {

				// Yes!! Eat it!!
				float speed = (m_dragon.GetSpeedMultiplier() > 1)? 1.5f : 1f;
				m_eatingTimer = m_eatingTime = (m_dragon.data.bite.value * prey.size) / speed;

				m_prey.Add(prey);
				m_absorbTimer.Add(m_absorbTime);
				m_eatingAnimationTimer.Add(Mathf.Max(m_minEatAnimTime, m_eatingTimer));

				m_animator.SetBool("bite", true);
				m_animator.SetBool("big_prey", m_prey.Count > 2);

				if (m_bloodEmitter == null) {
					Vector3 bloodPos = m_mouth.position;
					bloodPos.z = -50f;
					m_bloodEmitter = InstanceManager.particles.Spaw("bloodchurn-large", bloodPos);
				}
			}
		}
	}

	private void Swallow(EdibleBehaviour _prey) {

		Reward reward = _prey.Eat(m_eatingTime);
		m_dragon.AddLife(reward.health);
		m_dragon.AddFury(reward.fury);

		_prey.OnSwallow();
	}
}