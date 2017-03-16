using UnityEngine;

public interface IProjectile {
	void AttachTo(Transform _parent);
	void AttachTo(Transform _parent, Vector3 _offset);

	// Shoots At player from transform position _from
	void Shoot(Transform _target, Vector3 _direction, float _damage);

	// Shoots towards a direction. We can override the speed and damage
	void ShootTowards(Vector3 _direction, float _speed, float _damage);

	// Shoots At world position _pos
	void ShootAtPosition(Vector3 _target, Vector3 _direction, float _damage);

	//
	void Explode(bool _hitDragon);
}