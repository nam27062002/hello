using UnityEngine;
using System.Collections;

public class SpawnBehaviour : MonoBehaviour {
	
	
	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	[SerializeField] private bool m_deactivate = true;

	private AreaBounds m_area;
	private Spawner m_spawner;
	private int m_index;
	public int index { get { return m_index; } }

	private GameCamera m_newCamera;

	private bool m_wasEatenOrBurned;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	void Start() {
		m_newCamera = Camera.main.GetComponent<GameCamera>();
	}

	void OnEnable() {
		//EntityManager.instance.Register(GetComponent<Entity_Old>());
		m_wasEatenOrBurned = false;
	}

	void OnDisable() {
		//if (EntityManager.instance != null)
			//EntityManager.instance.Unregister(GetComponent<Entity_Old>());

		if (m_spawner) {
			m_spawner.RemoveEntity(gameObject, m_wasEatenOrBurned);
			m_spawner = null;
		}
	}

	public void EatOrBurn() {
		m_wasEatenOrBurned = true;
		gameObject.SetActive(false);
	}

	void LateUpdate() {
		
		if (m_deactivate) 
		{
			bool isInsideDeactivationArea = m_newCamera.IsInsideDeactivationArea(transform.position);

			if ( isInsideDeactivationArea )
			{
				if (m_spawner) 
				{
					gameObject.SetActive(false);
				}
			}
		}
	}

	public void Spawn(Spawner _spawner, int _index, Vector3 _position, AreaBounds _bounds) {		
		m_spawner = _spawner;
		m_index = _index;
		m_area = _bounds;
		
		transform.position = _position;

		Initializable[] components = GetComponents<Initializable>();

		foreach (Initializable component in components) {
			component.SetAreaBounds(m_area);
			component.Initialize();
		}
	}
}