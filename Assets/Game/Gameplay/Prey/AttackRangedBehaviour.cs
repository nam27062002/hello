using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SensePlayer))]
public class AttackRangedBehaviour : AttackBehaviour {
	
	[SerializeField] private bool m_canAim = false;
	[SerializeField] private Transform m_eye;
	[SerializeField] private GameObject m_projectilePrefab;
	[SerializeField] private Transform m_projectileSpawnPoint;

	private GameObject m_projectile;

	// Use this for initialization
	protected override void Start () {

		Debug.Assert(m_projectilePrefab != null, "Attach a projectile.");

		// create a pool of projectiles
		PoolManager.CreatePool(m_projectilePrefab, 2, true);

		base.Start();
	}

	protected override void OnAttackStart_Extended() {}

	protected override void OnAttachProjectile_Extended() {	
		if (m_projectile == null) {
			m_projectile = PoolManager.GetInstance(m_projectilePrefab.name);

			if (m_projectile != null) {
				ProjectileBehaviour projectile = m_projectile.GetComponent<ProjectileBehaviour>();
				projectile.AttachTo(m_projectileSpawnPoint);
			} else {
				Debug.LogError("Projectile not available");
			}
		}
	}

	protected override void OnAttack_Extended() {
		if (m_projectile != null) {
			Transform target = transform;
			ProjectileBehaviour projectile = m_projectile.GetComponent<ProjectileBehaviour>();

			if (m_projectileSpawnPoint != null) {
				target = m_projectileSpawnPoint;
			}

			projectile.Shoot(transform, m_damage);
			m_projectile = null;
		}
	}

	protected override void OnAttackEnd_Extended() {}

	protected override void UpdateOrientation() {
		if (m_canAim) {
			if (m_eye != null && m_target != null) {
				Vector3 targetDir = m_target.position - m_eye.position;

				targetDir.Normalize();
				Vector3 cross = Vector3.Cross(targetDir, Vector3.right);
				float aim = cross.z * -1;

				//between aim [0.9 - 1 - 0.9] we'll rotate the model
				//for testing purpose, it'll go from 90 to 270 degrees and back. Aim value 1 is 180 degrees of rotation
				float absAim = Mathf.Abs(aim);

				float angleSide = 0f;
				if (targetDir.x < 0) {
					angleSide = 180f;
				}
				float angle = angleSide;

				if (absAim >= 0.6f) {
					angle = (((absAim - 0.6f) / (1f - 0.6f)) * (180f - angleSide)) + angleSide;
				}

				// face target
				m_orientation.SetAngle(angle);

				// blend between attack directions
				m_animator.SetFloat("aim", aim);
			}
		} else {
			base.UpdateOrientation();
		}
	}
}
