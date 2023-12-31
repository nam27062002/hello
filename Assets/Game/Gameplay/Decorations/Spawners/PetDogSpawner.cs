﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AI;

public class PetDogSpawner : AbstractSpawner, IBroadcastListener  {	

	private enum LookAtVector {
		Right = 0,
		Left,
		Forward,
		Back
	};

	//-------------------------------------------------------------------
	[SerializeField] private string m_entityPrefabStr;
	private int m_entityPrefabIndex;
	[SerializeField] public Range m_spawnTime = new Range(40f, 45f);
	[SerializeField] private Transform m_spawnAtTransform;
	[SerializeField] private bool m_mustBeChild = false;
	[SerializeField] private LookAtVector m_lookAtVector;

	//-------------------------------------------------------------------	
	private IEntity m_operatorEntity;
	public IEntity operatorEntity
	{
		get { return m_operatorEntity; }
	}
	private IMachine m_operator;
	private Pilot m_operatorPilot;
	public Pilot operatorPilot
	{
		get { return m_operatorPilot; }
	}

	[System.Serializable]
	public class SpawnerChances
	{
		public string m_spawnPrefab;
		public float m_chance = 1;
		public SpawnerChances()
		{
			m_spawnPrefab = "";
			m_chance = 1;
		}
	}

	public List<SpawnerChances> m_possibleSpawners;

	private PoolHandler[] m_poolHandlers;
	private SpawnerChances[]  m_validSpanwerChances;

