using UnityEngine;
using System.Collections.Generic;

// This is a Quadtree! a Quadtree full of fires
public class FirePropagationManager : SingletonMonoBehaviour<FirePropagationManager> {
	
	[SerializeField] private float m_checkFireTime = 0.25f;

	private QuadTree m_fireNodes;
	private DragonBreathBehaviour m_breath;
	
	private float m_timer;



	void Awake() {
		m_fireNodes = new QuadTree(-600f, -100f, 1000f, 400f);
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
		m_timer -= Time.deltaTime;
		if (m_timer <= 0) {
			m_timer = m_checkFireTime;

			List<Transform> nodes = m_fireNodes.GetItemsInRange(m_breath.bounds2D);

			for (int i = 0; i < nodes.Count; i++) {
				if (m_breath.IsInsideArea(nodes[i].position)) {
					nodes[i].GetComponent<FireNode>().Burn(m_breath.damage);
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


	// :3
	void OnDrawGizmosSelected() {
		if (m_fireNodes != null)
			m_fireNodes.DrawGizmos(Color.yellow);
	}
}
