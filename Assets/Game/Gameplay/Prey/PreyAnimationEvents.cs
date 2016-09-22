using UnityEngine;

public class PreyAnimationEvents : MonoBehaviour {

	public delegate void OnAttachprojectile();
	public delegate void OnAttackStartDelegate();
	public delegate void OnAttackDealDamageDelegate();
	public delegate void OnAttackEndDelegate();
	public delegate void OnEatDelegate();

	public event OnAttachprojectile 		onAttachProjectile;
	public event OnAttackStartDelegate 		onAttackStart;
	public event OnAttackDealDamageDelegate onAttackDealDamage;
	public event OnAttackEndDelegate 		onAttackEnd;
	public event OnEatDelegate 				onEat;



	// ------------------------------------------------------- //

	public void AttachProjectile() {
		if (onAttachProjectile != null)
			onAttachProjectile();
	}

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

	public void EatEvent()
	{
		if (onEat != null)
			onEat();
	}
}
