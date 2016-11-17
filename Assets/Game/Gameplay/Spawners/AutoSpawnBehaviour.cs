using UnityEngine;
using System.Collections;

public class AutoSpawnBehaviour : MonoBehaviour, ISpawner {
	//-----------------------------------------------
	// Constants
	//-----------------------------------------------
	public enum State {
		Idle = 0,
		Respawning
	};

	
	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	[SerializeField] private float m_spawnTime;


	private State m_state;
	public State state
	{
		get
		{
			return m_state;
		}
	}

	private float m_respawnTime;

	private Bounds m_bounds; // view bounds
	private Rect m_rect;
	public Rect boundingRect { get { return m_rect; } }

	// Scene referemces
	private GameSceneControllerBase m_gameSceneController = null;

	public AreaBounds area{ get {return null;} }
	public IGuideFunction guideFunction{ get {return null;} }

	private GameCamera m_newCamera;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	void Start() {
		SpawnerManager.instance.Register(this);

		m_newCamera = Camera.main.GetComponent<GameCamera>();
		m_gameSceneController = InstanceManager.GetSceneController<GameSceneControllerBase>();

		GameObject view = transform.FindChild("view").gameObject;
		m_bounds = view.GetComponentInChildren<Renderer>().bounds;

		Vector2 position = (Vector2)m_bounds.min;
		Vector2 size = (Vector2)m_bounds.size;
		Vector2 extraSize = size * (transform.position.z * 4f) / 100f; // we have to increase the size due to z depth

		m_rect = new Rect(position - extraSize * 0.5f, size + extraSize);
	}

	public void Initialize() {
		m_state = State.Idle;
	}    

    public void ForceRemoveEntities() {

	}

	public void StartRespawn() {
		// Program the next spawn time
		m_respawnTime = m_gameSceneController.elapsedSeconds + m_spawnTime;
		m_state = State.Respawning;
	}        

    public bool CanRespawn() {
		if (m_state == State.Respawning) {
			if(m_gameSceneController.elapsedSeconds > m_respawnTime) {
				bool isInsideActivationArea = m_newCamera.IsInsideActivationArea(m_bounds);
				//bool isInsideActivationArea = m_newCamera.IsInsideActivationArea(transform.position);	
				if (isInsideActivationArea) {
					return true;
				}
			}
		}

		return false;
	}

	public bool Respawn() {
		Spawn();
		return true;
	}
		
	private void Spawn() {
		Initializable[] components = GetComponents<Initializable>();
		foreach (Initializable component in components) {
			component.Initialize();
		}
		m_state = State.Idle;
	}

	public void RemoveEntity(GameObject _entity, bool _killedByPlayer) {}

	public void DrawStateGizmos() {}
}