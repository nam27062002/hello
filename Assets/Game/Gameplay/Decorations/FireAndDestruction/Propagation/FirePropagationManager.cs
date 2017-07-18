using UnityEngine;
using System.Collections.Generic;

// This is a Quadtree! a Quadtree full of fires
public class FirePropagationManager : UbiBCN.SingletonMonoBehaviour<FirePropagationManager> {
	
	private QuadTree<FireNode> m_fireNodesTree;
	private List<FireNode> m_fireNodes;
	private List<FireNode> m_burningFireNodes;
	private AudioSource m_fireNodeAudio;

	private BoundingSphere[] m_boundigSpheres;

	private CullingGroup m_cullingGroup;

	void Awake() {
		m_fireNodesTree = new QuadTree<FireNode>(-600f, -100f, 1000f, 400f);
		m_fireNodes = new List<FireNode>();
		m_burningFireNodes = new List<FireNode>();

		m_boundigSpheres = new BoundingSphere[1000];

		m_fireNodeAudio = gameObject.AddComponent<AudioSource>();
		m_fireNodeAudio.playOnAwake = false;
		m_fireNodeAudio.loop = false;
		m_fireNodeAudio.clip = Resources.Load("audio/sfx/Fire/EnvTrch") as AudioClip;

		m_fireNodeAudio.outputAudioMixerGroup = (Resources.Load("audio/SfxMixer") as UnityEngine.Audio.AudioMixer).FindMatchingGroups("Master")[0];
	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Messenger.AddListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
		Messenger.AddListener(GameEvents.GAME_ENDED, OnGameEnded);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Messenger.RemoveListener(GameEvents.GAME_LEVEL_LOADED, OnLevelLoaded);
		Messenger.RemoveListener(GameEvents.GAME_ENDED, OnGameEnded);
	}

	/// <summary>
	/// A new level was loaded.
	/// </summary>
	private void OnLevelLoaded() {
		// Create and populate QuadTree
		// Get map bounds!
		Rect bounds = new Rect(-440, -100, 1120, 305);	// Default hardcoded values
		LevelData data = LevelManager.currentLevelData;
		if(data != null) {
			bounds = data.bounds;
		}
		m_fireNodesTree = new QuadTree<FireNode>(bounds.x, bounds.y, bounds.width, bounds.height);
		for(int i = 0; i < m_fireNodes.Count; i++) {
			m_fireNodesTree.Insert(m_fireNodes[i]);
		}


		m_cullingGroup = new CullingGroup();
		m_cullingGroup.targetCamera = Camera.main;
		m_cullingGroup.SetBoundingSpheres(m_boundigSpheres);
		m_cullingGroup.SetBoundingSphereCount(m_fireNodes.Count);
		m_cullingGroup.onStateChanged += CullingStateChange;

		/*
		m_cullingGroup.targetCamera = Camera.main;
		m_cullingGroup.SetBoundingSphereCount(m_boundingSpheres.Count);*/
	}

	/// <summary>
	/// The game has ended.
	/// </summary>
	private void OnGameEnded() {
		// Clear QuadTree
		m_fireNodesTree = null;
		m_fireNodes.Clear();
		m_burningFireNodes.Clear();

		m_cullingGroup.onStateChanged -= CullingStateChange;
		m_cullingGroup.Dispose();
		m_cullingGroup = null;
	}

	public static void Insert(FireNode _fireNode) {
		instance.m_fireNodes.Add(_fireNode);

		if (instance.m_fireNodes.Count < instance.m_boundigSpheres.Length) {
			instance.m_boundigSpheres[instance.m_fireNodes.Count - 1] = _fireNode.boundingSphere;

			if (instance.m_cullingGroup != null) {
				instance.m_cullingGroup.SetBoundingSphereCount(instance.m_fireNodes.Count);
			}
		}

		if (instance.m_fireNodesTree != null) instance.m_fireNodesTree.Insert(_fireNode);
	}

	public static void RegisterBurningNode(FireNode _fireNode) {
		instance.m_burningFireNodes.Add(_fireNode);
	}

	public static void UnregisterBurningNode(FireNode _fireNode) {
		instance.m_burningFireNodes.Remove(_fireNode);
	}

	/// <summary>
	/// Inserts the burning. Registers a burning fire node while on fire
	/// </summary>
	/// <param name="_fireNode">Fire node.</param>
	public static void PlayBurnAudio() {
		if(!instance.m_fireNodeAudio.isPlaying)
			instance.m_fireNodeAudio.Play();
	}

	/// <summary>
	/// Removes the burning fire node from the registered burning list
	/// </summary>
	/// <param name="_fireNode">Fire node.</param>
	public static void StopBurnAudio() {		
		if (instance.m_burningFireNodes.Count <= 0)
			if (instance.m_fireNodeAudio != null)
				instance.m_fireNodeAudio.Stop();
	}

	void Update() {
		for (int i = 0; i < m_burningFireNodes.Count; i++) {
			m_burningFireNodes[i].UpdateLogic();
		}
	}

	private static void CullingStateChange(CullingGroupEvent evt) {
		if (evt.hasBecomeVisible) {
			instance.m_fireNodes[evt.index].SetEffectVisibility(true);
		} else if (evt.hasBecomeInvisible) {
			instance.m_fireNodes[evt.index].SetEffectVisibility(false);
		}      
	}

	public delegate bool CheckMethod( CircleAreaBounds _fireNodeBounds );

	public void FireUpNodes(Rect _rectArea, CheckMethod _checkMethod, DragonTier _tier, Vector3 _direction)	{
		FireNode[] nodes = m_fireNodesTree.GetItemsInRange(_rectArea);
		for (int i = 0; i < nodes.Length; i++) {
			FireNode fireNode = nodes[i];
			if (_checkMethod(fireNode.area)) {
				fireNode.Burn(_direction, true, _tier);
			}
		}
	}
			
	// :3
	void OnDrawGizmosSelected() {
		if (m_fireNodesTree != null)
			m_fireNodesTree.DrawGizmos(Color.yellow);
	}
}
