using UnityEngine;

public interface IAttacker {
	void StartAttackTarget(Transform _transform);
	void StopAttackTarget();
	void StartEating();
	void StopEating();
}
