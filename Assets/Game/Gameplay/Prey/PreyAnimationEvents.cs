using UnityEngine;

public class PreyAnimationEvents : MonoBehaviour {

	public delegate void Attack_Start();
	public delegate void Attack_DealDamage();
	public delegate void Attack_End();

	public event Attack_Start 		onAttackStart;
	public event Attack_DealDamage 	onAttackDealDamage;
	public event Attack_End 		onAttackEnd;


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
