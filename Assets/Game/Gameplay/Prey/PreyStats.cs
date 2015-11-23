using UnityEngine;
using System.Collections;

public class PreyStats : Initializable {


	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[SerializeField] private string m_typeID;
	public string typeID { get { return m_typeID; } }

	[SerializeField] private Reward m_reward;
	public Reward reward { get { return m_reward; } }

	[SerializeField][Range(0,1)] private float m_goldenChance = 0f;
	[SerializeField][Range(0,1)] private float m_pcChance = 0f;

	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private bool m_isGolden = false;
	private bool m_givePC = false;

	private Material[] m_materials;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Awake() {
		// keep the original materials, sometimes it will become Gold!
		SkinnedMeshRenderer renderer = GetComponentInChildren<SkinnedMeshRenderer>();		
		if (renderer) {
			m_materials = GetComponentInChildren<SkinnedMeshRenderer>().materials;
		}
	}

	public override void Initialize() {
	//	m_health = m_maxHealth;		
		SetGolden((Random.Range(0f, 1f) <= m_goldenChance));

		// [AOC] TODO!! Implement PC shader, implement PC reward feedback
		m_givePC = (Random.Range(0f, 1f) <= m_pcChance);
	}

/*	public void AddLife(float _offset) {
		m_health = Mathf.Min(m_maxHealth, Mathf.Max(0, m_health + _offset)); 
	}*/

	private void SetGolden(bool _value) {
		SkinnedMeshRenderer renderer = GetComponentInChildren<SkinnedMeshRenderer>();

		if (renderer) {
			if (_value) {
				Material goldMat = Resources.Load ("PROTO/Materials/Gold") as Material;
				Material[] materials = GetComponentInChildren<SkinnedMeshRenderer>().materials;
				for (int i = 0; i < materials.Length; i++) {
					materials[i] = goldMat;
				}
				GetComponentInChildren<SkinnedMeshRenderer>().materials = materials;
			} else {
				GetComponentInChildren<SkinnedMeshRenderer>().materials = m_materials;
			}
		}

		m_isGolden = _value;
	}

	/// <summary>
	/// Get a Reward struct initialized with the reward to be given when killing this
	/// prey, taking in account its golden/pc chances and status.
	/// </summary>
	/// <returns>The reward to be given to the player when killing this unit.</returns>
	public Reward GetOnKillReward() {
		// Create a copy of the base rewards and tune them
		Reward newReward = m_reward;	// Since it's a struct, this creates a new copy rather than being a reference

		// Give coins?
		if(!m_isGolden) {
			newReward.coins = 0;
		}

		// Give PC?
		if(!m_givePC) {
			newReward.pc = 0;
		}

		return newReward;
	}
}
