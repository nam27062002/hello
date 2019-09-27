using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This class is responsible for giving an implementation for the ISpawner interface that different subclasses can share. These are some of this class responsabilities:
/// 1)Entities are spawned progressively so the load is split in several frames to prevent frame rate drops
/// 2)Entities are taken and returned to PoolManager
/// 3)Entities are registered/unregistered in EntityManager 
/// </summary>
public abstract class AbstractSpawner : MonoBehaviour, ISpawner
{
    /// <summary>
    /// When <c>true</c> the spawner will be told to respawn only when its area is inside camera respawn area. When <c>false</c> respawning will be managed by another controller
    /// </summary>
    private bool m_useSpawnManagerTree = true;
    protected bool UseSpawnManagerTree
    {
        get { return m_useSpawnManagerTree; }
        set { m_useSpawnManagerTree = value; }
    }

    /// <summary>
    /// When <c>true</c> the entities are respawned progressively, otherwise all entities will be respawned with a single call to Respawn() regardless the time that the operation
    /// will take. You should have a good reason to change this value to <c>false</c> as disabling it might cause a severe frame rate drop
    /// </summary>
    private bool m_useProgressiveRespawn = true;
    protected bool UseProgressiveRespawn {
        get { return m_useProgressiveRespawn; }
        set { m_useProgressiveRespawn = value; }
    }

    public enum EState
    {
        Init = 0,
        Respawning,
        Create_Instances,
        Activating_Instances,
        Alive
    }

    private EState m_state = EState.Init;
    protected EState State {
        get { return m_state; }
        set { m_state = value; }
    }

    /// <summary>
    /// Watch used to meassure time time spent on spawning entities. It's defined static to save memory.
    /// </summary>
    private static System.Diagnostics.Stopwatch sm_watch;

    private bool m_hasToDoStart = true;

#if UNITY_EDITOR || !USE_OPTIMIZED_SCENES
    void Start() {
        if (m_hasToDoStart) {
            DoStart();
        }
    }
#endif

    public void DoStart() {
        if (m_hasToDoStart) {
            m_hasToDoStart = false;

            if (m_rect.size == Vector2.zero) {
                m_rect.size = Vector2.one * 2f;
            }
            m_rect.center = (Vector2)transform.position + m_rect.position;

            OnStart();
        }
    }

    protected virtual void OnDestroy() {
        if (SpawnerManager.isInstanceCreated && ApplicationManager.IsAlive)
            SpawnerManager.instance.Unregister(this, UseSpawnManagerTree);
    }

    public void Initialize() {
        Entities_Create(GetMaxEntities());

        EntitiesAlive = 0;
        EntitiesToSpawn = 0;
        EntitiesSpawned = 0;
        EntitiesKilled = 0;
        EntitiesAllKilledByPlayer = false;
        State = EState.Respawning;

        OnInitialize();
    }

    public void Clear() {
        ForceRemoveEntities();
        gameObject.SetActive(false);
    }

    // Delegate called when the spawner is done with the stuff it had to do
    public System.Action<AbstractSpawner> OnDone { get; set; }

	public bool IsRespawing() {
		return (State >= EState.Respawning) && (State < EState.Alive);
	}

    public bool CanRespawn() {
        return (State == EState.Respawning) ? CanRespawnExtended() : false;       
    }

	// this spawner will kill its entities if it is outside camera disable area
	public virtual bool MustCheckCameraBounds()	 	{ return false; }
	public virtual bool IsRespawingPeriodically() 	{ return false; }
    
