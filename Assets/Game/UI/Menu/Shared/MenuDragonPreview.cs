// MenuDragonPreview.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 26/01/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------//
// INCLUDES																//
//----------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

//----------------------------------------------------------------------//
// CLASSES																//
//----------------------------------------------------------------------//
/// <summary>
/// Preview of a dragon in the main menu.
/// </summary>
public class MenuDragonPreview : MonoBehaviour {
	//------------------------------------------------------------------//
	// CONSTANTS														//
	//------------------------------------------------------------------//
	public enum Anim {
		NONE = -1,
		IDLE,
		UNLOCKED,
		RESULTS,
		POSE_FLY,
		FLY,

		COUNT
	};

	public static readonly string[] ANIM_TRIGGERS  = {
		"idle",
		"unlocked",
		"results",
		"pose_fly",
		"fly"
	};


	//------------------------------------------------------------------//
	// MEMBERS															//
	//------------------------------------------------------------------//
	// Exposed
	[SerializeField] private string m_sku;
	public string sku { get { return m_sku; }}


	private Anim m_currentAnim = Anim.IDLE;


	// IDLE STATE CONTROL
	public enum AltAnimSpecialAction
	{
		NONE,
		BIRD,
	};

	[System.Serializable]
	public struct AltAnimConfig
	{
		public Range m_range;
		public bool m_allowFaceDetails;
		public int m_loopsMin;
		public int m_loopsMax;
		public AltAnimSpecialAction m_special;
		public float m_timeToNext;
		public bool m_isPreferedAnimation;	// Animation Used when we touch the dragon on dragon selection
		public int m_animationLevel;
	}
	public List<AltAnimConfig> m_altAnimConfigs = new List<AltAnimConfig>();
	private int m_currentAnimIndex = -1;
	private int m_lastAltAnim = -1;
	private int m_count;


	// Components
	private DragonEquip m_equip = null;
	public DragonEquip equip {
		get {
			if(m_equip == null) {
				m_equip = this.GetComponent<DragonEquip>();
			}
			return m_equip;
		}
	}

	// Internal
	private Animator m_animator = null;
	public Animator animator{
		get{
			if(m_animator == null) {
				m_animator = GetComponentInChildren<Animator>();
			} 
			return m_animator;
		}	
	}

	private Renderer[] m_renderers;
	private Dictionary<int, List<Material>> m_materials;


	public bool m_hasFire = false;
	public GameObject m_dragonFlameStandard = null;
	private FireBreathDynamic m_dragonFlameStandardInstance = null;

	public ParticleControl m_bloodParticle;

	private bool m_allowAltAnimations = true;
	public bool allowAltAnimations{ get{ return m_allowAltAnimations; }set{ m_allowAltAnimations = value; } }

	private int m_altAnimationsMaxLevel = 10;
	public int altAnimationsMaxLevel{ get{ return m_altAnimationsMaxLevel; }set{ m_altAnimationsMaxLevel = value; } }

    public ParticleControl[] m_extraParticles;


    //------------------------------------------------------------------//
    // GENERIC METHODS													//
    //------------------------------------------------------------------//
    private void Awake()
    {
        m_animator = GetComponentInChildren<Animator>();
        m_renderers = GetComponentsInChildren<Renderer>();
        m_materials = new Dictionary<int, List<Material>>();

        if (m_renderers != null)
        {
            for (int i = 0; i < m_renderers.Length; i++)
            {
                Renderer renderer = m_renderers[i];
                Material[] materials = renderer.sharedMaterials;

                // Stores the materials of this renderer in a dictionary for direct access//
                List<Material> materialList = new List<Material>();
                materialList.AddRange(materials);
                m_materials[renderer.GetInstanceID()] = materialList;
            }
        }

        m_count = m_altAnimConfigs.Count;
    }


    void Start(){


		NumLoopsBehaviour[] behaviours = m_animator.GetBehaviours<NumLoopsBehaviour>();
		int behavioursCount = behaviours.Length;

		for( int i = 0;i<m_count; ++i ){
			AltAnimConfig item = m_altAnimConfigs[i];
			item.m_timeToNext -= item.m_range.GetRandom();
			m_altAnimConfigs[i] = item;

			for( int j = 0; j<behavioursCount; ++j ){
				if ( behaviours[ j ].m_optionalId == i ){
					behaviours[ j ].m_minLoops = item.m_loopsMin;
					behaviours[ j ].m_maxLoops = item.m_loopsMax;
				}
			}
		}
		m_currentAnimIndex = -1;
		m_lastAltAnim = - 1;

		if ( m_hasFire )
		{
			Transform cacheTransform = transform;
	        Transform mouth = cacheTransform.FindTransformRecursive("mouth");
			GameObject tempFire = Instantiate<GameObject>(m_dragonFlameStandard);
	        Transform t = tempFire.transform;
	        t.SetParent(mouth, true);
	        t.localPosition = Vector3.zero;
	        t.localRotation = Quaternion.Euler(new Vector3(0.0f, 0.0f, 180.0f));
	        t.localScale = GameConstants.Vector3.one;
			m_dragonFlameStandardInstance = tempFire.GetComponent<FireBreathDynamic>();

			DefinitionNode def = DefinitionsManager.SharedInstance.GetDefinition( DefinitionsCategory.DRAGONS, m_sku);
			float furyBaseLength = def.GetAsFloat("furyBaseLength");
			m_dragonFlameStandardInstance.setEffectScale(furyBaseLength / 2.0f, transform.localScale.x);

			m_dragonFlameStandardInstance.EnableFlame(false);
			// m_dragonFlameStandardInstance.gameObject.SetActive(false);

		}

	}

