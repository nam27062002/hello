using UnityEngine;
using System.Collections;

public class DragonEatBehaviour : MonoBehaviour {


	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------	
	private float m_absorbTimer;
	[SerializeField]private float m_absorbTime;

	private float m_eatingTimer;
	private float m_eatingTime;

	private Transform m_mouth;
	private Transform m_head;
	private Animator m_animator;
	private DragonPlayer m_dragon;

	private EdibleBehaviour m_prey;
	
	private GameObject m_bloodEmitter;

	
	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Start () {
	
		m_eatingTimer = 0;

		m_mouth = transform.FindSubObjectTransform("eat");
		m_head = transform.FindSubObjectTransform("head");

		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_dragon = GetComponent<DragonPlayer>();

		m_prey = null;
		m_bloodEmitter = null;
	}

	void OnDisable() {
		if (m_animator != null) {
			m_animator.SetBool("big_prey", false);
			m_animator.SetBool("bite", false);
		}

		m_prey = null;
	}

	public bool IsEating() {

		return enabled && m_eatingTimer > 0;
	}

	// Update is called once per frame
	void Update() {
	
		if (m_eatingTimer > 0) {
			m_eatingTimer -= Time.deltaTime;
			m_absorbTimer -= Time.deltaTime;

			float t = 1 - Mathf.Max(0, m_absorbTimer / m_absorbTime);

			// swallow entity
			Vector3 playerMouthDir = (m_head.position - m_mouth.position);

			float d = playerMouthDir.magnitude;
			playerMouthDir.Normalize();

			Vector3 targetPosition = m_mouth.position;// + playerMouthDir * d * 0.5f;
			targetPosition.z = 50f;

			m_prey.transform.position = Vector3.Lerp(m_prey.transform.position, targetPosition, t);
			m_prey.transform.rotation = Quaternion.Lerp(m_prey.transform.rotation, Quaternion.AngleAxis(-90f, playerMouthDir), t);

			if (m_bloodEmitter != null) {
				m_bloodEmitter.transform.position =  m_mouth.position;
			}

			// remaining time eating
			if (m_eatingTimer < 0) {
				m_eatingTimer = 0;

				m_prey.OnSwallow();
				m_prey = null;

				m_animator.SetBool("big_prey", false);
				m_animator.SetBool("bite", false);
			}
		}
	}

	void OnTriggerStay(Collider _other) {

		if (enabled && m_eatingTimer <= 0) {
			// Can object be eaten?
			m_prey = _other.gameObject.GetComponent<EdibleBehaviour>();

			if (m_prey != null && m_prey.edibleFromTier <= m_dragon.data.tier) {
				// Yes!! Eat it!
				m_absorbTimer = m_absorbTime;
				m_eatingTimer = m_eatingTime = (m_dragon.data.bite.value * m_prey.size) / m_dragon.GetSpeedMultiplier(); // (  time  ) / speedMultiplier

				Reward reward = m_prey.Eat(m_eatingTime);
				m_dragon.AddLife(reward.health);
				m_dragon.AddFury(reward.fury);

				m_animator.SetBool("big_prey", m_prey.isBig);
				m_animator.SetBool("bite", true);

				// spawn blood particle TEMP - use some kind of particle manager
				m_bloodEmitter = InstanceManager.particles.Spaw("bloodchurn-large", m_mouth.position);
				//m_bloodEmitter.GetComponent<Renderer>().sortingLayerName = "enemies";
			}
		}
	}
}