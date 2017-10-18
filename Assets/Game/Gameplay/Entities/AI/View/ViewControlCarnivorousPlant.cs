using System;
using UnityEngine;
using System.Collections.Generic;

public class ViewControlCarnivorousPlant : MonoBehaviour, IViewControl, ISpawnable {
	
	private Animator m_animator;

	private Renderer[] m_renderers;
	private Dictionary<int, List<Material>> m_materials;
	private List<Material> m_materialList;

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

		m_materials = new Dictionary<int, List<Material>>();
		m_materialList = new List<Material>();
		m_renderers = GetComponentsInChildren<Renderer>();

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

				for (int m = 0; m < materials.Length; ++m) {
					Material mat = materials[m];

					m_materialList.Add(mat);
					m_materials[renderID].Add(mat);

					materials[m] = null; // remove all materials to avoid instantiation.
				}
				renderer.sharedMaterials = materials;
			}
		}
    }

	public void Spawn(ISpawner _spawner) {
		// Restore materials
		for (int i = 0; i < m_renderers.Length; i++) {
			int id = m_renderers[i].GetInstanceID();
			Material[] materials = m_renderers[i].sharedMaterials;
			for (int m = 0; m < materials.Length; m++) {				
				materials[m] = m_materials[id][m];
			}
			m_renderers[i].sharedMaterials = materials;
		}
	}

    void OnDestroy() { }
    public void PreDisable() { }
	public void CustomUpdate() { }

	public void Attack(bool _attack) { m_animator.SetBool("attack", _attack); }
	public void Aim(float _blendFactor) { m_animator.SetFloat("aim", _blendFactor); }
}
