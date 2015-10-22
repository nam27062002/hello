using UnityEngine;
using System.Collections;

public class DragonAttackBehaviour : MonoBehaviour {

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	[SerializeField] private float m_attackDelay = 0.25f;
	[SerializeField] private float m_damage = 10f;

	private DragonPlayer m_dragon;
	private DragonMotion m_motion;
	private Animator m_animator;
	
	private HittableBehaviour m_target;

	private bool m_isAttacking;
	private float m_attackTimer;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	void Start () {		
		m_animator = transform.FindChild("view").GetComponent<Animator>();
		m_dragon = GetComponent<DragonPlayer>();
		m_motion = GetComponent<DragonMotion>();

		m_target = null;

		m_attackTimer = 0;
	}

	void OnEnable() {

	}
	
	// Update is called once per frame
	void Update () {
		if (m_isAttacking) {
			if (m_animator.GetCurrentAnimatorStateInfo(0).fullPathHash == Animator.StringToHash("BaseLayer.idle")
		    ||  m_animator.GetBool("fly")) {
				m_isAttacking = false;
				m_motion.enabled = true;
				m_attackTimer = m_attackDelay;
			}
		} else {
			if (m_attackTimer > 0) {
				m_attackTimer -= Time.deltaTime;
				if (m_attackTimer <= 0) {
					m_attackTimer = 0;
				}
			}
		}
	}

	void OnCollisionStay(Collision collision) {
		// play hit animation
		if (m_attackTimer <= 0 && !m_isAttacking) {
			m_target = collision.gameObject.GetComponent<HittableBehaviour>();
			if (m_target != null) {
				m_animator.SetTrigger("attack");
				m_isAttacking = true;
				m_motion.enabled = false;
			}
		}
	}

	void OnCollisionExit(Collision collision) {

		if (m_target) {
			if (m_target.gameObject == collision.gameObject) {
				m_target = null;
			}
		}
	}

	public void OnAttack() {
		
		if (m_target != null) {
			float damage = m_dragon.GetSpeedMultiplier();
			if (damage > 1) {
				damage *= m_damage;
			}
			m_target.OnHit(damage);
		}
	}
}