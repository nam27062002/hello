using UnityEngine;
using System.Collections;
using AI;

public class SiegeEngineOperatorSpawner : MonoBehaviour, ISpawner {	

	private enum LookAtVector {
		Right = 0,
		Left,
		Forward,
		Back
	};

	//-------------------------------------------------------------------
	[SerializeField] private string m_entityPrefabStr;
	[SerializeField] private Transform m_spawnAtTransform;
	[SerializeField] private LookAtVector m_lookAtVector;
	//-------------------------------------------------------------------

	private AreaBounds m_areaBounds = new RectAreaBounds(Vector3.zero, Vector3.one);

	private Rect m_rect;
	public Rect boundingRect 			{ get { return m_rect; } }
	public AreaBounds area 				{ get { return m_areaBounds; } set { m_areaBounds = value; } }
	public IGuideFunction guideFunction	{ get { return null; } }

	private GameCamera m_newCamera;
	private Machine m_operator;
	private Pilot m_operatorPilot;

	private AutoSpawnBehaviour m_autoSpawner;


	//-------------------------------------------------------------------
	// Init
	//-------------------------------------------------------------------
	void Awake() {
		m_autoSpawner = GetComponent<AutoSpawnBehaviour>();

		m_operator = null;
		m_operatorPilot = null;
	}

	void Start() {
		m_rect = new Rect((Vector2)transform.position, Vector2.zero);

		m_newCamera = Camera.main.GetComponent<GameCamera>();

		// TODO[MALH]: Get path relative to quality version
		PoolManager.CreatePool(m_entityPrefabStr, IEntity.ENTITY_PREFABS_PATH + m_entityPrefabStr, 1, true);
	}
		
	//
	public void Initialize() {
	}
	//-------------------------------------------------------------------


	//-------------------------------------------------------------------
	// Spawn stuff
	//-------------------------------------------------------------------
	public void ForceRemoveEntities() {
		if (m_operator != null) {
			PoolManager.ReturnInstance(m_operator.gameObject);
			m_operator = null;
			m_operatorPilot = null;
		}
	}

	public void RemoveEntity(GameObject _entity, bool _killedByPlayer) {
		if (m_operator != null) {			
			PoolManager.ReturnInstance(m_operator.gameObject);
			m_operator = null;
			m_operatorPilot = null;
		}
	}
	
	public bool CanRespawn() {
		if (m_autoSpawner.state == AutoSpawnBehaviour.State.Idle) {
			if (IsOperatorDead()) {
				return m_newCamera.IsInsideDeactivationArea(m_spawnAtTransform.position);
			}
		}
		return false;
	}

	public bool Respawn() {
		if (m_operator == null) {
			GameObject spawning = PoolManager.GetInstance(m_entityPrefabStr);

			if (spawning != null) {
				Transform groundSensor = spawning.transform.FindChild("groundSensor");
				spawning.transform.position = m_spawnAtTransform.position;
				if (groundSensor != null) {
					spawning.transform.position += (spawning.transform.position - groundSensor.position);
				}

				Entity entity = spawning.GetComponent<Entity>();
				if (entity != null) {
					entity.Spawn(this); // lets spawn Entity component first
				}

				AI.AIPilot pilot = spawning.GetComponent<AI.AIPilot>();
				if (pilot != null) {
					pilot.Spawn(this);
				}

				ISpawnable[] components = spawning.GetComponents<ISpawnable>();
				foreach (ISpawnable component in components) {
					if (component != entity && component != pilot) {
						component.Spawn(this);
					}
				}

				m_operator = spawning.GetComponent<Machine>();
				m_operatorPilot = spawning.GetComponent<Pilot>();

				spawning.transform.rotation = m_spawnAtTransform.rotation;//Quaternion.LookRotation(GetLookAtVector());
			}
		}

		return true; 
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

	public void OperatorDoShoot() {
		m_operatorPilot.PressAction(Pilot.Action.Button_B);
		m_operatorPilot.ReleaseAction(Pilot.Action.Button_A);
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

	public void DrawStateGizmos() {}
	//-------------------------------------------------------------------
}
