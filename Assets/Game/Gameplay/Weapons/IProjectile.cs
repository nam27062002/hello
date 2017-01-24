using UnityEngine;

public interface IProjectile {
	void AttachTo(Transform _parent);
		// Shoots At player from transform position _from
	void Shoot(Vector3 _target, float _damage = 0f);
		// Shoots At world position _pos
	void ShootAtPosition( Transform _from, float _damage, Vector3 _pos);

	void Explode(bool _hitDragon);
}