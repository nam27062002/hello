using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SensePlayer))]
public class AttackMeleeBehaviour : AttackBehaviour {
	
	[SerializeField] private MeleeWeapon m_meleeWeapon = null;


	protected override void Start () {
		if (m_meleeWeapon != null) {
			m_meleeWeapon.damage = m_damage;
			m_meleeWeapon.enabled = false;
		}

		base.Start();
	}

	protected override void OnDisable() {
		if (m_meleeWeapon != null) {
			m_meleeWeapon.enabled = false;
		}

		base.OnDisable();
	}

	protected override void OnAttackStart_Extended() {}

	protected override void OnAttachProjectile_Extended() {}

	protected override void OnAttack_Extended() {
		if (m_meleeWeapon != null) {
			m_meleeWeapon.enabled = true;
		}
	}

	protected override void OnAttackEnd_Extended() {
		if (m_meleeWeapon != null) {
			m_meleeWeapon.enabled = false;
		}
	}
}
