using UnityEngine;

public interface ISpawner : IQuadTreeItem {    
	string name { get; }
	void Initialize();
    void Clear();
    void ForceRemoveEntities();
    void ForceReset(); // Used for debug purpose    
        
    bool CanRespawn();
	bool Respawn(); //return true if it respawned completelly
	void RemoveEntity(GameObject _entity, bool _killedByPlayer);
	void DrawStateGizmos();
	bool SpawnersCheckCurrents();

	AreaBounds area { get; }
	IGuideFunction guideFunction { get; }
	Transform transform { get; }
	Quaternion rotation { get; }

#region save_spawner_state
	int GetSpawnerID();
	AbstractSpawnerData Save();
	void Save( ref AbstractSpawnerData _data);
	void Load( AbstractSpawnerData _data);
#endregion
	// Abstract
	// EntitiesKilled
	// EntitiesToSpawn

	// Spawner
	// m_respawnCount
	// m_respawnTime

}
