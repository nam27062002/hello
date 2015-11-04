using UnityEngine;

public class PreyAnimationEvents : MonoBehaviour {

	private AttackBehaviour m_attackBehaviour;

	void Start() {
		m_attackBehaviour = transform.parent.GetComponent<AttackBehaviour>();
	}

	public void OnAttackEvent() {
		m_attackBehaviour.OnAttack();
	}
}
