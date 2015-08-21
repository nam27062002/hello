using UnityEngine;
using System.Collections;

public class DragonStats : MonoBehaviour {
	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[SerializeField] [Range(0, 1)] private int m_type = 0; 
	public int type { get { return m_type; } }

	[SerializeField] [Range(0, 10)] private int m_level = 0;  
	public int level { get { return m_level; } }

	[SerializeField] private float m_eatSpeed = 10f;
	public float eatSpeed { get { return m_eatSpeed; } }

	[SerializeField] private float m_speed = 100f;
	public float speed { get { return m_speed; } }

	[SerializeField] private float m_boostMultiplier = 2.5f;
	public float boostMultiplier { get { return m_boostMultiplier; } }

	[SerializeField] private float m_maxLife = 100f;
	public float maxLife { get { return m_maxLife; } }

	[SerializeField] private float m_maxEnergy = 50f;
	public float maxEnergy { get { return m_maxEnergy; } }

	private float m_life;
	public float life { get { return m_life; } }

	private float m_energy;
	public float energy { get { return m_energy; } }

	
	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	void Start () {
		m_life = m_maxLife;
		m_energy = m_maxEnergy;
	}

	public void AddLife(float _offset) {
		m_life = Mathf.Min(m_maxLife, Mathf.Max(0, m_life + _offset)); 
	}

	public void AddEnergy(float _offset) {
		m_energy = Mathf.Min(m_maxEnergy, Mathf.Max(0, m_energy + _offset)); 
	}


	private void SetupFromLevel() {
		// add formulas and stuff to calculate values
		// and remove properties from inspector
	}
}
