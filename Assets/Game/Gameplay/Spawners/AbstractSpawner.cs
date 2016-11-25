﻿using UnityEngine;

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

    protected enum EState
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

    void Start() {
        OnStart();
    }

    private void OnDestroy() {
        if (SpawnerManager.isInstanceCreated)
            SpawnerManager.instance.Unregister(this, UseSpawnManagerTree);
    }

    public void Initialize() {
        m_rect = new Rect((Vector2)transform.position, Vector2.zero);
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

    public bool CanRespawn() {
        return (State == EState.Respawning) ? CanRespawnExtended() : false;       
    }
    
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

            string prefabName;
            GameObject go;
            long start = sm_watch.ElapsedMilliseconds;
            while (EntitiesAlive < EntitiesToSpawn) {
                prefabName = GetPrefabNameToSpawn(EntitiesAlive);
                go = PoolManager.GetInstance(prefabName, !UseProgressiveRespawn);                
                OnCreateInstance(EntitiesAlive, go);
                m_entities[EntitiesAlive] = go.GetComponent<IEntity>();                                  
                EntitiesAlive++;

                if (ProfilerSettingsManager.ENABLED) {
                    SpawnerManager.AddToTotalLogicUnits(1, prefabName);
                }

                if (m_useProgressiveRespawn && sm_watch.ElapsedMilliseconds - start >= SpawnerManager.SPAWNING_MAX_TIME) {
                    break;
                }
            }

            if (EntitiesAlive == EntitiesToSpawn) {
                EntitiesSpawned = 0;
                State = EState.Activating_Instances;
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
            if (!spawning.activeSelf) {
                spawning.SetActive(true);
            }

            if (m_guideFunction != null) {
                m_guideFunction.ResetTime();
            }

            OnEntitySpawned(spawning, EntitiesSpawned, startPosition);

            EntitiesSpawned++;

            IEntity entity = spawning.GetComponent<IEntity>();
            if (entity != null) {
                RegisterInEntityManager(entity);
                entity.Spawn(this); // lets spawn Entity component first
            }

            AI.Machine machine = spawning.GetComponent<AI.Machine>();
            if (machine != null) {
                machine.Spawn(this);
                OnMachineSpawned(machine);
            }

            AI.AIPilot pilot = spawning.GetComponent<AI.AIPilot>();
            if (pilot != null) {
                OnPilotSpawned(pilot);
                pilot.Spawn(this);
            }

            ISpawnable[] components = spawning.GetComponents<ISpawnable>();
            foreach (ISpawnable component in components) {
                if (component != entity && component != pilot && component != machine) {
                    component.Spawn(this);
                }
            }

            if (m_useProgressiveRespawn && sm_watch.ElapsedMilliseconds - start >= SpawnerManager.SPAWNING_MAX_TIME) {
                break;
            }
        }
    }        

    public void ForceRemoveEntities() {
        for (int i = 0; i < EntitiesToSpawn; i++) {
            if (m_entities[i] != null) {
                RemoveEntity(m_entities[i].gameObject, false);
            }
        }

        EntitiesAlive = 0;
        EntitiesSpawned = 0;        
        EntitiesAllKilledByPlayer = false;
        State = EState.Respawning;
        
        OnForceRemoveEntities();
    }    

    public void RemoveEntity(GameObject _entity, bool _killedByPlayer) {
        int index = -1;
        for (int i = 0; i < EntitiesToSpawn && index == -1; i++) {
            if (m_entities[i] != null && m_entities[i].gameObject == _entity) {
                index = i;                                                
            }
        }

        if (index > -1) {
            if (_killedByPlayer)
            {
                EntitiesKilled++;
            }
            EntitiesAlive--;

            string prefabName = GetPrefabNameToSpawn((uint)index);
            if (ProfilerSettingsManager.ENABLED)
            {               
                SpawnerManager.RemoveFromTotalLogicUnits(1, prefabName);
            }

            // Unregisters the entity            
            if (m_entities[index] != null) {
                UnregisterFromEntityManager(m_entities[index]);
            }

            // Returns the entity to the pool
            ReturnEntityToPool(prefabName, _entity);

            OnRemoveEntity(_entity, index);

            m_entities[index] = null;            

            // Check if all entities have been destroyed
            if (EntitiesAlive == 0 && EntitiesSpawned == EntitiesToSpawn) {
                EntitiesAllKilledByPlayer = EntitiesKilled == EntitiesToSpawn;
                OnAllEntitiesRemoved(_entity, EntitiesAllKilledByPlayer);
                State = EState.Respawning;
            }
        }
    }

    protected virtual void ReturnEntityToPool(string _prefabName, GameObject _entity) {
        if (string.IsNullOrEmpty(_prefabName)) {
            PoolManager.ReturnInstance(_entity);
        } else {
            PoolManager.ReturnInstance(_prefabName, _entity);
        }
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

    protected Rect m_rect;
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
    public virtual AreaBounds area { get; set; }
    protected virtual void RegisterInEntityManager(IEntity e)
    {
        EntityManager.instance.RegisterEntity(e as Entity);
    }

    protected virtual void UnregisterFromEntityManager(IEntity e)
    {
        EntityManager.instance.UnregisterEntity(e as Entity);
    }

    protected virtual void OnStart() {}
    protected abstract uint GetMaxEntities();
    protected virtual void OnInitialize() {}    
    protected virtual bool CanRespawnExtended() { return true; }
    protected virtual void OnPrepareRespawning() {}
    protected abstract uint GetEntitiesAmountToRespawn();    
    protected abstract string GetPrefabNameToSpawn(uint index);
    protected virtual void OnCreateInstance(uint index, GameObject go) {}    
    protected virtual void OnEntitySpawned(GameObject spawning, uint index, Vector3 originPos) {}
    protected virtual void OnMachineSpawned(AI.Machine machine) {}
    protected virtual void OnPilotSpawned(AI.Pilot pilot) {}
    protected virtual void OnAllEntitiesRespawned() {}    
    protected virtual void OnRemoveEntity(GameObject _entity, int index) {}
    protected virtual void OnAllEntitiesRemoved(GameObject _lastEntity, bool _allKilledByPlayer) {}
    protected virtual void OnForceRemoveEntities() {}
    public virtual void DrawStateGizmos() {}
    #endregion
}
