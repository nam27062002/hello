using UnityEngine;

/// <summary>
/// This class is responsible for giving an implementation for the ISpawner interface that different subclasses can share. These are some of this class responsabilities:
/// 1)Entities are spawned progressively so the load is split in several frames to prevent frame rate drops
/// 2)Entities are taken and returned to PoolManager
/// 3)Entities are registered/unregistered in EntityManager 
/// </summary>
public abstract class AbstractSpawner : MonoBehaviour, ISpawner
{
    void Start()
    {
        StartExtended();
    }

    protected virtual void StartExtended() { }

    public abstract void Initialize();
    public abstract void ForceRemoveEntities();

    public abstract bool CanRespawn();
    public abstract bool Respawn(); //return true if it respawned completelly

    public void RemoveEntity(GameObject _entity, bool _killedByPlayer)
    {
        if (RemoveEntityExtended(_entity, _killedByPlayer))
        {

            // Unregisters the entity
            Entity entity = _entity.GetComponent<Entity>();
            if (entity != null)
            {
                EntityManager.instance.Unregister(entity);
            }

            // Returns the entity to the pool
            ReturnEntityToPool(_entity);
        }
    }

    /// <summary>
    /// Template method so every subclass can do its own stuff
    /// </summary>    
    /// <returns></returns>
    protected virtual bool RemoveEntityExtended(GameObject _entity, bool _killedPlayer) { return true; }

    protected virtual void ReturnEntityToPool(GameObject _entity)
    {
        PoolManager.ReturnInstance(_entity);
    }

    public abstract void DrawStateGizmos();

    public virtual AreaBounds area { get; set; }
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
}
