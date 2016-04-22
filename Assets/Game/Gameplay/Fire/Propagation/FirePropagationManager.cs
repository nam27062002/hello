using UnityEngine;
using System.Collections.Generic;

// This is a Quadtree! a Quadtree full of fires
public class FirePropagationManager : SingletonMonoBehaviour<FirePropagationManager> {
	
	[SerializeField] private float m_checkFireTime = 0.25f;

	private QuadTree m_fireNodes;
	private AudioSource m_fireNodeAudio;
	private DragonBreathBehaviour m_breath;
	
	private float m_timer;

	public List<Transform> m_burningFireNodes = new List<Transform>();

	void Awake() {
		m_fireNodes = new QuadTree(-600f, -100f, 1000f, 400f);

		m_fireNodeAudio = gameObject.AddComponent<AudioSource>();
		m_fireNodeAudio.playOnAwake = false;
		m_fireNodeAudio.loop = false;
		m_fireNodeAudio.clip = Resources.Load("audio/sfx/Fire/EnvTrch") as AudioClip;
		// m_fireNodeAudio.outputAudioMixerGroup
		// UnityEngine.Audio.AudioMixerGroup
		m_fireNodeAudio.outputAudioMixerGroup = (Resources.Load("audio/SfxMixer") as UnityEngine.Audio.AudioMixer).FindMatchingGroups("Master")[0];
	}

	void Start() {
		PoolManager.CreatePool((GameObject)Resources.Load("Particles/FireSprite"), 25, true);
		PoolManager.CreatePool((GameObject)Resources.Load("Particles/FireSprite_a"), 25, true);
		PoolManager.CreatePool((GameObject)Resources.Load("Particles/FireSprite_b"), 25, true);
		PoolManager.CreatePool((GameObject)Resources.Load("Particles/SmokeParticle"), 25, true);
		PoolManager.CreatePool((GameObject)Resources.Load("Particles/BurnParticle"), 25, true);

		// get player breath component
		m_breath = InstanceManager.player.GetComponent<DragonBreathBehaviour>();
		m_timer = m_checkFireTime;
	}

	void Update() {
		//check if this intersecs with dragon breath
		if ( m_breath.IsFuryOn() )
		{
			m_timer -= Time.deltaTime;
			if (m_timer <= 0) 
			{
				m_timer += m_checkFireTime;
				Transform[] nodes = m_fireNodes.GetItemsInRange(m_breath.bounds2D);

				for (int i = 0; i < nodes.Length; i++) {
					Transform node = nodes[i];
					if (m_breath.IsInsideArea(node.position)) 
					{
						// Check if I can burn this fire Node
						FireNode fireNode = node.GetComponent<FireNode>();
						if ( fireNode.canBurn || m_breath.type == DragonBreathBehaviour.Type.Super )
						{
							fireNode.Burn(m_breath.damage * m_checkFireTime, m_breath.direction, true);
						}
						else
						{
							// Show message: "I cannot burn this!"
						}
					}
				}
			}
		}
	}
	
	public static void Insert(Transform _fireNode) {

		instance.m_fireNodes.Insert(_fireNode);
	}

	public static void Remove(Transform _fireNode) {

		instance.m_fireNodes.Remove(_fireNode);
	}

	/// <summary>
	/// Inserts the burning. Registers a burning fire node while on fire
	/// </summary>
	/// <param name="_fireNode">Fire node.</param>
	public static void InsertBurning( Transform _fireNode )
	{
		instance.m_burningFireNodes.Add( _fireNode );
		if(!instance.m_fireNodeAudio.isPlaying)
			instance.m_fireNodeAudio.Play();
	}

	/// <summary>
	/// Removes the burning fire node from the registered burning list
	/// </summary>
	/// <param name="_fireNode">Fire node.</param>
	public static void RemoveBurning( Transform _fireNode)
	{
		instance.m_burningFireNodes.Remove( _fireNode );
		if ( instance.m_burningFireNodes.Count <= 0 )
			instance.m_fireNodeAudio.Stop();
	}


	// :3
	void OnDrawGizmosSelected() {
		if (m_fireNodes != null)
			m_fireNodes.DrawGizmos(Color.yellow);
	}
}
