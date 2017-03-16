using UnityEngine;

public interface ISpawner : IQuadTreeItem {    
	string name { get; }
	void Initialize();
    void Clear();
    void ForceRemoveEntities();
        
    bool CanRespawn();
	bool Respawn(); //return true if it respawned completelly
	void RemoveEntity(GameObject _entity, bool _killedByPlayer);
	void DrawStateGizmos();
	bool SpawnersCheckCurrents();

	AreaBounds area { get; }
	IGuideFunction guideFunction { get; }
	Transform transform { get; }
	Quaternion rotation { get; }
}
