using UnityEngine;
using System.Collections.Generic;

// This is a Quadtree! a Quadtree full of fires
public class FirePropagationManager : UbiBCN.SingletonMonoBehaviour<FirePropagationManager>, IBroadcastListener {
	
	private QuadTree<FireNode> m_fireNodesTree;
    private HashSet<FireNode> m_selectedFireNodes = new HashSet<FireNode>();
    private List<FireNode> m_fireNodes;
	private List<FireNode> m_burningFireNodes;
	private AudioSource m_fireNodeAudio = null;

	private BoundingSphere[] m_boundigSpheres;

    private CullingGroup m_cullingGroup;

	void Awake() {
		m_fireNodesTree = new QuadTree<FireNode>(-1600f, -600f, 2600f, 1400f);
		m_fireNodes = new List<FireNode>();
		m_burningFireNodes = new List<FireNode>();

		m_boundigSpheres = new BoundingSphere[1000];


	}

	/// <summary>
	/// Component enabled.
	/// </summary>
	private void OnEnable() {
		// Subscribe to external events
		Broadcaster.AddListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
		Broadcaster.AddListener(BroadcastEventType.GAME_AREA_ENTER, this);
		Broadcaster.AddListener(BroadcastEventType.GAME_AREA_EXIT, this);
		Broadcaster.AddListener(BroadcastEventType.GAME_ENDED, this);
	}

	/// <summary>
	/// Component disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe from external events
		Broadcaster.RemoveListener(BroadcastEventType.GAME_LEVEL_LOADED, this);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_ENTER, this);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_AREA_EXIT, this);
		Broadcaster.RemoveListener(BroadcastEventType.GAME_ENDED, this);
	}

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo)
    {
        switch( eventType )
        {
            case BroadcastEventType.GAME_LEVEL_LOADED:
            case BroadcastEventType.GAME_AREA_ENTER:
            {
                OnLevelLoaded();
            }break;
            case BroadcastEventType.GAME_ENDED:
            case BroadcastEventType.GAME_AREA_EXIT:
            {
                OnGameEnded();
            }break;
        }
    }
    
	/// <summary>
	/// A new level was loaded.
	/// </summary>
	private void OnLevelLoaded() {

		/*
		if ( m_fireNodeAudio == null )
		{
			m_fireNodeAudio = gameObject.AddComponent<AudioSource>();
			m_fireNodeAudio.playOnAwake = false;
			m_fireNodeAudio.loop = false;
			m_fireNodeAudio.clip = Resources.Load("audio/sfx/Fire/EnvTrch") as AudioClip;
			m_fireNodeAudio.outputAudioMixerGroup = (Resources.Load("audio/MasterMixer") as UnityEngine.Audio.AudioMixer).FindMatchingGroups("Master")[0];
		}
		 */

		// Create and populate QuadTree
		// Get map bounds!
		Rect bounds = new Rect(-1600f, -600f, 2600f, 1400f);	// Default hardcoded values
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
        if (instance != null) {
            instance.m_burningFireNodes.Remove(_fireNode);
        }
	}

	/// <summary>
	/// Inserts the burning. Registers a burning fire node while on fire
	/// </summary>
	public static void PlayBurnAudio() {
		/*
		if(instance.m_fireNodeAudio != null && !instance.m_fireNodeAudio.isPlaying)
			instance.m_fireNodeAudio.Play();
		*/
	}

	/// <summary>
	/// Removes the burning fire node from the registered burning list
	/// </summary>
	public static void StopBurnAudio() {
		/*
        if (instance != null && instance.m_burningFireNodes != null) {
			if (instance.m_burningFireNodes.Count <= 0)
				if (instance.m_fireNodeAudio != null && instance.m_fireNodeAudio != null)
					instance.m_fireNodeAudio.Stop();
		}
		 */
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



    public void FireUpNodes(Rect _rectArea, CheckMethod _checkMethod, DragonTier _tier, DragonBreathBehaviour.Type _breathType, Vector3 _direction, IEntity.Type _source, FireColorSetupManager.FireColorType _colorType = FireColorSetupManager.FireColorType.RED)	
    {
        if ( m_fireNodesTree != null && _checkMethod != null)
        {
            m_fireNodesTree.GetHashSetInRange(_rectArea, ref m_selectedFireNodes);
            foreach (FireNode fireNode in m_selectedFireNodes) {
    			if (fireNode != null && _checkMethod(fireNode.area)) {
    				fireNode.Burn(_direction, true, _tier, _breathType, _source, _colorType);
    			}
    		}
            m_selectedFireNodes.Clear();
        }
    }
			
	// :3
	void OnDrawGizmosSelected() {
		if (m_fireNodesTree != null)
			m_fireNodesTree.DrawGizmos(Color.yellow);
	}
}
