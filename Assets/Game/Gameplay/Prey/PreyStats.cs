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
	
	[SerializeField] private float m_maxHealth = 100f;
	public float maxHealth { get { return m_maxHealth; } }



	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private bool m_isGolden;
	public bool isGolden { get { return m_isGolden; } }

	private float m_health;
	public float health { get { return m_health; } }

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

		m_health = m_maxHealth;		
		SetGolden((Random.Range(0, 1000) < 200));
	}

	public void AddLife(float _offset) {
		m_health = Mathf.Min(m_maxHealth, Mathf.Max(0, m_health + _offset)); 
	}

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
}
