using UnityEngine;

public interface ISpawner : IQuadTreeItem {
	string name { get; }
	void Initialize();
	void ForceRemoveEntities();
	void CheckRespawn();
	void Respawn();
	void RemoveEntity(GameObject _entity, bool _killedByPlayer);

	AreaBounds area{ get; }
	IGuideFunction guideFunction{ get; }
	Transform transform{ get; }

}
