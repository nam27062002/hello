using UnityEngine;

public enum ERespawnPendingTask
{
    None,
    ForceRemoveEntities,
    KeepRespawning
};


public interface ISpawner : IQuadTreeItem {    
	string name { get; }
	void Initialize();    
    void ForceRemoveEntities();
    
    ERespawnPendingTask RespawnPendingTask { get; set; } // It's only relevant for those spawners that can split their respawning stuff in several frames (to boost performance)
    bool IsRespawningWithDelay(); // It's only relevant for those spawners that can split their respawning stuff in several frames (to boost performance)
    bool CanRespawn();
	bool Respawn(); //return true if it respawned completelly
	void RemoveEntity(GameObject _entity, bool _killedByPlayer);
	void DrawStateGizmos();

	AreaBounds area{ get; }
	IGuideFunction guideFunction{ get; }
	Transform transform{ get; }
	Rect boundingRect { get; }
}
