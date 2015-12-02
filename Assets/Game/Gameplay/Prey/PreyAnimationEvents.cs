using UnityEngine;

public class PreyAnimationEvents : MonoBehaviour {

	public delegate void OnAttackStartDelegate();
	public delegate void OnAttackDealDamageDelegate();
	public delegate void OnAttackEndDelegate();

	public OnAttackStartDelegate 		onAttackStart		= delegate() { };
	public OnAttackDealDamageDelegate 	onAttackDealDamage 	= delegate() { };
	public OnAttackEndDelegate 			onAttackEnd 		= delegate() { };


	// ------------------------------------------------------- //

	public void AttackStart() {
		onAttackStart();
	}

	public void AttackDealDamage() {
		onAttackDealDamage();
	}

	public void AttackEnd() {
		onAttackEnd();
	}
}
