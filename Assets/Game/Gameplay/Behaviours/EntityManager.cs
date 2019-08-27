using UnityEngine;
using System.Collections.Generic;

public class EntityManager : UbiBCN.SingletonMonoBehaviour<EntityManager>, IBroadcastListener
{
    private List<Pet> m_pets;
    private List<Entity> m_entities;
    private List<EntityBg> m_entitiesBg;
	private List<Cage> m_cages;
    private List<Entity> m_searchList;
    private Rect m_area;


	public int totalVertexCount {
		get {
			int i = 0;
			int vc = 0;

			for (i = 0; i < m_cages.Count; ++i) 	{ vc += m_cages[i].GetVertexCount(); }
			for (i = 0; i < m_entities.Count; ++i) 	{ vc += m_entities[i].GetVertexCount(); }
			for (i = 0; i < m_entitiesBg.Count; ++i){ vc += m_entitiesBg[i].GetVertexCount(); }

			return vc;
		}
	}

	public int drawCalls {
		get {
			int i = 0;
			int dc = 0;

			for (i = 0; i < m_cages.Count; ++i) 	{ dc += m_cages[i].GetRendererCount(); }
			for (i = 0; i < m_entities.Count; ++i) 	{ dc += m_entities[i].GetRendererCount(); }
			for (i = 0; i < m_entitiesBg.Count; ++i){ dc += m_entitiesBg[i].GetRendererCount(); }

			return dc;
		}
	}

    public enum OverlapingMethod
    {
        EntitiesManager,
        Box,
        Capsule
    }
    private OverlapingMethod m_overlapingMethod = OverlapingMethod.EntitiesManager;
    public OverlapingMethod overlapingMethod
    {
        get { return m_overlapingMethod; }
        set { m_overlapingMethod = value; }
    }
    private Collider[] m_checkEntityColliders = new Collider[50];
    private int m_entitiesColliderMask = -1;

    private bool m_updateEnabled;


