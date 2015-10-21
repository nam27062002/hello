using UnityEngine;

public class DragonAnimationEvents : MonoBehaviour {

	private DragonAttackBehaviour m_attackBehaviour;

	void Start() {
		m_attackBehaviour = transform.parent.GetComponent<DragonAttackBehaviour>();
	}

	public void OnAttackEvent() {
		m_attackBehaviour.OnAttack();
	}
}
