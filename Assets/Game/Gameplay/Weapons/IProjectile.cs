using UnityEngine;

public interface IProjectile {
	void AttachTo(Transform _parent);
	void AttachTo(Transform _parent, Vector3 _offset);

	// Shoots At player from transform position _from
	void Shoot(Vector3 _target, float _damage);

	// Shoots towards a direction. We can override the speed and damage
	void ShootTowards(Vector3 _direction, float _speed, float _damage);

	// Shoots At world position _pos
	void ShootAtPosition(Transform _from, float _damage, Vector3 _pos);

	void Explode(bool _hitDragon);
}