    void Awake()
    {
        m_pets = new List<Pet>();
        m_entities = new List<Entity>();
        m_entitiesBg = new List<EntityBg>();
		m_cages = new List<Cage>();
        m_searchList = new List<Entity>();
        m_entitiesColliderMask = 1 << LayerMask.NameToLayer("AirPreys") | 1 << LayerMask.NameToLayer("WaterPreys") | 1 << LayerMask.NameToLayer("MachinePreys") | 1 << LayerMask.NameToLayer("GroundPreys") | 1 << LayerMask.NameToLayer("Mines");

        m_updateEnabled = false;

        Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
        Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);
        Broadcaster.AddListener(BroadcastEventType.GAME_AREA_EXIT, this);
        Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);
    }

    override protected void OnDestroy()
    {
        base.OnDestroy();
        Broadcaster.RemoveListener(BroadcastEventType.GAME_ENDED, this);
        Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_EXIT, this);
        Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);
        Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
    }
    
    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch(eventType) 
        {
            case BroadcastEventType.GAME_LEVEL_LOADED:  m_updateEnabled = true; break;
            case BroadcastEventType.GAME_AREA_ENTER:    m_updateEnabled = true; break;
            case BroadcastEventType.GAME_AREA_EXIT:
                {
                    FreezingObjectsRegistry.instance.ClearEntities();
                    FreezingObjectsRegistry.instance.ClearScalings();
                    m_updateEnabled = false;
                }
                break;
            case BroadcastEventType.GAME_ENDED:         m_updateEnabled = false; OnGameEnded(); break;
        }
    }

    public void RegisterPet(Pet _pet)   { m_pets.Add(_pet); }
    public void UnregisterPet(Pet _pet) { m_pets.Remove(_pet); }

    public void RegisterEntity(Entity _entity) {
        // If an entity is registered after entities visibility was disabled then we make sure that entity won't be visible
        if (!Debug_EntitiesVisibility && FeatureSettingsManager.IsProfilerEnabled)
        {
            Debug_SetEntityVisible(_entity, Debug_EntitiesVisibility);
        }

        m_entities.Add(_entity);
    }
    public void UnregisterEntity(Entity _entity) 		{ m_entities.Remove(_entity); }

    public void RegisterEntityBg(EntityBg _entity) 		{ m_entitiesBg.Add(_entity); }
    public void UnregisterEntityBg(EntityBg _entity) 	{ m_entitiesBg.Remove(_entity); }

	public void RegisterEntityCage(Cage _cage)			{ m_cages.Add(_cage); }
	public void UnregisterEntityCage(Cage _cage)		{ m_cages.Remove(_cage); }


    public Entity[] GetEntitiesInRange2D(Vector2 _center, float _radius)
    {
        m_searchList.Clear();
		int size = m_entities.Count;
        for (int i = 0; i < size; ++i)
        {
            Entity e = m_entities[i];
            if (e != null)
            {
                if (e.IntersectsWith(_center, _radius))
                {
                    m_searchList.Add(e);
                }
            }
        }

        return m_searchList.ToArray();
    }

    public int GetEntitiesInRange2DNonAlloc(Vector2 _center, float _radius, Entity[] results)
    {
        int numResult = 0;
		int size = m_entities.Count;
		int length = results.Length;
        for (int i = 0; i < size && numResult < length; ++i)
        {
            Entity e = m_entities[i];
            if (e != null)
            {
                if (e.IntersectsWith(_center, _radius))
                {
                    results[numResult] = e;
                    numResult++;
                }
            }
        }
        return numResult;
    }


    public Entity GetEntityInRangeNearest2D(Vector2 _center, float _radius, DragonTier _tier)
    {
        float minDistSqr = _radius * _radius;
        Entity nearestEntity = null;
		int size = m_entities.Count;
        for (int i = 0; i < size; ++i)
        {
            Entity e = m_entities[i];
            if (e != null)
            {
                float distSqr = Vector2.SqrMagnitude((Vector2)e.transform.position - _center);
                if (distSqr <= _radius * _radius && e.IsEdible(_tier))
                {
                    if (distSqr <= minDistSqr)
                    {
                        minDistSqr = distSqr;
                        nearestEntity = e;
                    }
                }
            }
        }

        return nearestEntity;
    }

	public int GetCagesInRange2DNonAlloc(Vector2 _center, float _radius, Cage[] _results)
	{
		int numResult = 0;
		int size = m_cages.Count;
		int length = _results.Length;
		for (int i = 0; i < size && numResult < length; ++i)
		{
			Cage c = m_cages[i];
			if (c != null)
			{
				if (c.IntersectsWith(_center, _radius))
				{
					_results[numResult] = c;
					numResult++;
				}
			}
		}
		return numResult;
	}

    /**
	*
	*/
    public Entity[] GetEntitiesIn(Vector2 position, Vector2 dir, float amplitude, float length)
    {
        m_searchList.Clear();
        float halfAmplitude = amplitude / 2.0f;
        float angle = Mathf.Atan2(dir.y, dir.x);
        for (int i = 0; i < m_entities.Count; ++i)
        {
            Entity e = m_entities[i];
            if (e != null)
            {
				if (e.circleArea == null)
            	{
	                Vector2 entityPos = (Vector2)e.transform.position;
	                Vector2 inversePos = entityPos - position;
	                inversePos = inversePos.RotateRadians(-angle);
	                if (inversePos.x >= 0 && inversePos.x <= length)
	                {
	                    if (inversePos.y >= -halfAmplitude && inversePos.y <= halfAmplitude)
	                    {
	                        m_searchList.Add(e);
	                    }
	                }
	          	}else{
					float distanceSqr = e.circleArea.SqrDistanceToSegment( position, position + dir * length);
					if ( distanceSqr <= (amplitude + e.circleArea.radius) * (amplitude + e.circleArea.radius) )
					{
						m_searchList.Add(e);
					}
	          	}
            }
        }

        return m_searchList.ToArray();
    }

	public int GetEntitiesInNonAlloc(Vector2 position, Vector2 dir, float amplitude, float length, Entity[] _results)
    {
		int numResult = 0;

        float halfAmplitude = amplitude / 2.0f;
        float angle = Mathf.Atan2(dir.y, dir.x);

		int size = m_entities.Count;
		int resultsLength = _results.Length;

		for (int i = 0; i < size && numResult < resultsLength; ++i)
        {
            Entity e = m_entities[i];
            if (e != null)
            {
            	if (e.circleArea == null)
            	{
	                Vector2 entityPos = (Vector2)e.transform.position;
	                Vector2 inversePos = entityPos - position;
	                inversePos = inversePos.RotateRadians(-angle);
	                if (inversePos.x >= 0 && inversePos.x <= length)
	                {
	                    if (inversePos.y >= -halfAmplitude && inversePos.y <= halfAmplitude)
	                    {
							_results[numResult] = e;
							numResult++;
	                    }
	                }
                }
                else
                {
					float distanceSqr = e.circleArea.SqrDistanceToSegment( position, position + dir * length);
					if ( distanceSqr <= (amplitude + e.circleArea.radius) * (amplitude + e.circleArea.radius) )
					{
						_results[numResult] = e;
						numResult++;
					}
                }
            }
        }

        return numResult;



        // 1 seach closest line point
        // Check distance to center?
    }


	public int GetOverlapingCages(Vector3 position, float distance, Cage[] results)
    {
		int numResult = 0;
		int size = m_cages.Count;
		int length = results.Length;
        for (int i = 0; i < size && numResult < length; ++i)
        {
			Cage e = m_cages[i];
            if (e != null)
            {
				float sqrMagnitude = (position - e.behaviour.centerTarget.position).sqrMagnitude;
				if ( sqrMagnitude <= distance * distance )
                {
                    results[numResult] = e;
                    numResult++;
                }
            }
        }
        return numResult;
    }


    public int GetOverlapingEntities(Vector3 position, float distance, Entity[] result)
    {
        int numEntities = 0;
        switch (m_overlapingMethod)
        {
            case OverlapingMethod.EntitiesManager:
                {
                    numEntities = GetEntitiesInRange2DNonAlloc(position, distance, result);
                }
                break;
            case OverlapingMethod.Capsule:
                {
                    Vector3 start = position;
                    start.z = -10;
                    Vector3 end = position;
                    end.z = 10;
                    int entities = Physics.OverlapCapsuleNonAlloc(start, end, distance, m_checkEntityColliders, m_entitiesColliderMask);

                    for (int i = 0; i < entities && numEntities < result.Length; ++i)
                    {
                        Entity entity = m_checkEntityColliders[i].attachedRigidbody.GetComponent<Entity>();
                        if (entity != null)
                        {
                            result[numEntities] = entity;
                            numEntities++;
                        }
                    }
                }
                break;
            case OverlapingMethod.Box:
                {
                    int entities = Physics.OverlapBoxNonAlloc(position, Vector3.one * distance * 0.5f + Vector3.forward * 10, m_checkEntityColliders, Quaternion.identity, m_entitiesColliderMask);

                    for (int i = 0; i < entities && numEntities < result.Length; ++i)
                    {
                        Entity entity = m_checkEntityColliders[i].attachedRigidbody.GetComponent<Entity>();
                        if (entity != null)
                        {
                            result[numEntities] = entity;
                            numEntities++;
                        }
                    }
                }
                break;
        }
        return numEntities;
    }

    public int GetOnScreenEntities(Entity[] results) {
        int numResults = 0;
        int maxResults = results.Length;
        int count = m_entities.Count;

        for (int i = 0; i < count && numResults < maxResults; ++i) {
            Entity e = m_entities[i];
            if (e != null && e.isOnScreen) {
                results[numResults] = e;
                numResults++;
            }
        }

        return numResults;
    }

    void Update()
	{
        if (m_updateEnabled) {
            int i;
            int count;
            float delta = Time.deltaTime;

            count = m_pets.Count - 1;
            for (i = count; i >= 0; i--) {
                m_pets[i].CustomUpdate();
            }

            if (m_entities != null) {
                count = m_entities.Count - 1;
                // for (i = 0; i < count; ++i)
                for (i = count; i >= 0; i--) {
                    m_entities[i].CustomUpdate();
                }

                count = m_entitiesBg.Count - 1;
                // for (i = 0; i < count; ++i)
                for (i = count; i >= 0; i--) {
                    m_entitiesBg[i].CustomUpdate();
                }

                count = m_cages.Count - 1;
                // for (i = 0; i < count; ++i)
                for (i = count; i >= 0; i--) {
                    m_cages[i].CustomUpdate();
                }
            }


            FreezingObjectsRegistry.instance.CustomUpdate();

#if UNITY_EDITOR
            if (Input.GetKey(KeyCode.G)) {
                ForceOnScreenEntitiesGolden();
            }
#endif
        }
    }

    void FixedUpdate()
    {
        if (m_updateEnabled) {
            int i;
            int count = m_pets.Count - 1;
            for (i = count; i >= 0; i--) {
                m_pets[i].CustomFixedUpdate();
            }

            if (m_entities != null) {
                count = m_entities.Count - 1;
                for (i = count; i >= 0; i--) {
                    m_entities[i].CustomFixedUpdate();
                }

                count = m_entitiesBg.Count - 1;
                for (i = count; i >= 0; i--) {
                    m_entitiesBg[i].CustomFixedUpdate();
                }

                count = m_cages.Count - 1;
                for (i = count; i >= 0; i--) {
                    m_cages[i].CustomFixedUpdate();
                }
            }
        }
    }

    void LateUpdate()
    {
        if (m_updateEnabled) {
            GameCamera camera = InstanceManager.gameCamera;
            if (camera != null) {
                int i;
                int count = m_entities.Count;
                // Inverse loop because the current entity could be deleted from the list if it's disabled
                for (i = count - 1; i > -1; i--) {
                    if (m_entities[i].CanDieOutsideFrustrum() && camera.IsInsideDeactivationArea(m_entities[i].machine.position)) {
                        m_entities[i].Disable(false);
                    }
                }

                count = m_entitiesBg.Count;
                // Inverse loop because the current entity could be deleted from the list if it's disabled
                for (i = count - 1; i > -1; i--) {
                    if (m_entitiesBg[i].CanDieOutsideFrustrum() && camera.IsInsideBackgroundDeactivationArea(m_entitiesBg[i].machine.position)) {
                        m_entitiesBg[i].Disable(false);
                    }
                }

                count = m_cages.Count;
                // Inverse loop because the current entity could be deleted from the list if it's disabled
                for (i = count - 1; i > -1; i--) {
                    if (m_cages[i].CanDieOutsideFrustrum() && camera.IsInsideDeactivationArea(m_cages[i].transform.position)) //cages don't have machine
                    {
                        m_cages[i].Disable(false);
                    }
                }
            }
        }
    }

	void OnGameEnded() {
        if (m_pets != null)         { m_pets.Clear(); }
        if (m_entities != null)     { m_entities.Clear(); }
		if (m_entitiesBg != null)   { m_entitiesBg.Clear(); }
		if ( m_cages != null )      { m_cages.Clear(); }
	}

    #region debug
    private bool m_entitiesVisibility = true;
    public bool Debug_EntitiesVisibility 
    {
        get
        {
            return m_entitiesVisibility;
        }

        set
        {
            m_entitiesVisibility = value;

            int count;
            if (m_entities != null)
            {
                count = m_entities.Count;
                for (int i = 0; i < count; ++i)
                {
                    //m_entities[i].gameObject.SetActive( m_entitiesAreEnabled);
                    Debug_SetEntityVisible(m_entities[i], m_entitiesVisibility);
                }
            }

            if (m_entitiesBg != null)
            {
                count = m_entitiesBg.Count;
                for (int i = 0; i < count; ++i)
                {
                    //m_entitiesBg[i].gameObject.SetActive(m_entitiesAreEnabled);
                    Debug_SetEntityVisible(m_entitiesBg[i], m_entitiesVisibility);
                }
            }

            if (m_cages != null)
            {
                count = m_cages.Count;
                for (int i = 0; i < count; ++i)
                {
                    //m_cages[i].gameObject.SetActive(m_entitiesAreEnabled);
                    Debug_SetEntityVisible(m_cages[i], m_entitiesVisibility);
                }
            }
        }
    }

    private void Debug_SetEntityVisible(IEntity e, bool value)
    {
        if (e != null)
        {
            Transform child;
            Transform t = e.transform;
            int count = t.childCount;
            for (int i = 0; i < count; ++i)
            {
                child = t.GetChild(i);
                child.gameObject.SetActive(value);
            }
        }
    }


    public void ForceOnScreenEntitiesGolden()
    {
		int count = m_entities.Count;
        // Inverse loop because the current entity could be deleted from the list if it's disabled
        for (int i = count - 1; i > -1; i--)
        {
			if ( m_entities[i].isOnScreen )
            {
                m_entities[i].ForceGolden();
            }
        }
    }

    // Check if alive and not dying prior to force golden

    #endregion
}
