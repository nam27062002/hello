using UnityEngine;

public class PreyAnimationEvents : MonoBehaviour {

	public delegate void OnAttachprojectile();
	public delegate void OnAttackStartDelegate();
	public delegate void OnAttackDealDamageDelegate();
	public delegate void OnAttackEndDelegate();
	public delegate void OnEatDelegate();
	public delegate void OnStandUpDelegate();

	public event OnAttachprojectile 		onAttachProjectile;
	public event OnAttackStartDelegate 		onAttackStart;
	public event OnAttackDealDamageDelegate onAttackDealDamage;
	public event OnAttackEndDelegate 		onAttackEnd;
	public event OnEatDelegate 				onEat;
	public event OnStandUpDelegate			onStandUp;

	// To avoid blend trees to fire the same event twice in one frame we will use a flag
	// For the moment we use attack start flag for the archer
	private bool m_attackStartFlag = true;
	private bool m_attackEndFlag = false;
	// ------------------------------------------------------- //

	public void AttachProjectile() {
		if (onAttachProjectile != null)
			onAttachProjectile();
	}

	public void AttackStart() {
		if (m_attackStartFlag){
			m_attackStartFlag = false;
			m_attackEndFlag = true;
			if (onAttackStart != null){
				onAttackStart();
			}
		}
	}

	public void AttackDealDamage() {
		if (onAttackDealDamage != null)
			onAttackDealDamage();
	}

	public void AttackEnd() {
		if (m_attackEndFlag) {
			m_attackStartFlag = true;
			m_attackEndFlag = false;
			if (onAttackEnd != null)
				onAttackEnd();
		}
	}

	public void EatEvent() {
		if (onEat != null)
			onEat();
	}

	public void OnStandUpEvent() {
		if (onStandUp != null) {
			onStandUp();
		}
	}
}
