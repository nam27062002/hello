using UnityEngine;
using System.Collections;

public class DragonStats : MonoBehaviour {
	//-----------------------------------------------
	// Properties
	//-----------------------------------------------
	[SerializeField] [Range(0, 1)] private int m_type = 0; 
	public int type { get { return m_type; } }


	[Header("Progression")]
	[SerializeField] [Range(0, 10)] private int m_level = 0;  
	public int level { get { return m_level; } }
	
	[SerializeField] private float m_eatSpeed = 10f;
	public float eatSpeed { get { return m_eatSpeed; } }

	[SerializeField] private float m_speed = 100f;
	public float speed { get { return m_speed; } }

	[SerializeField] private float m_boostMultiplier = 2.5f;
	public float boostMultiplier { get { return m_boostMultiplier; } }


	[Header("Life")]
	[SerializeField] private float m_maxLife = 100f;
	public float maxLife { get { return m_maxLife; } }

	[SerializeField] private float m_lifeDrainPerSecond = 10f;
	public float lifeDrainPerSecond { get { return m_lifeDrainPerSecond; } }

	[SerializeField] private float m_lifeWarningThreshold = 0.2f;	// Percentage of maxLife
	public float lifeWarningThreshold { get { return m_lifeWarningThreshold; } }


	[Header("Energy")]
	[SerializeField] private float m_maxEnergy = 50f;
	public float maxEnergy { get { return m_maxEnergy; } }

	[SerializeField] private float m_energyDrainPerSecond = 10f;
	public float energyDrainPerSecond { get { return m_energyDrainPerSecond; } }

	[SerializeField] private float m_energyRefillPerSecond = 25f;
	public float energyRefillPerSecond { get { return m_energyRefillPerSecond; } }

	[SerializeField] private float m_energyMinRequired = 25f;
	public float energyMinRequired { get { return m_energyMinRequired; } }


	[Header("Fury")]
	[SerializeField] private float m_maxFury = 160f;
	public float maxFury { get { return m_maxFury; } }

	[SerializeField] private float m_furyDuration = 15f; //seconds
	public float furyDuration { get { return m_furyDuration; } }


	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private float m_life;
	public float life { get { return m_life; } }

	private float m_energy;
	public float energy { get { return m_energy; } }
	
	private float[] m_fury = new float[2];//we'll use a secondary variable to store all the fury got while in Rush mode 
	private bool m_furyActive = false;
	public float fury { get { return m_fury[0]; } }
		


	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	void Start () {
		m_life = m_maxLife;
		m_energy = m_maxEnergy;
		m_fury[0] = 0;
		m_fury[1] = 0;
		m_furyActive = false;
	}

	public void AddLife(float _offset) {
		m_life = Mathf.Min(m_maxLife, Mathf.Max(0, m_life + _offset)); 
	}

	public void AddEnergy(float _offset) {
		m_energy = Mathf.Min(m_maxEnergy, Mathf.Max(0, m_energy + _offset)); 
	}
		
	public void AddFury(float _offset) {
		if (m_furyActive && _offset >= 0) {
			m_fury[1] = Mathf.Min(m_maxFury, Mathf.Max(0, m_fury[1] + _offset)); 
		} else {
			m_fury[0] = Mathf.Min(m_maxFury, Mathf.Max(0, m_fury[0] + _offset)); 
		}
	}

	public void ActivateFury() {
		m_furyActive = true;
	}

	public void FinishFury() {
		//when player used all the fury, we swap all the fury we got while throwing fire
		m_furyActive = false;
		m_fury[0] = m_fury[1];
		m_fury[1] = 0;
	}

	private void SetupFromLevel() {
		// add formulas and stuff to calculate values
		// and remove properties from inspector
	}
}