	/// <summary>
	/// Apply the given animation to the dragon's animator.
	/// </summary>
	/// <param name="_anim">The animation to be launched.</param>
	public void SetAnim(Anim _anim) {
		m_currentAnim = _anim;
		if(m_animator != null) {
			m_animator.SetTrigger(ANIM_TRIGGERS[(int)_anim]);
		}
	}

	public void DisableMovesOnResults()
	{
		DragonPartFollow[] moves = GetComponentsInChildren<DragonPartFollow>();
		if ( moves != null )
		{
			for( int i = 0; i<moves.Length; ++i )
			{
				if ( moves[i].m_disableOnResults )
					moves[i].enabled = false;
			}
		}
	}

	public void SetFresnelColor( Color col )
	{
        if (m_renderers != null )
        {
            for (int i = 0; i < m_renderers.Length; i++)
            {
                Material[] mats = m_renderers[i].materials;
                for (int j = 0; j < mats.Length; j++)
                {
                    string shaderName = mats[j].shader.name;
                    if (shaderName.Contains("Dragon standard"))
                    {
                        mats[j].SetColor("_FresnelColor", col);
                    }
                }
            }
        }
	}

	public void Update()
	{
		switch( m_currentAnim )
		{
			case Anim.IDLE:
			{
				// Work with alternative animations
				if (m_currentAnimIndex >= 0)
				{
					// Check if state is "idle" to get back
					if ( m_animator.GetInteger(GameConstants.Animator.ALT_ANIMATION) == -1 && m_animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") )
					{
						AltAnimConfig item = m_altAnimConfigs[m_lastAltAnim];
						item.m_timeToNext = item.m_range.GetRandom();
						m_altAnimConfigs[m_lastAltAnim] = item;
						m_currentAnimIndex = -1;
						m_lastAltAnim = - 1;
					}
				}
				else
				{
					// Count down every timer to check when to start the new alternative animation
					if (m_allowAltAnimations)
					{
						for( int i = 0; i<m_count && m_currentAnimIndex < 0; ++i )
						{
							AltAnimConfig item = m_altAnimConfigs[i];
							if ( item.m_animationLevel <= m_altAnimationsMaxLevel )
							{
								item.m_timeToNext -= Time.deltaTime;	
								m_altAnimConfigs[i] = item;
								if (item.m_timeToNext <= 0) 
								{
									// Set alternative animation
									m_lastAltAnim = m_currentAnimIndex = i;
									m_animator.SetInteger(GameConstants.Animator.ALT_ANIMATION, m_currentAnimIndex);
									// 
									if ( item.m_special != AltAnimSpecialAction.NONE )
										StartSpecialEvent( item.m_special );
								}
							}
						}
					}
				}
			}break;
		}
	}

	public void ForcePreferedAltAnimation()
	{
		for( int i = 0; i<m_count && m_currentAnimIndex < 0; ++i )
		{
			AltAnimConfig item = m_altAnimConfigs[i];
			if ( item.m_isPreferedAnimation )
			{
				item.m_timeToNext = 0;
			}
			m_altAnimConfigs[i] = item;

			// Check if we are doing another alt animation to stop it
			if (m_currentAnimIndex >= 0)
			{
				if (m_animator.GetInteger( GameConstants.Animator.ALT_ANIMATION ) != -1)
				{
					m_animator.SetInteger(GameConstants.Animator.ALT_ANIMATION, -1);
				}
			}
		}
	}

	void StartSpecialEvent( AltAnimSpecialAction action )
	{
		switch( action )
		{
			case AltAnimSpecialAction.BIRD:
			{
				// Spawn special anim bird!!
				MenuDragonBirdControl birdControl = GetComponent<MenuDragonBirdControl>();
				if ( birdControl )
				{
					birdControl.PlayBird();
				}
			}break;
		}
	}

	public void StartFlame()
	{
		m_dragonFlameStandardInstance.EnableFlame(true, false);
	}

	public void EndFlame()
	{
		m_dragonFlameStandardInstance.EnableFlame(false, false);
	}

	public void StartBlood()
	{
        bool useBlood = FeatureSettingsManager.instance.IsBloodEnabled();
		if ( useBlood )
			m_bloodParticle.Play();
	}

	public void EndBlood()
	{
		m_bloodParticle.Stop();
	}
    
    public void PlayExtraParticle(int index)
    {
        if (index < m_extraParticles.Length)
        {
            m_extraParticles[index].Play();
        }
    }
    
    public void StopExtraParticle( int index )
    {
        if (index < m_extraParticles.Length)
        {
            m_extraParticles[index].Stop();
        }
    }
}

