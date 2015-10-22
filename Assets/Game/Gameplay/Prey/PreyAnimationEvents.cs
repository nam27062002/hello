using UnityEngine;

public class PreyAnimationEvents : MonoBehaviour {

	private PreyBehaviour m_prey;

	void Start() {
		m_prey = transform.parent.GetComponent<PreyBehaviour>();
	}

	public void OnAttackEvent() {
		m_prey.OnAttack();
	}
}
