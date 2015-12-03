using UnityEngine;

public class PreyAnimationEvents : MonoBehaviour {

	public delegate void OnAttackStartDelegate();
	public delegate void OnAttackDealDamageDelegate();
	public delegate void OnAttackEndDelegate();

	public event OnAttackStartDelegate 		onAttackStart;
	public event OnAttackDealDamageDelegate onAttackDealDamage;
	public event OnAttackEndDelegate 		onAttackEnd;


	// ------------------------------------------------------- //

	public void AttackStart() {
		if (onAttackStart != null)
			onAttackStart();
	}

	public void AttackDealDamage() {
		if (onAttackDealDamage != null)
			onAttackDealDamage();
	}

	public void AttackEnd() {
		if (onAttackEnd != null)
			onAttackEnd();
	}
}
