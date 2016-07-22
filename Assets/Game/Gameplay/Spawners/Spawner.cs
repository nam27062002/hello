using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour, ISpawner {


	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[Separator("Entity")]
	[CommentAttribute("The entities will spawn on the coordinates of the Spawner, and will move inside the defined area.")]
	[SerializeField] public GameObject m_entityPrefab;
	[SerializeField] public RangeInt m_quantity = new RangeInt(1, 1);
	[SerializeField] public Range	 m_scale = new Range(1f, 1f);
	[CommentAttribute("Amount of points obtained after killing the whole flock. Points are multiplied by the amount of entities spawned.")]
	[SerializeField] private int m_flockBonus = 0;

	[Separator("Activation")]
	[Tooltip("For the spawners that must spawn even when the dragon is not near (i.e. the spawners around the start area)")]
	[SerializeField] private bool m_activeOnStart = false;

	[Tooltip("Meant for background spawners, will ignore respawn settings and activation triggers.")]
	[SerializeField] private bool m_alwaysActive = false;
	public bool alwaysActive { get { return m_alwaysActive; }}

	[HideInInspector] [Tooltip("Total seconds of gameplay.\n-1 to not take a value into account.")]
	[SerializeField] private Range m_activationTime = new Range(-1, -1);
	public Range activationTime { get { return m_activationTime; }}

	[HideInInspector] [Tooltip("XP acquired during the game session.\n-1 to not take a value into account")]
	[SerializeField] private Range m_activationXP = new Range(-1, -1);
	public Range activationXP { get { return m_activationXP; }}

	[Separator("Respawn")]
	[SerializeField] private Range m_spawnTime = new Range(40f, 45f);
	[SerializeField] private int m_maxSpawns;
	

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	protected AreaBounds m_area;
	public AreaBounds area { 
		get { 
			if (m_guideFunction != null) {
				return m_guideFunction.GetBounds();
			} else {
				return m_area;
			}
		} 
	}

	protected EntityGroupController m_groupController;
	protected IGuideFunction m_guideFunction;

	private uint m_entityAlive;
	private uint m_entitySpawned;
	private uint m_entitiesKilled; // we'll use this to give rewards if the dragon destroys a full flock
	private bool m_allEntitiesKilledByPlayer;
	protected GameObject[] m_entities; // list of alive entities

	private float m_activationTimer;
	private float m_respawnTimer;
	private uint m_respawnCount;

	private bool m_readyToBeDisabled;

	private GameCameraController m_camera;

	// Level editing stuff
	private bool m_showSpawnerInEditor = true;
	public bool showSpawnerInEditor {
		get { return m_showSpawnerInEditor; }
		set { m_showSpawnerInEditor = value; }
	}
	
	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	protected virtual void Start() {
		m_entities = new GameObject[m_quantity.max];

		PoolManager.CreatePool(m_entityPrefab, Mathf.Max(15, m_entities.Length), true);

		m_camera = GameObject.Find("PF_GameCamera").GetComponent<GameCameraController>();

		m_area = GetArea();

		m_groupController = GetComponent<EntityGroupController>();
		if (m_groupController) {
			m_groupController.Init(m_quantity.max);
		}

		m_guideFunction = GetComponent<IGuideFunction>();

		SpawnerManager.instance.Register(this);

		gameObject.SetActive(false);
	}

	public void Initialize() {
		m_activationTimer = 0;

		m_respawnTimer = 0;
		m_respawnCount = 0;

		m_entityAlive = 0;
		m_entitySpawned = 0;
		m_entitiesKilled = 0;

		m_allEntitiesKilledByPlayer = false;

		m_readyToBeDisabled = false;

		// [AOC] Safecheck: If both start conditions are set to -1 (they shouldn't, wrong content), switch initial time to 0 so everything works ok
		if(m_activationXP.min < 0 && m_activationTime.min < 0) m_activationTime.min = 0;

		if (m_activeOnStart) {
			Spawn();
		}
	}

	public void ResetSpawnTimer()
	{
		m_respawnTimer = 0;
	}

	public void ForceRemoveEntities() {
		for (int i = 0; i < m_entitySpawned; i++) {			
			if (m_entities[i] != null) {
				PoolManager.ReturnInstance(m_entities[i]);
				m_entities[i] = null;
			}
		}

		m_entityAlive = 0;
		m_respawnTimer = 0;
		m_allEntitiesKilledByPlayer = false;
	}

	// entities can remove themselves when are destroyed by the player or auto-disabled when are outside of camera range
	public void RemoveEntity(GameObject _entity, bool _killedByPlayer) {
		bool found = false;
		for (int i = 0; i < m_entitySpawned; i++) {			
			if (m_entities[i] == _entity) 
			{
				PoolManager.ReturnInstance( m_entities[i] );
				m_entities[i] = null;
				if (_killedByPlayer) {
					m_entitiesKilled++;
				}
				m_entityAlive--;
				found = true;
			}
		}

		if (found) {
			// all the entities are dead
			if (m_entityAlive == 0) {
				m_allEntitiesKilledByPlayer = m_entitiesKilled == m_entitySpawned;

				if (m_allEntitiesKilledByPlayer) {
					// check if player has destroyed all the flock
					if (m_flockBonus > 0) {
						Reward reward = new Reward();
						reward.score = (int)(m_flockBonus * m_entitiesKilled);
						Messenger.Broadcast<Transform, Reward>(GameEvents.FLOCK_EATEN, _entity.transform, reward);
					}
				} else {
					m_respawnTimer = 0; // instant respawn, because player didn't kill all the entities
				}

				if (m_readyToBeDisabled) {
					SpawnerManager.instance.Unregister(this);
				}
			}
		}
	}
		
	public void UpdateTimers() {		
		// Ignore all logic for always active spawners
		if (m_alwaysActive) {
			if (m_entityAlive == 0) {
				Spawn();
			}
		}

		// Rest of the spawners
		else {
			// Update timer
			m_activationTimer += Time.deltaTime;

			// If we can spawn, do it
			if(CanSpawn(m_activationTimer, RewardManager.xp)) {
				// If we don't have any entity alive, proceed
				if(m_entityAlive == 0) {
					// Respawn on cooldown?
					if(m_respawnTimer > 0) {
						m_respawnTimer -= Time.deltaTime;
						if(m_respawnTimer <= 0) m_respawnTimer = 0;
					} else {
						// Check activation area
						if(m_camera != null && m_camera.IsInsideActivationArea(transform.position)) {
							Spawn();
						}
					}
				}
			}

			// If we can't spawn and we're ready to be disabled, wait untill all entities are dead to do it
			else if(m_readyToBeDisabled) {
				if(m_entityAlive == 0) {
					SpawnerManager.instance.Unregister(this);
				}
			}
		}
	}

	public void UpdateLogic() {
		ExtendedUpdateLogic();
	}

	/// <summary>
	/// Check all the required conditions (time, xp) to determine whether this spawner can spawn or not.
	/// Doesn't check respawn timer nor activation area, only time and xp constraints.
	/// </summary>
	/// <returns>Whether this spawner can spawn or not.</returns>
	/// </returns><param name="_time">Elapsed game time.</param>
	/// </returns><param name="_xp">Earned xp.</param>
	public bool CanSpawn(float _time, float _xp) {
		// If always active, we're done!
		if(m_alwaysActive) return true;

		// If already ready to be disabled, no need for further checks
		if(m_readyToBeDisabled) return false;

		// Check start conditions
		bool startThresholdOk = (m_activationTime.min >= 0 && _time > m_activationTime.min)	// We have an activation time (>= 0) and we've reached the threshold
							 || (m_activationXP.min >= 0 && _xp > m_activationXP.min);		// We have a minimum activation XP (>= 0) and we have earned enough XP

		// If start conditions aren't met, we can't spawn, no need to check anything else
		if(!startThresholdOk) {
			return false;
		}

		// Check end conditions
		bool endThresholdOk = (m_activationTime.max < 0 || _time < m_activationTime.max)	// Either we don't have an end time (-1) or we haven't yet reached the threshold
						   && (m_activationXP.max < 0 || _xp < m_activationXP.max);			// Either we don't have a XP limit (-1) or we haven't yet reach it

		// If we've reached either of the end conditions, mark the spawner as ready to disable
		if(!endThresholdOk && Application.isPlaying) {
			m_readyToBeDisabled = true;
		}

		return endThresholdOk;
	}

	public void Respawn() {

	}

	private void Spawn() {
		if (m_entitiesKilled == m_entitySpawned) {
			m_entitySpawned = (uint)m_quantity.GetRandom();
		} else {
			// If player didn't killed all the spawned entities we'll re spawn only the remaining alive.
			// Also, this respawn will be instant.
			m_entitySpawned = m_entitySpawned - m_entitiesKilled;
		}

		for (int i = 0; i < m_entitySpawned; i++) {			
			m_entities[i] = PoolManager.GetInstance(m_entityPrefab.name);
			m_entityAlive++;
		}
		m_entitiesKilled = 0;

		ExtendedSpawn();

		for (int i = 0; i < m_entitySpawned; i++) {			
			Entity entity = m_entities[i].GetComponent<Entity>();
			AI.Pilot pilot = m_entities[i].GetComponent<AI.Pilot>();
			pilot.guideFunction = m_guideFunction;

			Vector3 pos = transform.position;
			if (m_guideFunction != null) {
				m_guideFunction.ResetTime();
				pos = m_guideFunction.NextPositionAtSpeed(0);
			}

			if (i > 0) {
				pos += RandomStartDisplacement(); // don't let multiple entities spawn on the same point
			}

			entity.transform.position = pos;
			entity.transform.localScale = Vector3.one * m_scale.GetRandom();

			entity.Spawn(this); // lets spawn Entity component first
			ISpawnable[] components = entity.GetComponents<ISpawnable>();
			foreach (ISpawnable component in components) {
				if (component != entity) {
					component.Spawn(this);
				}
			}
		}

		if (m_groupController) {			
			for (int i = 0; i < m_entities.Length; i++) {
				if (m_entities[i] != null) {
					AI.Machine m = m_entities[i].GetComponent<AI.Machine>();
					m.EnterGroup(ref m_groupController.flock);
				}
			}
		}

		// Disable this spawner after a number of spawns
		if (m_allEntitiesKilledByPlayer && m_maxSpawns > 0) {
			m_respawnCount++;
			if (m_respawnCount == m_maxSpawns) {
				gameObject.SetActive(false);
				SpawnerManager.instance.Unregister(this);
			}
		}

		m_allEntitiesKilledByPlayer = false;

		m_respawnTimer = m_spawnTime.GetRandom();
	}

	protected virtual void ExtendedSpawn() {}
	protected virtual void ExtendedUpdateLogic() {}

	protected virtual AreaBounds GetArea() {
		Area area = GetComponent<Area>();
		if (area != null) {
			return area.bounds;
		} else {
			// spawner for static objects with a fixed position
			return new CircleAreaBounds(transform.position, 1f);
		}
	}

	void OnDrawGizmos() {
		// Only if editor allows it
		if(showSpawnerInEditor) {
			// Draw spawn area
			GetArea().DrawGizmo();

			// Draw icon! - only in editor!
			#if UNITY_EDITOR
				// Icons are stored in the Gizmos folder in the project root (Unity rules), and have the same name as the entities
				Gizmos.DrawIcon(transform.position, "Spawners/" + this.m_entityPrefab.name, true);
			#endif
		}
	}

	virtual protected Vector3 RandomStartDisplacement()
	{
		return Random.onUnitSphere * 2f;
	}
}
