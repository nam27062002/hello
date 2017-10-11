using System;
using UnityEngine;

public class ViewControlCarnivorousPlant : MonoBehaviour, IViewControl, ISpawnable {
	
	private Animator m_animator;
	private AudioObject m_onCollectAudioAO;

	private int m_vertexCount;
	public int vertexCount { get { return m_vertexCount; } }

	private int m_rendererCount;
	public int rendererCount { get { return m_rendererCount; } }

    //-----------------------------------------------
    // Use this for initialization
    //-----------------------------------------------
    protected virtual void Awake() {
		m_animator = transform.FindComponentRecursive<Animator>();
		m_animator.logWarnings = false;
		
		m_vertexCount = 0;
		m_rendererCount = 0;

		Renderer[] renderers = GetComponentsInChildren<Renderer>();
		if (renderers != null) {
			m_rendererCount = renderers.Length;
			for (int i = 0; i < m_rendererCount; i++) {
				Renderer renderer = renderers[i];

				// Keep the vertex count (for DEBUG)
				if (renderer.GetType() == typeof(SkinnedMeshRenderer)) {
					m_vertexCount += (renderer as SkinnedMeshRenderer).sharedMesh.vertexCount;
				} else if (renderer.GetType() == typeof(MeshRenderer)) {
					MeshFilter filter = renderer.GetComponent<MeshFilter>();
					if (filter != null) {
						m_vertexCount += filter.sharedMesh.vertexCount;
					}
				}
			}
		}
    }

	public void Spawn(ISpawner _spawner) {
	}

    void OnDestroy() {
    	RemoveAudios();
    }

    public void PreDisable() {
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

	public void CustomUpdate() {

	}

	public void Attack(bool _attack) {
		m_animator.SetBool("attack", _attack);
	}

	public void Aim(float _blendFactor) {		
		m_animator.SetFloat("aim", _blendFactor);
	}
}
