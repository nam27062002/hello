﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AI;

public class PetDogSpawner : AbstractSpawner {	

	private enum LookAtVector {
		Right = 0,
		Left,
		Forward,
		Back
	};

	//-------------------------------------------------------------------
	[SerializeField] private string m_entityPrefabStr;
	[SerializeField] public Range m_spawnTime = new Range(40f, 45f);
	[SerializeField] private Transform m_spawnAtTransform;
	[SerializeField] private bool m_mustBeChild = false;
	[SerializeField] private LookAtVector m_lookAtVector;
	//-------------------------------------------------------------------	

	private Machine m_operator;
	private Pilot m_operatorPilot;
	public Pilot operatorPilot
	{
		get { return m_operatorPilot; }
	}

	public List<string> m_possibleSpawners;

    void Awake()
    {
        m_operator = null;
        m_operatorPilot = null;
    }

	void OnDestroy() {
		if ( ApplicationManager.IsAlive )
		{
			ForceRemoveEntities();
		}
	}
    //-------------------------------------------------------------------

    //-------------------------------------------------------------------
    // AbstractSpawner implementation
    //-------------------------------------------------------------------	

    private AreaBounds m_areaBounds = new RectAreaBounds(Vector3.zero, Vector3.one);
    public override AreaBounds area { get { return m_areaBounds; } set { m_areaBounds = value; } }

    protected override void OnStart() {
        // Progressive respawn disabled because it respawns only one instance and it's triggered by Catapult which is not prepared to loop until Respawn returns true
        UseProgressiveRespawn = false;
        UseSpawnManagerTree = false;
        RegisterInSpawnerManager();        

		for( int i = 0; i<m_possibleSpawners.Count; i++ )
		{
			string entityPrefabPath = IEntity.EntityPrefabsPath + m_possibleSpawners[i];        
			PoolManager.CreatePool(m_possibleSpawners[i], entityPrefabPath, 1, true);
		}
    }

    protected override uint GetMaxEntities() {
        return 1;
    }

    protected override uint GetEntitiesAmountToRespawn() {        
        return GetMaxEntities();
    }        

    protected override string GetPrefabNameToSpawn(uint index) {
        return m_entityPrefabStr;
    }    

    public void RamdomizeEntity()
    {
    	m_entityPrefabStr = m_possibleSpawners[ Random.Range(0, m_possibleSpawners.Count) ];
    }

    protected override void OnEntitySpawned(GameObject spawning, uint index, Vector3 originPos) {
        Transform groundSensor = spawning.transform.FindChild("groundSensor");
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

    protected override void OnMachineSpawned(AI.Machine machine) {
        m_operator = machine;
    }

    protected override void OnPilotSpawned(AI.Pilot pilot) {
        m_operatorPilot = pilot;
    }    

    protected override void OnRemoveEntity(GameObject _entity, int index) {
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

	public void OperatorBurn() {
		m_operator.Burn(transform);
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
