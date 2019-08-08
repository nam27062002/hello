using UnityEngine;
using System.Collections.Generic;
using AI;

public class DeviceOperatorSpawner : AbstractSpawner {	

	private enum LookAtVector {
		Right = 0,
		Left,
		Forward,
		Back
	};

	//-------------------------------------------------------------------
	[EntityPrefabListAttribute]
	[SerializeField] private string m_entityPrefabStr;
	[SerializeField] public Range m_spawnTime = new Range(40f, 45f);
	[SerializeField] private Transform m_spawnAtTransform;
	[SerializeField] private bool m_mustBeChild = false;
	[SerializeField] private LookAtVector m_lookAtVector;
	//-------------------------------------------------------------------	

	private PoolHandler m_poolHandler;

	private GameCamera m_newCamera;
    private IEntity m_operatorEntity;
    private Pilot m_operatorPilot;
    private IMachine m_operator;	
	private Transform m_operatorParent;

	private float m_respawnTime;

	// Scene referemces
	private GameSceneControllerBase m_gameSceneController = null;
	private AutoSpawnBehaviour m_autoSpawner;

    void Awake() {
        m_autoSpawner = GetComponent<AutoSpawnBehaviour>();

        m_operator = null;
        m_operatorEntity = null;
        m_operatorPilot = null;
		m_operatorParent = null;
    }

	protected override void OnDestroy() {
		base.OnDestroy();
		if (ApplicationManager.IsAlive && InstanceManager.gameSceneController != null && InstanceManager.gameSceneController.state <= GameSceneController.EStates.RUNNING) {
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

    protected override void OnStart() {
		Area area = GetComponent<Area>();
		if (area != null) {
			m_areaBounds = area.bounds;
		}

        // Progressive respawn disabled because it respawns only one instance and it's triggered by Catapult which is not prepared to loop until Respawn returns true
        UseProgressiveRespawn = false;        
        UseSpawnManagerTree = false;        
        RegisterInSpawnerManager();        

        m_newCamera = Camera.main.GetComponent<GameCamera>();

		m_respawnTime = -1;

		m_gameSceneController = InstanceManager.gameSceneControllerBase;
		
		m_poolHandler = PoolManager.RequestPool(m_entityPrefabStr, (int)GetMaxEntities());
    }

    protected override uint GetMaxEntities() {
        return 1;
    }

    protected override bool CanRespawnExtended() {
        if (m_autoSpawner.state == AutoSpawnBehaviour.State.Idle) {
            if (IsOperatorDead()) {
				if (m_gameSceneController.elapsedSeconds > m_respawnTime) {
					if (m_newCamera != null) {
						return  m_newCamera.IsInsideActivationMaxArea(m_spawnAtTransform.position)
                            && !m_newCamera.IsInsideActivationMinArea(m_spawnAtTransform.position);
					}
				}
            }
        }
        return false;
    }

    protected override uint GetEntitiesAmountToRespawn() {
        return GetMaxEntities();
    }

	protected override PoolHandler GetPoolHandler(uint index) {
		return m_poolHandler;
	}

    protected override string GetPrefabNameToSpawn(uint index) {
        return m_entityPrefabStr;
    }    

	protected override void OnEntitySpawned(IEntity spawning, uint index, Vector3 originPos) {
        m_operatorEntity = spawning;

        Transform groundSensor = spawning.transform.Find("groundSensor");
        Transform t = spawning.transform;
        
		m_operatorParent = t.parent;

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
        if (m_operator != null && _entity == m_operatorEntity) {
            m_operator = null;
            m_operatorPilot = null;

            if (_killedByPlayer) {
                m_respawnTime = m_gameSceneController.elapsedSeconds + m_spawnTime.GetRandom();    
            } else {
                m_respawnTime = 0f;
            }			
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

    public void OperatorDoIdle() {
		if ( m_operatorPilot != null ) {			
			m_operatorPilot.ReleaseAction (Pilot.Action.Button_A);
			m_operatorPilot.ReleaseAction (Pilot.Action.Button_B);
			m_operatorPilot.ReleaseAction (Pilot.Action.Scared);
		}
    }

    public void OperatorDoActionA() {
		if ( m_operatorPilot != null ) {			
			m_operatorPilot.PressAction (Pilot.Action.Button_A);
			m_operatorPilot.ReleaseAction (Pilot.Action.Button_B);
			m_operatorPilot.ReleaseAction (Pilot.Action.Scared);
		}
    }

	public void OperatorDoActionB() {
		if ( m_operatorPilot != null ) {
			m_operatorPilot.PressAction (Pilot.Action.Button_B);
			m_operatorPilot.ReleaseAction (Pilot.Action.Button_A);
			m_operatorPilot.ReleaseAction (Pilot.Action.Scared);
		}
    }

    public void OperatorDoScared() {
        if ( m_operatorPilot != null ){
            m_operatorPilot.PressAction(Pilot.Action.Scared);
            m_operatorPilot.ReleaseAction(Pilot.Action.Button_A);
            m_operatorPilot.ReleaseAction(Pilot.Action.Button_B);
        }
    }

    public void OperatorEnterDevice() {
		m_operator.EnterDevice(false);
	}

	public void OperatorLeaveDevice() {
		if (m_mustBeChild) {
			m_operator.transform.parent = m_operatorParent;
		}
		m_operator.LeaveDevice(false);
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
		Gizmos.color = Colors.WithAlpha(Colors.paleGreen, 0.5f);
		Gizmos.DrawCube(transform.position, m_rect.size);

		if (m_spawnAtTransform != null) {
			Gizmos.color = Colors.lime;
			Gizmos.DrawSphere(m_spawnAtTransform.position, 0.5f);
			Gizmos.DrawCube(m_spawnAtTransform.position + GetLookAtVector() * 0.5f, Vector3.one * 0.125f + GetLookAtVector() * 1f);
		}
	}	
	//-------------------------------------------------------------------
}
