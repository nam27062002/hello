using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragonBoostBehaviour : MonoBehaviour {
	
	
	//-----------------------------------------------
	// Attributes
	//-----------------------------------------------
	private DragonPlayer 	m_dragon;
	private DragonControl 	m_controls;
	private Animator 		m_animator;

	private bool m_active;
	private bool m_ready;

	public List<GameObject> m_trails;

	//-----------------------------------------------
	// Methods
	//-----------------------------------------------
	// Use this for initialization
	void Awake () {
		m_dragon = GetComponent<DragonPlayer>();	
		m_controls = GetComponent<DragonControl>();
		m_animator = transform.FindChild("view").GetComponent<Animator>();

		m_active = false;
		m_ready = true;

		for( int i = 0; i<m_trails.Count; i++ )
		{
			TrailRenderer tr = m_trails[i].GetComponent<TrailRenderer>();
			tr.sortingLayerName = "player";
		}

		DeactivateTrails();
	}

	void OnEnable() {
		
		m_active = false;
		m_ready = true;
	}

	void OnDisable() {
		StopBoost();
	}
	
	// Update is called once per frame
	void Update () {
		bool activate = Input.GetKey(KeyCode.X) || m_controls.action;

		if (activate) {
			if (m_ready) {
				m_ready = false;
				if (m_dragon.energy > GameSettings.energyRequiredToBoost) {
					StartBoost();
				}
			}
		} else {
			m_ready = true;
			if (m_active) {
				StopBoost();
			}
		}

		if (m_active) {
			// Don't drain energy if cheat is enabled
			if(!Debug.isDebugBuild || !DebugSettings.infiniteBoost) {
				m_dragon.AddEnergy(-Time.deltaTime * m_dragon.data.def.energyDrainPerSecond);
				if (m_dragon.energy <= 0f) {
					StopBoost();
				}
			}
		} else {
			m_dragon.AddEnergy(Time.deltaTime * m_dragon.data.def.energyRefillPerSecond);
		}
	}


	private void StartBoost() 
	{
		m_active = true;
		m_dragon.SetSpeedMultiplier(m_dragon.data.boostSkill.value);
		// ActivateTrails();
		if (m_animator && m_animator.isInitialized)
			m_animator.SetBool("boost", true);
		Messenger.Broadcast<bool>(GameEvents.BOOST_TOGGLED, true);
	}

	public void StopBoost() 
	{
		m_active = false;
		m_dragon.SetSpeedMultiplier(1f);
		// DeactivateTrails();
		if (m_animator && m_animator.isInitialized)
			m_animator.SetBool("boost", false);

		Messenger.Broadcast<bool>(GameEvents.BOOST_TOGGLED, false);
	}

	public bool IsBoostActive()
	{
		return m_active;
	}

	public void ResumeBoost() {
		m_ready = true;
	}

	public void ActivateTrails()
	{
		for( int i = 0; i<m_trails.Count; i++ )
		{
			m_trails[i].SetActive(true);
		}
	}

	public void DeactivateTrails()
	{
		for( int i = 0; i<m_trails.Count; i++ )
		{
			m_trails[i].SetActive(false);
		}
	}
}
