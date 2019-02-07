using UnityEngine;

public interface ISpawner : IQuadTreeItem {    
	string name { get; }
	void Initialize();
    void Clear();
    
	bool IsRespawing();
	bool IsRespawingPeriodically();
	bool CanRespawn();
	bool Respawn(); //return true if it respawned completelly

	bool MustCheckCameraBounds(); // this spawner will kill its entities if it is outside camera disable area

	void RemoveEntity(IEntity _entity, bool _killedByPlayer);
	void ForceRemoveEntities();
    void ForceReset(); // Used for debug purpose    
    void ForceGolden( IEntity entity );
    
	void DrawStateGizmos();

	AreaBounds area { get; }
	IGuideFunction guideFunction { get; }
	Transform transform { get; }
	Quaternion rotation { get; }
	Vector3 homePosition { get; }

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
