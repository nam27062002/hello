using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SensePlayer))]
public class EvadeBehaviour : Initializable {

	private DragonMotion m_dragon;
	private Transform m_dragonMouth;
			
	private PreyMotion m_motion;
	private Animator m_animator;


	// Use this for initialization
	void Awake () {
		m_motion = GetComponent<PreyMotion>();
	}

	void Start() {
		m_dragon = InstanceManager.player.GetComponent<DragonMotion>();
		m_dragonMouth = m_dragon.tongue;
	}

	public override void Initialize() {
	}

	// Update is called once per frame
	void FixedUpdate() {
		m_motion.Evade(m_dragonMouth.position, m_dragon.GetVelocity(), m_dragon.GetMaxSpeed());
	}
}