	private float m_maxChance;
    void Awake()
    {
        m_operator = null;
        m_operatorPilot = null;

		// Register change area events
		Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
		Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);
    }

	override protected void OnDestroy() {
		base.OnDestroy();
		Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);

		if ( ApplicationManager.IsAlive )
		{
			ForceRemoveEntities();
		}
	}

    public override List<string> GetPrefabList() {
        return null;
    }
    //-------------------------------------------------------------------

    //-------------------------------------------------------------------
    // AbstractSpawner implementation
    //-------------------------------------------------------------------	

    private AreaBounds m_areaBounds = new RectAreaBounds(Vector3.zero, Vector3.one);
    public override AreaBounds area { get { return m_areaBounds; } set { m_areaBounds = value; } }

	public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
			case BroadcastEventType.GAME_AREA_ENTER:
            case BroadcastEventType.GAME_LEVEL_LOADED:
            {
				PreparePools();
				ForceReset();
            }break;
        }
    }

	void PreparePools()
	{
		List<SpawnerChances> listValidSpanwer = new List<SpawnerChances>();
		List<PoolHandler> listValidHandlers = new List<PoolHandler>();
		for (int i = 0; i<m_possibleSpawners.Count; i++) {
			string prefab = m_possibleSpawners[i].m_spawnPrefab;
			PoolHandler handle = PoolManager.GetHandler(prefab);
			if ( handle != null ) 
			{
				listValidSpanwer.Add( m_possibleSpawners[i] );
				listValidHandlers.Add(handle);
			}
		}
		m_validSpanwerChances = listValidSpanwer.ToArray();
		m_poolHandlers = listValidHandlers.ToArray();
		CalculateMaxChance();
	}

    protected override void OnStart() {
        // Progressive respawn disabled because it respawns only one instance and it's triggered by Catapult which is not prepared to loop until Respawn returns true
        UseProgressiveRespawn = false;
        UseSpawnManagerTree = false;
        RegisterInSpawnerManager();
		PreparePools();
    }

    private void CalculateMaxChance()
    {
    	m_maxChance = 0;
		if ( m_validSpanwerChances != null )
		{
			for( int i = 0; i<m_validSpanwerChances.Length; i++ )
			{
				m_maxChance += m_validSpanwerChances[i].m_chance;
			}
		}
    }

    protected override uint GetMaxEntities() {
        return 1;
    }

    protected override uint GetEntitiesAmountToRespawn() {        
        return GetMaxEntities();
    }        

	protected override PoolHandler GetPoolHandler(uint index) {
		return m_poolHandlers[m_entityPrefabIndex];
	}

    protected override string GetPrefabNameToSpawn(uint index) {
        return m_entityPrefabStr;
    }    

    public string GetSelectedPrefabStr()
    {
    	return m_entityPrefabStr;
    }

    public void RamdomizeEntity()
    {
   #if UNITY_EDITOR
   		// CalculateMaxChance();
   #endif
		float chance = Random.Range(0.0f, m_maxChance);
		float currentChance = 0;
		for( int i = 0; i<m_validSpanwerChances.Length; i++ )
		{
			currentChance += m_validSpanwerChances[i].m_chance;
			if (chance <= currentChance )
			{
				m_entityPrefabStr = m_validSpanwerChances[ i ].m_spawnPrefab;
				m_entityPrefabIndex = i;
				break;
			}
		}
    }


	protected override void OnEntitySpawned(IEntity spawning, uint index, Vector3 originPos) {
		m_operatorEntity = spawning;
        Transform groundSensor = spawning.transform.Find("groundSensor");
        Transform t = spawning.transform;
        
		if (m_mustBeChild) {
			t.parent = m_spawnAtTransform;
			t.localPosition = Vector3.zero;
			t.localRotation = Quaternion.identity;
		} else {
			t.position = m_spawnAtTransform.position;
			t.rotation = m_spawnAtTransform.rotation;
		}

		if (groundSensor != null) {
			t.position -= groundSensor.localPosition;
		}
		t.localScale = Vector3.one;
    }

    protected override void OnMachineSpawned(AI.IMachine machine, uint index) {
        m_operator = machine;
    }

    protected override void OnPilotSpawned(AI.Pilot pilot) {
        m_operatorPilot = pilot;
    }    

	protected override void OnRemoveEntity(IEntity _entity, int index, bool _killedByPlayer) {
        if (m_operator != null && _entity == m_operator.gameObject) {
            m_operator = null;
            m_operatorPilot = null;
        }
    }    
    //-------------------------------------------------------------------


    //-------------------------------------------------------------------
    // Queries
    //-------------------------------------------------------------------
    public bool IsOperatorDead() {
		if (m_operator != null) {
			return m_operator.IsDying() || m_operator.IsDead();
		}
		return true;
	}

	public void OperatorDoReload() {
		m_operatorPilot.PressAction(Pilot.Action.Button_A);
		m_operatorPilot.ReleaseAction(Pilot.Action.Button_B);
	}

	public void OperatorDoIdle() {
		m_operatorPilot.ReleaseAction(Pilot.Action.Button_A);
		m_operatorPilot.ReleaseAction(Pilot.Action.Button_B);
	}

	public void OperatorDoShoot() {
		m_operatorPilot.PressAction(Pilot.Action.Button_B);
		m_operatorPilot.ReleaseAction(Pilot.Action.Button_A);
	}

	public void OperatorBurn(IEntity.Type _source) {
		m_operator.Burn(transform, _source);
	}

	private Vector3 GetLookAtVector() {
		Vector3 lookAt = Vector3.zero;
		switch(m_lookAtVector) {
			case LookAtVector.Right:	lookAt = Vector3.right; 	break;
			case LookAtVector.Left:		lookAt = Vector3.left; 		break;
			case LookAtVector.Forward:	lookAt = Vector3.forward; 	break;
			case LookAtVector.Back:		lookAt = Vector3.back; 		break;
		}
		return lookAt;
	}
	//-------------------------------------------------------------------


	//-------------------------------------------------------------------
	// Debug
	//-------------------------------------------------------------------
	void OnDrawGizmosSelected() {
		if (m_spawnAtTransform != null) {
			Gizmos.color = Colors.coral;
			Gizmos.DrawSphere(m_spawnAtTransform.position, 0.5f);
			Gizmos.DrawCube(m_spawnAtTransform.position + GetLookAtVector() * 0.5f, Vector3.one * 0.125f + GetLookAtVector() * 1f);
		}
	}	
	//-------------------------------------------------------------------
}
