using UnityEngine;

public interface ISpawner : IQuadTreeItem {
	string name { get; }
	void Initialize();
	void ForceRemoveEntities();
	void CheckRespawn();
	void Respawn();
}
