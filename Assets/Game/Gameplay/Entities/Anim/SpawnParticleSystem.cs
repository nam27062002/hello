using UnityEngine;

public class SpawnParticleSystem : StateMachineBehaviour {

	[SerializeField] private ParticleData m_particleSystemData;
	[SerializeField] private bool m_attach = true;

	Transform m_Transform;

	// Use this for initialization
	void Start () {		
		ParticleManager.CreatePool(m_particleSystemData);
	}
	
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
		Transform transform = animator.transform;
		GameObject ps = ParticleManager.Spawn(m_particleSystemData, transform.position + m_particleSystemData.offset);
		if (m_attach && ps != null) {
			ps.transform.parent = transform;
			ps.transform.localRotation = Quaternion.identity;
		}
	}
}
