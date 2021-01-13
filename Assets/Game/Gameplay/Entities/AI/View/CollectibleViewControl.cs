using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CollectibleViewControl : IViewControl {
	
	//-----------------------------------------------
	[SeparatorAttribute("Collect")]
	[SerializeField] private ParticleData m_onCollectParticle;
	[SerializeField] private string m_onCollectAudio;
    [SeparatorAttribute("Skin")]
    [SerializeField] protected List<ViewControl.SkinData> m_skins = new List<ViewControl.SkinData>();

    private IEntity m_entity;
	private AudioObject m_onCollectAudioAO;

	private int m_vertexCount;
	override public int vertexCount { get { return m_vertexCount; } }

	private int m_rendererCount;
	override public int rendererCount { get { return m_rendererCount; } }

    override public float freezeParticleScale { get { return 1f; } }


    private Transform m_view;

	protected Renderer[] m_renderers;
	protected List<Material> m_materialList;
	private Dictionary<int, List<Material>> m_materials;

	protected bool m_instantiateMaterials = false;

	override public PreyAnimationEvents animationEvents { get { return null; } }

    //-----------------------------------------------
    // Use this for initialization
    //-----------------------------------------------
    protected virtual void Awake() {
		m_entity = GetComponent<Entity>();

		// Preload particle
		m_onCollectParticle.CreatePool();

		m_vertexCount = 0;
		m_rendererCount = 0;

		m_view = transform.FindObjectRecursive("view").transform;

		m_renderers = m_view.GetComponentsInChildren<Renderer>();
		m_materials = new Dictionary<int, List<Material>>();
		m_materialList = new List<Material>();

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

				for (int m = 0; m < materials.Length; ++m) {
					Material mat = materials[m];
					if (m_instantiateMaterials) {
						if (materials[m] != null) {
							mat = new Material(materials[m]);
						}
					}

					m_materialList.Add(mat);
					m_materials[renderID].Add(mat);

					materials[m] = null; // remove all materials to avoid instantiation.
				}
				renderer.sharedMaterials = materials;
			}
		}
    }

	override public void Spawn(ISpawner _spawner) {
		// Restore materials
		for (int i = 0; i < m_renderers.Length; i++) {
			int id = m_renderers[i].GetInstanceID();
			Material[] materials = m_renderers[i].sharedMaterials;
			for (int m = 0; m < materials.Length; m++) {
                Material mat = m_materials[id][m];
                if (m_skins.Count > 0) {
                    for (int s = 0; s < m_skins.Count; s++) {
                        float rnd = UnityEngine.Random.Range(0f, 100f);
                        if (rnd < m_skins[s].chance) {
                            mat = m_skins[s].skin;
                            break;
                        }
                    }
                }
                materials[m] = mat;
            }
			m_renderers[i].sharedMaterials = materials;
		}
	}

    void OnDestroy() {
    	RemoveAudios();
    }

    override public void PreDisable() {
		RemoveAudios();
    }

    private void RemoveAudios() {
		if (ApplicationManager.IsAlive) {
			RemoveAudioParent(m_onCollectAudioAO);
		}
    }

	protected void RemoveAudioParent(AudioObject ao) {
		if (ao != null && ao.transform.parent == transform) {
			ao.transform.parent = null;		
		}
	}

	public virtual void Collect() {
		if (m_entity.isOnScreen && !string.IsNullOrEmpty(m_onCollectAudio)) {
			m_onCollectAudioAO = AudioController.Play(m_onCollectAudio, transform);
		}

		if (FeatureSettingsManager.IsDebugEnabled) {
			// If the debug settings for particles eaten is disabled then they are not spawned
			if (!DebugSettings.ingameParticlesEaten)
				return;
		} 

		m_onCollectParticle.Spawn(transform.position + m_onCollectParticle.offset);
	}

	public virtual bool HasCollectAnimationFinished() {
		return true;
	}

	override public void ForceGolden(){}
    override public void Freezing(float freezeLevel) { }
	override public void CustomUpdate() {}
}