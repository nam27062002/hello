﻿using UnityEngine;

public class PreyAnimationEvents : MonoBehaviour {

	public delegate void OnAttachprojectile();
	public delegate void OnAttackStartDelegate();
	public delegate void OnAttackDealDamageDelegate();
	public delegate void OnAttackEndDelegate();

	public event OnAttachprojectile 		onAttachProjectile;
	public event OnAttackStartDelegate 		onAttackStart;
	public event OnAttackDealDamageDelegate onAttackDealDamage;
	public event OnAttackEndDelegate 		onAttackEnd;


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
}
