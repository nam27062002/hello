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

	private float m_timer;

	private Bounds m_bounds; // view bounds

	public AreaBounds area{ get {return null;} }

	private GameCameraController m_camera;
	private GameCamera m_newCamera;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	void Start() {
		SpawnerManager.instance.Register(this);

		m_camera = Camera.main.GetComponent<GameCameraController>();
		m_newCamera = Camera.main.GetComponent<GameCamera>();

		GameObject viewBurned = transform.FindChild("view_burned").gameObject;
		Collider collider = GetComponent<Collider>();
		if (collider != null) {
			m_bounds = collider.bounds;
		} else {
			m_bounds = viewBurned.GetComponent<Renderer>().bounds;
		}
	}

	public void Initialize() {
		m_state = State.Idle;
	}

	public void ForceRemoveEntities() {

	}

	public void CheckRespawn() {
		if (m_state == State.Respawning) {
			if (m_timer > 0) {
				m_timer -= Time.deltaTime;
				if (m_timer < 0) {
					m_timer = 0;
				}
			} else {
				bool isInsideActivationArea = false;
				if ( DebugSettings.newCameraSystem )
				{
					isInsideActivationArea = m_newCamera.IsInsideActivationArea(m_bounds);
				}
				else
				{
					isInsideActivationArea = m_camera.IsInsideActivationArea(m_bounds);
				}
				if (isInsideActivationArea) 
				{
					Spawn();
				}
			}
		}
	}

	public void Respawn() {
		m_timer = m_spawnTime;
		m_state = State.Respawning;
	}
		
	private void Spawn() {		
		Initializable[] components = GetComponents<Initializable>();
		foreach (Initializable component in components) {
			component.Initialize();
		}
		m_state = State.Idle;
	}

	public void RemoveEntity(GameObject _entity, bool _killedByPlayer){}
}