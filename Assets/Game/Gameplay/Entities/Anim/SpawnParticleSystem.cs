using UnityEngine;

public class SpawnParticleSystem : StateMachineBehaviour {

	[SerializeField] private ParticleData m_particleSystemData;
	[SerializeField] private float m_delay = 0f;
	[SerializeField] private bool m_attach = true;

	private float m_timer = 0f;

	// Use this for initialization
	void Start () {	
		m_particleSystemData.CreatePool();
		m_timer = 0f;
	}
	
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (m_delay > 0f) {
			m_timer = m_delay;
		} else {
			SpawnParticle(animator.transform);
		}
	}

	public override void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		if (m_timer > 0f) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0f) {
				SpawnParticle(animator.transform);
			}
		}
	}

	private void SpawnParticle(Transform _parent) {		
		if (m_attach)	m_particleSystemData.Spawn(_parent);
		else			m_particleSystemData.Spawn();
	}
}
