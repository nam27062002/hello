using UnityEngine;
using System.Collections.Generic;

// This is a Quadtree! a Quadtree full of fires
public class FirePropagationManager : UbiBCN.SingletonMonoBehaviour<FirePropagationManager> {
	
	[SerializeField] private float m_checkFireTime = 0.25f;

	private QuadTree<FireNode> m_fireNodesTree;
	private List<FireNode> m_fireNodes;
	private AudioSource m_fireNodeAudio;
	private DragonBreathBehaviour m_breath;
		
	private float m_timer;

	public List<Transform> m_burningFireNodes = new List<Transform>();


	void Awake() {
		m_fireNodesTree = new QuadTree<FireNode>(-600f, -100f, 1000f, 400f);
		m_fireNodes = new List<FireNode>();

		m_fireNodeAudio = gameObject.AddComponent<AudioSource>();
		m_fireNodeAudio.playOnAwake = false;
		m_fireNodeAudio.loop = false;
		m_fireNodeAudio.clip = Resources.Load("audio/sfx/Fire/EnvTrch") as AudioClip;
		// m_fireNodeAudio.outputAudioMixerGroup
		// UnityEngine.Audio.AudioMixerGroup
		m_fireNodeAudio.outputAudioMixerGroup = (Resources.Load("audio/SfxMixer") as UnityEngine.Audio.AudioMixer).FindMatchingGroups("Master")[0];
	}

	void Start() {
		PoolManager.CreatePool((GameObject)Resources.Load("Particles/PF_FireNewProc"), 25, true);

		PoolManager.CreatePool((GameObject)Resources.Load("Particles/FireSprite_a"), 25, true);
		PoolManager.CreatePool((GameObject)Resources.Load("Particles/FireSprite_b"), 25, true);
		PoolManager.CreatePool((GameObject)Resources.Load("Particles/SmokeParticle"), 25, true);
		PoolManager.CreatePool((GameObject)Resources.Load("Particles/BurnParticle"), 25, true);

		m_breath = InstanceManager.player.GetComponent<DragonBreathBehaviour>();
		m_timer = m_checkFireTime;
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
	}

	/// <summary>
	/// A new level was loaded.
	/// </summary>
	private void OnLevelLoaded() {
		// Create and populate QuadTree
		// Get map bounds!
		Rect bounds = new Rect(-440, -100, 1120, 305);	// Default hardcoded values
		LevelMapData data = GameObjectExt.FindComponent<LevelMapData>(true);
		if(data != null) {
			bounds = data.mapCameraBounds;
		}
		m_fireNodesTree = new QuadTree<FireNode>(bounds.x, bounds.y, bounds.width, bounds.height);
		for(int i = 0; i < m_fireNodes.Count; i++) {
			m_fireNodesTree.Insert(m_fireNodes[i]);
		}
	}

	public static void Insert(FireNode _fireNode) {
		instance.m_fireNodes.Add(_fireNode);
		if (instance.m_fireNodesTree != null) instance.m_fireNodesTree.Insert(_fireNode);
	}

	public static void Remove(FireNode _fireNode) {
		instance.m_fireNodes.Remove(_fireNode);
		if (instance.m_fireNodesTree != null) instance.m_fireNodesTree.Remove(_fireNode);
	}

	/// <summary>
	/// Inserts the burning. Registers a burning fire node while on fire
	/// </summary>
	/// <param name="_fireNode">Fire node.</param>
	public static void InsertBurning(Transform _fireNode) {
		instance.m_burningFireNodes.Add( _fireNode );
		if(!instance.m_fireNodeAudio.isPlaying)
			instance.m_fireNodeAudio.Play();
	}

	/// <summary>
	/// Removes the burning fire node from the registered burning list
	/// </summary>
	/// <param name="_fireNode">Fire node.</param>
	public static void RemoveBurning( Transform _fireNode) {
		instance.m_burningFireNodes.Remove(_fireNode);
		if (instance.m_burningFireNodes.Count <= 0)
			instance.m_fireNodeAudio.Stop();
	}

	void Update() {
		if(m_breath == null) return;

		//check if this intersecs with dragon breath
		if (m_breath.IsFuryOn()) {
			m_timer -= Time.deltaTime;
			if (m_timer <= 0) {
				m_timer += m_checkFireTime;
				FireNode[] nodes = m_fireNodesTree.GetItemsInRange(m_breath.bounds2D);

				for (int i = 0; i < nodes.Length; i++) {
					FireNode fireNode = nodes[i];
					if (m_breath.Overlaps(fireNode.area)) {
						fireNode.Burn(m_breath.damage * m_checkFireTime, m_breath.direction, true);
					}
				}
			}
		}

		for (int i = 0; i < m_fireNodes.Count; i++) {
			m_fireNodes[i].UpdateLogic();
		}
	}
		
	// :3
	void OnDrawGizmosSelected() {
		if (m_fireNodesTree != null)
			m_fireNodesTree.DrawGizmos(Color.yellow);
	}
}