    //return true if it respawned completelly
    public bool Respawn() {
        if (State == EState.Respawning) {
            OnPrepareRespawning();
            EntitiesToSpawn = GetEntitiesAmountToRespawn();
            EntitiesSpawned = 0;
            EntitiesAlive = 0;           

            EntitiesKilled = 0;
            EntitiesAllKilledByPlayer = false;

            State = EState.Create_Instances;

            if (UseProgressiveRespawn) {
                return false;
            }
        }

        if (State == EState.Create_Instances) {
            if (sm_watch == null) {
                sm_watch = new System.Diagnostics.Stopwatch();
                sm_watch.Start();
            }

			PoolHandler handler;
            GameObject go;
            long start = sm_watch.ElapsedMilliseconds;
            while (EntitiesAlive < EntitiesToSpawn) {
				string prefabName = GetPrefabNameToSpawn(EntitiesAlive);
				handler = GetPoolHandler(EntitiesAlive);
				go = handler.GetInstance(!UseProgressiveRespawn);

				if (go == null) {
                    EntitiesToSpawn--;
                    break;
				} else {
					go.transform.position = transform.position;
					OnCreateInstance(EntitiesAlive, go);
	                m_entities[EntitiesAlive] = go.GetComponent<IEntity>();                                  
	                EntitiesAlive++;

	                if (ProfilerSettingsManager.ENABLED) {
						SpawnerManager.AddToTotalLogicUnits(1, prefabName);
	                }
				}

                if (m_useProgressiveRespawn && sm_watch.ElapsedMilliseconds - start >= SpawnerManager.SPAWNING_MAX_TIME) {
                    break;
                }
            }

            if (EntitiesAlive == EntitiesToSpawn) {
                if (EntitiesToSpawn == 0) {
                    // This spawner can't spawn anything this time, so we'll disable it and restart the respawn timer
                    OnAllEntitiesRemoved(null, true);
                    return true;
                } else {
                    EntitiesSpawned = 0;
                    State = EState.Activating_Instances;
                }
            }

            if (UseProgressiveRespawn) {
                return false;
            }
        }

        if (State == EState.Activating_Instances) {
            ActivateEntities();

            if (EntitiesSpawned == EntitiesToSpawn) {
                OnAllEntitiesRespawned();
                State = EState.Alive;
            }
        }

        return State == EState.Alive;
    }    

    private void ActivateEntities() {
        long start = sm_watch.ElapsedMilliseconds;
        Vector3 startPosition = transform.position;
        while (EntitiesSpawned < EntitiesToSpawn) {            
            GameObject spawning = m_entities[EntitiesSpawned].gameObject;

			Transform spawningTransform = spawning.transform;
			spawningTransform.rotation = Quaternion.identity;
			spawningTransform.localRotation = Quaternion.identity;
			spawningTransform.localScale = Vector3.one;

            if (!spawning.activeSelf) {
                spawning.SetActive(true);
            }

            if (m_guideFunction != null) {
                m_guideFunction.ResetTime();
            }

            IEntity entity = spawning.GetComponent<IEntity>();
            if (entity != null) {
                RegisterInEntityManager(entity);
                entity.Spawn(this); // lets spawn Entity component first
				OnEntitySpawned(entity, EntitiesSpawned, startPosition);
            }

			IViewControl view = spawning.GetComponent<IViewControl>();
			if (view != null) {
				view.Spawn(this);
			}

			AI.IMachine machine = spawning.GetComponent<AI.IMachine>();
            if (machine != null) {
                machine.Spawn(this);
                OnMachineSpawned(machine, EntitiesSpawned);
			}

            AI.AIPilot pilot = spawning.GetComponent<AI.AIPilot>();
            if (pilot != null) {
                OnPilotSpawned(pilot);
                pilot.Spawn(this);
            }

            ISpawnable[] components;
            if (entity != null) {
                components = entity.m_otherSpawnables;
            }
            else {
                components = spawning.GetComponents<ISpawnable>();
            }

			EntitiesSpawned++;

            foreach (ISpawnable component in components)
            {
                if (component != entity && component != pilot && component != machine && component != view)
                {
                    component.Spawn(this);
                }
            }

            if (m_useProgressiveRespawn && sm_watch.ElapsedMilliseconds - start >= SpawnerManager.SPAWNING_MAX_TIME) {
                break;
            }
        }
    }        

    public virtual void ForceRemoveEntities() {
        for (int i = 0; i < EntitiesToSpawn; i++) {
            if (m_entities[i] != null) {
                RemoveEntity(m_entities[i], false);
            }
        }

        EntitiesAlive = 0;
        EntitiesSpawned = 0;        
        EntitiesAllKilledByPlayer = false;
        State = EState.Respawning;
        
        OnForceRemoveEntities();
    }    

    public void ForceReset() {
        ForceRemoveEntities();
        Initialize();        
    }    

    public virtual void ForceGolden( IEntity entity ){
		if( !entity.isGolden && entity.edibleFromTier <= InstanceManager.player.data.tier)
			entity.SetGolden(Spawner.EntityGoldMode.Gold);
    }

