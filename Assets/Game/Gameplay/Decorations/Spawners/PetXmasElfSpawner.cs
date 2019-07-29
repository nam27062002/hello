using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AI;

public class PetXmasElfSpawner : MonoBehaviour, ISpawner, IBroadcastListener {	

	[SerializeField] private Transform m_spawnAtTransform;
	public List<string> m_possibleSpawners;
	private PoolHandler[] m_poolHandlers;

	private string m_entityPrefabStr;
	private int m_entityPrefabIndex;
	private PoolHandler m_selectedPoolHandler;

	struct EntityInfo
	{
		public Entity m_entity;
		public int m_poolIndex;
	}
	List<EntityInfo> m_entityInfo;

    //-------------------------------------------------------------------

    protected  void Start() {
		// SpawnerManager.instance.Register(this, true);
		m_entityInfo = new List<EntityInfo>();
		Initialize();
        Broadcaster.AddListener(BroadcastEventType.POOL_MANAGER_READY, this);
        Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);
    }

	void OnDestroy() {
        Broadcaster.RemoveListener(BroadcastEventType.POOL_MANAGER_READY, this);
        Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);
	}


    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.POOL_MANAGER_READY:
            case BroadcastEventType.GAME_AREA_ENTER:
            {
                CreatePool();
            }break;
        }
    }
    
    
	public void Initialize() {
		m_poolHandlers = new PoolHandler[m_possibleSpawners.Count];
		CreatePool();
		// create a projectile from resources (by name) and save it into pool

	}

	public void Clear() {
		ForceRemoveEntities();
		gameObject.SetActive(false);
	}

    public List<string> GetPrefabList() {
        return null;
    }

    public void ForceRemoveEntities(){
		for( int i = m_entityInfo.Count - 1; i >= 0; i-- ){
			RemoveEntity(m_entityInfo[i].m_entity, false);
		}
	}

	public void ForceReset() {
		ForceRemoveEntities();
		Initialize();
	}

	public void ForceGolden( IEntity entity ){
		// entity.SetGolden(Spawner.EntityGoldMode.Gold);	
	}

	void CreatePool() {
		for (int i = 0; i<m_possibleSpawners.Count; i++) {
			m_poolHandlers[i] = PoolManager.CreatePool( m_possibleSpawners[i], 2, false, false);
		}
	}

   
    public bool HasAvailableEntities(){
    	bool ret = false;
		for( int i = 0; i<m_poolHandlers.Length && !ret; ++i ){
			ret = m_poolHandlers[i].HasAvailableInstances();
    	}
    	return ret;
    }

	public void RamdomizeEntity(){
		m_selectedPoolHandler = null;
		if ( HasAvailableEntities() )
		{
			int index = 0;
			do
			{
				index = Random.Range( 0, m_poolHandlers.Length);
				m_selectedPoolHandler = m_poolHandlers[index];
			}while( !m_selectedPoolHandler.HasAvailableInstances() );

			m_entityPrefabStr = m_possibleSpawners[ index ];
			m_entityPrefabIndex = index;
		}
    }

    public string GetSelectedPrefabStr(){
    	return m_entityPrefabStr;
    }

	public bool MustCheckCameraBounds(){ return true; }

	protected void OnEntitySpawned(IEntity spawning, uint index, Vector3 originPos) {
        Transform t = spawning.transform;
		t.position = m_spawnAtTransform.position;
		t.rotation = m_spawnAtTransform.rotation;
		t.localScale = Vector3.one;
    }


	public void RemoveEntity(IEntity _entity, bool _killedByPlayer) {
		for( int i = m_entityInfo.Count - 1; i >= 0; --i )
		{
			if ( m_entityInfo[i].m_entity == _entity)
			{
				PoolHandler handler = m_poolHandlers[ m_entityInfo[i].m_poolIndex ];
				if (ProfilerSettingsManager.ENABLED) {               
					SpawnerManager.RemoveFromTotalLogicUnits(1, m_possibleSpawners[ m_entityInfo[i].m_poolIndex ]);
				}
				// Returns the entity to the pool
				handler.ReturnInstance(_entity.gameObject);
				m_entityInfo.RemoveAt(i);
				break;
			}
		}

		// Unregisters the entity
		EntityManager.instance.UnregisterEntity(_entity as Entity);
	} 

	public bool IsRespawing(){
		return true;
	}
	public bool IsRespawingPeriodically(){
		return true;
	}
	public bool CanRespawn(){
		return HasAvailableEntities();
	}


	//return true if it respawned completelly
    public bool Respawn() {

    	bool ret = false;
    	if ( m_selectedPoolHandler != null )
    	{
			GameObject spawning = m_selectedPoolHandler.GetInstance(true);

			if (spawning != null) {
				Transform spawningTransform = spawning.transform;
				spawningTransform.rotation = Quaternion.identity;
				spawningTransform.localRotation = Quaternion.identity;
				spawningTransform.localScale = Vector3.one;
				spawningTransform.position = m_spawnAtTransform.position;

				Entity entity = spawning.GetComponent<Entity>();
				if (entity != null) {
					EntityManager.instance.RegisterEntity(entity);
					entity.Spawn(this); // lets spawn Entity component first
				}

				ISpawnable[] components = spawning.GetComponents<ISpawnable>();
				foreach (ISpawnable component in components) {
					if (component != entity ) {
						component.Spawn(this);
					}
				}

				if (ProfilerSettingsManager.ENABLED) {
					SpawnerManager.AddToTotalLogicUnits(1, m_entityPrefabStr);
				}

				EntityInfo info;
				info.m_entity = entity;
				info.m_poolIndex = m_entityPrefabIndex;
				m_entityInfo.Add(info);
				ret = true;
			}
		}
		return ret;
    }    

	public void DrawStateGizmos() {}


	// public AreaBounds area { get{return null;} }
	private AreaBounds m_area;
	public AreaBounds area {
		get {
			if (m_area == null) m_area = new RectAreaBounds(m_rect.center, m_rect.size);
			else 				m_area.UpdateBounds(m_rect.center, m_rect.size);
			return m_area;
		}
	}
	public IGuideFunction guideFunction { get{ return null; } }
	public Quaternion rotation { get { return Quaternion.identity; } }
	public Vector3 homePosition { get{ return Vector3.zero; } }
	// public Rect boundingRect { get { return Rect.zero; } }
	[SerializeField] protected Rect m_rect = new Rect(Vector2.zero, Vector2.one * 2f);
	public Rect boundingRect { get { return m_rect; } }

#region save_spawner_state
	public virtual int GetSpawnerID(){return -1;}
	public virtual AbstractSpawnerData Save(){return null;}
	public virtual void Save( ref AbstractSpawnerData _data){}
	public virtual void Load(AbstractSpawnerData _data){}
#endregion
    //-------------------------------------------------------------------

}
