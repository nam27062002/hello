using System;
using UnityEngine;
using System.Collections.Generic;

public class ViewControlCarnivorousPlant : IViewControl, IBroadcastListener {
    private Entity m_entity;
    private Animator m_animator;

	private Renderer[] m_renderers;
    private List<Material[]> m_rendererMaterials;
    private Dictionary<int, List<Material>> m_materials;
    private Dictionary<int, List<Material>> m_materialsFrozen;
	private List<Material> m_materialList;

    private ViewControl.MaterialType m_materialType = ViewControl.MaterialType.NONE;

    private int m_vertexCount;
	override public int vertexCount { get { return m_vertexCount; } }

	private int m_rendererCount;
	override public int rendererCount { get { return m_rendererCount; } }

    override public float freezeParticleScale { get { return 1f; } }

    [SerializeField] protected string m_onAttackAudio;
	private AudioObject m_onAttackAudioAO;
	[SerializeField] private string m_onBurnAudio;

	[SerializeField] private string m_corpseAsset;
	private ParticleHandler m_corpseHandler;

	protected PreyAnimationEvents m_animEvents;
	override public PreyAnimationEvents animationEvents { get { return m_animEvents; } }

    Transform m_transform;
    private float m_freezingLevel = 0;

    //-----------------------------------------------
    // Use this for initialization
    //-----------------------------------------------
    protected virtual void Awake() {
        m_entity = GetComponent<Entity>();
        m_transform = transform;
        m_animator = transform.FindComponentRecursive<Animator>();
		m_animator.logWarnings = false;

		m_materials = new Dictionary<int, List<Material>>();
        m_materialsFrozen = new Dictionary<int, List<Material>>();
		m_materialList = new List<Material>();
		m_renderers = GetComponentsInChildren<Renderer>();
        m_rendererMaterials = new List<Material[]>();

        m_vertexCount = 0;
		m_rendererCount = 0;

		if (m_renderers != null) {
			m_rendererCount = m_renderers.Length;
			for (int i = 0; i < m_rendererCount; i++) {
				Renderer renderer = m_renderers[i];

				// Keep the vertex count (for DEBUG)
				if (renderer.GetType() == typeof(SkinnedMeshRenderer)) {
					m_vertexCount += (renderer as SkinnedMeshRenderer).sharedMesh.vertexCount;
				} else if (renderer.GetType() == typeof(MeshRenderer)) {
					MeshFilter filter = renderer.GetComponent<MeshFilter>();
					if (filter != null) {
						m_vertexCount += filter.sharedMesh.vertexCount;
					}
				}

				Material[] materials = renderer.sharedMaterials;

				// Stores the materials of this renderer in a dictionary for direct access//
				int renderID = renderer.GetInstanceID();
				m_materials[renderID] = new List<Material>();
                m_materialsFrozen[renderID] = new List<Material>();

				for (int m = 0; m < materials.Length; ++m) {
					Material mat = materials[m];

					m_materialList.Add(mat);
					m_materials[renderID].Add(mat);
                    if ( mat != null ){
                        m_materialsFrozen[renderID].Add(FrozenMaterialManager.GetFrozenMaterialFor(mat));
                    }else{
                        m_materialsFrozen[renderID].Add(null);
                    }

					materials[m] = null; // remove all materials to avoid instantiation.
				}
				renderer.sharedMaterials = materials;
                m_rendererMaterials.Add(materials);
            }
		}

		if (!string.IsNullOrEmpty(m_corpseAsset)) {
			m_corpseHandler = ParticleManager.CreatePool(m_corpseAsset, "Corpses/");
		}

		m_animEvents = transform.FindComponentRecursive<PreyAnimationEvents>();
		if (m_animEvents != null) {
			m_animEvents.onAttackStart += animEventsOnAttackStart;
		}

        Broadcaster.AddListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);
    }

	protected virtual void animEventsOnAttackStart() {
		if (!string.IsNullOrEmpty(m_onAttackAudio)){
			m_onAttackAudioAO = AudioController.Play( m_onAttackAudio, transform );
			if (m_onAttackAudioAO != null )
				m_onAttackAudioAO.completelyPlayedDelegate = OnAttackAudioCompleted;
		}
	}

	override public void Spawn(ISpawner _spawner) {
        m_materialType = ViewControl.MaterialType.NONE;
        DragonBreathBehaviour dragonBreath = InstanceManager.player.breathBehaviour;
        CheckMaterialType(false, dragonBreath.IsFuryOn(), dragonBreath.type);
    }

    void OnDestroy() {
		RemoveAudioParent( ref m_onAttackAudioAO );
        Broadcaster.RemoveListener(BroadcastEventType.FURY_RUSH_TOGGLED, this);
    }

    override public void PreDisable() {
    	RemoveAudioParent( ref m_onAttackAudioAO );
    }

    public void OnBroadcastSignal(BroadcastEventType eventType, BroadcastEventInfo broadcastEventInfo) {
        switch (eventType) {
            case BroadcastEventType.FURY_RUSH_TOGGLED: {
                    DragonBreathBehaviour dragonBreath = InstanceManager.player.breathBehaviour;
                    CheckMaterialType(false, dragonBreath.IsFuryOn(), dragonBreath.type);
                }
                break;
        }
    }

    private bool IsBurnableByPlayer(DragonBreathBehaviour.Type _fireType) {
        if (m_entity != null) {
            switch (_fireType) {
                case DragonBreathBehaviour.Type.Mega:
                return m_entity.IsBurnable();
                case DragonBreathBehaviour.Type.Standard:
                default:
                return m_entity.IsBurnable(InstanceManager.player.data.tier);

            }
        }
        return false;
    }
    
    ViewControl.MaterialType GetMaterialType(bool _isGolden, bool _furyActive = false, DragonBreathBehaviour.Type _type = DragonBreathBehaviour.Type.None) {
        ViewControl.MaterialType matType = ViewControl.MaterialType.NORMAL;
        if (_isGolden || _furyActive) {
            if (IsBurnableByPlayer(_type)) {    
                matType = ViewControl.MaterialType.GOLD;
            }
        }

        // Check Freezing. It Has priority over inlove
        if (m_freezingLevel > 0) {
            if ( matType == ViewControl.MaterialType.GOLD ){
                matType = ViewControl.MaterialType.GOLD_FREEZE;
            }else{
                matType = ViewControl.MaterialType.FREEZE;
            }
        }
        return matType;
    }
    
    
    public void RefreshMaterialType(){
        DragonBreathBehaviour dragonBreath = InstanceManager.player.breathBehaviour;
        CheckMaterialType(false, dragonBreath.IsFuryOn(), dragonBreath.type);
    }

    private void CheckMaterialType(bool _isGolden, bool _furyActive = false, DragonBreathBehaviour.Type _type = DragonBreathBehaviour.Type.None) {
        ViewControl.MaterialType matType = GetMaterialType(_isGolden, _furyActive, _type);
        if (matType != m_materialType) {
            SetMaterialType(matType);
        }
    }

    public void SetMaterialType(ViewControl.MaterialType _type) {
        m_materialType = _type;

        // Restore materials
        if (m_renderers != null) {
            for (int i = 0; i < m_renderers.Length; i++) {
                int id = m_renderers[i].GetInstanceID();
                Material[] materials = m_rendererMaterials[i];
                for (int m = 0; m < materials.Length; m++) {
                    switch (_type) {
                        case ViewControl.MaterialType.GOLD:     materials[m] = ViewControl.sm_goldenMaterial;   break;
                        case ViewControl.MaterialType.NORMAL:   materials[m] = m_materials[id][m];              break;
                        case ViewControl.MaterialType.GOLD_FREEZE: materials[m] = ViewControl.sm_goldenFreezeMaterial; break;
                        case ViewControl.MaterialType.FREEZE: materials[m] = m_materialsFrozen[id][m]; break;
                    }
                }
                m_renderers[i].materials = materials;
            }
        }
    }

    override public void CustomUpdate() { }


	public void Attack(bool _attack) { 
		m_animator.SetBool( GameConstants.Animator.ATTACK , _attack); 
	}

	void OnAttackAudioCompleted(AudioObject ao)
	{
		RemoveAudioParent( ref m_onAttackAudioAO);
	}

	protected void RemoveAudioParent(ref AudioObject ao)
	{
		if ( ao != null && ao.transform.parent == transform )
		{
			ao.transform.parent = null;	
			ao.completelyPlayedDelegate = null;
			if ( ao.IsPlaying() && ao.audioItem.Loop != AudioItem.LoopMode.DoNotLoop )
				ao.Stop();
		}
		ao = null;
	}

	public void Burn(float _burnAnimSeconds) {
		if (!string.IsNullOrEmpty(m_onBurnAudio)) {
			AudioController.Play(m_onBurnAudio, transform.position);
		}
	}
    
     override public void Freezing( float freezeLevel ){
        if ((m_freezingLevel <= 0 && freezeLevel > 0) || (m_freezingLevel > 0 && freezeLevel <= 0)) {
            m_freezingLevel = freezeLevel;
            RefreshMaterialType();
            if ( m_freezingLevel > 0 )
            {
                AudioController.Play("freeze", m_transform.position);
            }
        }   
        m_freezingLevel = freezeLevel;  
    }
    

	public void Aim(float _blendFactor) { m_animator.SetFloat(GameConstants.Animator.AIM, _blendFactor); }

	override public void ForceGolden(){}


	// Queries
	public bool HasCorpseAsset() {
		return !string.IsNullOrEmpty(m_corpseAsset);
	}

	public void Bite() {
		if (m_corpseHandler != null) {
			// spawn corpse
			GameObject corpse = m_corpseHandler.Spawn(null);
			if (corpse != null) {
				corpse.transform.CopyFrom(transform);
				corpse.GetComponent<Corpse>().Spawn(false, false);
			}
		}
	}
    
}