    public virtual void RemoveEntity(IEntity _entity, bool _killedByPlayer) {
        int index = -1;
        for (int i = 0; i < EntitiesToSpawn && index == -1; i++) {
            if (m_entities[i] != null && m_entities[i] == _entity) {
                index = i;                                                
            }
        }

        if (index > -1) {
            if (_killedByPlayer) {
                EntitiesKilled++;
            }
            EntitiesAlive--;

			PoolHandler handler = GetPoolHandler((uint)index);
            if (ProfilerSettingsManager.ENABLED) {               
				SpawnerManager.RemoveFromTotalLogicUnits(1, GetPrefabNameToSpawn(((uint)index)));
            }

            // Unregisters the entity            
            if (m_entities[index] != null) {
                UnregisterFromEntityManager(m_entities[index]);
            }

            // Returns the entity to the pool
			ReturnEntityToPool(handler, _entity.gameObject);

			OnRemoveEntity(_entity, index, _killedByPlayer);

            m_entities[index] = null;            

            // Check if all entities have been destroyed
            if (EntitiesAlive == 0 && EntitiesSpawned == EntitiesToSpawn) {
                EntitiesAllKilledByPlayer = EntitiesKilled == EntitiesToSpawn;
                OnAllEntitiesRemoved(_entity, EntitiesAllKilledByPlayer);
                State = EState.Respawning;
            }
        }
    }

	protected virtual void ReturnEntityToPool(PoolHandler _handler, GameObject _entity) {
		_handler.ReturnInstance(_entity);
    }

    protected void RegisterInSpawnerManager() {
        SpawnerManager.instance.Register(this, UseSpawnManagerTree);
    }

    protected void UnregisterFromSpawnerManager() {
        SpawnerManager.instance.Unregister(this, UseSpawnManagerTree);
        if (OnDone != null) {
            OnDone(this);
        }
    }
    
    protected IGuideFunction m_guideFunction = null;
    public IGuideFunction guideFunction
    {
        get
        {
            return m_guideFunction;
        }
    }
		
	[SerializeField] protected Rect m_rect = new Rect(Vector2.zero, Vector2.one * 2f);
    public Rect boundingRect { get { return m_rect; } }

    #region entities
    protected IEntity[] m_entities;    

    protected uint EntitiesToSpawn { get; set; }
    private uint EntitiesSpawned { get; set; }
    protected uint EntitiesAlive { get; set; }
    protected uint EntitiesKilled { get; set; }
    protected bool EntitiesAllKilledByPlayer { get; set; }

    private void Entities_Create(uint amount) {
        m_entities = new IEntity[amount];        
    }
    #endregion

    #region interface_for_subclasses
    public abstract List<string> GetPrefabList();

    public virtual AreaBounds area { get; set; }
	public Quaternion rotation { get { return transform.rotation; } }

	public virtual Vector3 homePosition { get { return transform.position; } }

    protected virtual void RegisterInEntityManager(IEntity e)
    {
        if (EntityManager.instance != null)
        {
            EntityManager.instance.RegisterEntity(e as Entity);
        }
    }

    protected virtual void UnregisterFromEntityManager(IEntity e)
    {
        if (EntityManager.instance != null)
        {
            EntityManager.instance.UnregisterEntity(e as Entity);
        }
    }

    protected virtual void OnStart() {}
    protected abstract uint GetMaxEntities();
    protected virtual void OnInitialize() {}    
    protected virtual bool CanRespawnExtended() { return true; }
    protected virtual void OnPrepareRespawning() {}
    protected abstract uint GetEntitiesAmountToRespawn();    
    protected abstract PoolHandler GetPoolHandler(uint index);
	protected abstract string GetPrefabNameToSpawn(uint index);
    protected virtual void OnCreateInstance(uint index, GameObject go) {}    
	protected virtual void OnEntitySpawned(IEntity spawning, uint index, Vector3 originPos) {}
	protected virtual void OnMachineSpawned(AI.IMachine machine, uint index) {}
    protected virtual void OnPilotSpawned(AI.Pilot pilot) {}
    protected virtual void OnAllEntitiesRespawned() {}    
	protected virtual void OnRemoveEntity(IEntity _entity, int index, bool _killedByPlayer) {}
    protected virtual void OnAllEntitiesRemoved(IEntity _lastEntity, bool _allKilledByPlayer) {}
    protected virtual void OnForceRemoveEntities() {}
    public virtual void DrawStateGizmos() {}
    #endregion


	#region save_spawner_state
	public virtual int GetSpawnerID()
	{
		return transform.position.GetHashCode() ^ name.GetHashCode();
	}
	public virtual AbstractSpawnerData Save()
	{
		AbstractSpawnerData data = new AbstractSpawnerData();
		Save( ref data );
		return data;
	}
	public virtual void Save( ref AbstractSpawnerData _data)
	{
		_data.m_entitiesKilled = EntitiesKilled;
		_data.m_entitiesToSpawn = EntitiesToSpawn;
	}
	public virtual void Load(AbstractSpawnerData _data)
	{
		EntitiesKilled = _data.m_entitiesKilled;
		EntitiesToSpawn = _data.m_entitiesToSpawn;
	}
	#endregion
}
