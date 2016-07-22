using UnityEngine;

public interface IProjectile {
	void AttachTo(Transform _parent);
	void Shoot(Transform _from, float _damage);
}