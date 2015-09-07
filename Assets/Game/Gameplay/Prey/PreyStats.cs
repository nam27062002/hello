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
	
	[SerializeField] private float m_maxLife = 100f;
	public float maxLife { get { return m_maxLife; } }



	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private bool m_isGolden;
	public bool isGolden { get { return m_isGolden; } }

	private float m_life;

	private Material[] m_materials;



	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Start() {
		// keep the original materials, sometimes it will become Gold!
		m_materials = GetComponentInChildren<SkinnedMeshRenderer>().materials;
	}

	public override void Initialize() {

		m_life = m_maxLife;		
		SetGolden((Random.Range(0, 1000) < 200));
	}

	private void SetGolden(bool _value) {
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

		m_isGolden = _value;
	}
}
