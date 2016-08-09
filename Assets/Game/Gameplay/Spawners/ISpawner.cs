using UnityEngine;

public interface ISpawner {
	void Initialize();
	void ForceRemoveEntities();
	void UpdateTimers();
	void UpdateLogic();
	void Respawn();
	void RemoveEntity(GameObject _entity, bool _killedByPlayer);

	AreaBounds area{ get; }
	Transform transform{ get; }

}
