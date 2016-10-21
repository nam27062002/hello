// PrefabLayout.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 11/08/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Small helper to tile prefabs.
/// The prefabs must have a Mesh of any type to be valid for tiling.
/// TODO:
/// 	- Auto-scale toggle per axis (plus default scale if not)
/// 	- Work with existing children objects rather than creating/destroying children (alternate mode)
/// </summary>
public class PrefabLayout : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	private enum GizmoType {
		NONE,
		LINE,
		SOLID,
		SOLID_WITH_FRAME
	}

	[System.Serializable]
	public class AxisSetup {
		public float totalSize = 1f;
		public float padding = 0f;
		public int numSections = 1;
		public bool autoSections = true;
		public bool scaleToFit = false;
	}

	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private GameObject m_sectionPrefab = null;
	[SerializeField] private AxisSetup[] m_setup = new AxisSetup[3];

	// Debug: Total bounds, section bounds, padding, auto padding
	[SerializeField] private Color[] m_debugColors = new Color[4] {
		Colors.paleYellow, 
		Colors.orange, 
		new Color(0.5f, 1f, 1f, 0.25f), 
		new Color(1f, 0f, 0f, 0.25f)
	};
	[SerializeField] private GizmoType[] m_debugGizmoTypes = new GizmoType[4] {
		GizmoType.LINE, 
		GizmoType.LINE, 
		GizmoType.SOLID_WITH_FRAME, 
		GizmoType.SOLID_WITH_FRAME
	};

	// Internal
	private List<GameObject> m_sections = new List<GameObject>();
	private Vector3 m_sectionSize = Vector3.one;	// Not including padding
	private Vector3 m_sectionScale = Vector3.one;
	private Vector3 m_transformOffset = Vector3.zero;
	private Vector3 m_autoPadding = Vector3.zero;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// A change has occurred on the inspector, update object!
	/// </summary>
	private void OnValidate() {
		// Can't use OnValidate since Unity doesn't allow us to Destroy/DestroyImmediate during the OnValidate event -_-
		// Use a custom editor instead
		//Refresh();
	}

	/// <summary>
	/// Find all sections in the wall.
	/// </summary>
	/// <returns>The list of sections forming this wall.</returns>
	List<GameObject> GetSections() {
		// Consider a section any direct child
		List<GameObject> sections = new List<GameObject>(this.transform.childCount);
		for(int i = 0; i < this.transform.childCount; i++) {
			sections.Add(this.transform.GetChild(i).gameObject);
		}
		return sections;
	}

	/// <summary>
	/// Destroy all sections
	/// </summary>
	public void Clear() {
		// Remove sections
		List<GameObject> sections = GetSections();
		for(int i = 0; i < sections.Count; i++) {
			GameObject.DestroyImmediate(sections[i]);
			sections[i] = null;
		}
	}

	/// <summary>
	/// Refresh the layout!
	/// </summary>
	public void Refresh() {
		// Clear if prefab is not set
		if(m_sectionPrefab == null) {
			//Debug.Log("PrefabLayout: Section prefab not set, clearing the object.");
			Clear();
			return;
		}

		// Clear as well if the prefab doesn't have any Renderer component
		Renderer[] childRenderers = m_sectionPrefab.GetComponentsInChildren<Renderer>();
		if(childRenderers.Length == 0) {
			//Debug.Log("PrefabLayout: Section prefab doesn't contain a Renderer component, clearing the object.");
			Clear();
			return;
		}

		// Compute original section bounds, including all children renderers
		Bounds sectionBounds = childRenderers[0].bounds;
		for(int i = 1; i < childRenderers.Length; i++) {
			sectionBounds.Encapsulate(childRenderers[i].bounds);
		}

		// Compute some aux vars for each axis
		for(int i = 0; i < 3; i++) {
			// Dynamic or fixed amount of sections?
			if(m_setup[i].autoSections) {
				// Compute required amount of sections to fill each axis without changing the scale of the prefab
				m_setup[i].numSections = Mathf.FloorToInt((m_setup[i].totalSize + m_setup[i].padding)/(sectionBounds.size[i] + m_setup[i].padding));	// Add one auto padding at the end just for computing purposes
				m_setup[i].numSections = Mathf.Max(m_setup[i].numSections, 1);	// At least one!
			}
				
			// Compute the size of a single section, without any padding whatsoever
			m_sectionSize[i] = (m_setup[i].totalSize + m_setup[i].padding) / m_setup[i].numSections - m_setup[i].padding;	// Add one auto padding at the end just for computing purposes

			// If scale to fit is enabled, compute target scale
			// Otherwise, compute auto-padding to compensate for the empty space
			if(m_setup[i].scaleToFit) {
				m_sectionScale[i] = m_sectionSize[i]/sectionBounds.size[i];
				m_autoPadding[i] = 0f;
			} else {
				m_sectionScale[i] = 1f;
				m_autoPadding[i] = ((m_setup[i].totalSize + m_setup[i].padding) - (m_setup[i].numSections * (m_sectionSize[i] + m_setup[i].padding))) / m_setup[i].numSections;	// +1 because to distribute extra padding space equally at the start/end
			}
		}

		// Compute offset between transform's position and actual bounds center of a single section
		m_transformOffset = sectionBounds.center - m_sectionPrefab.transform.position;
		m_transformOffset.Scale(m_sectionScale);

		// Detect current sections
		m_sections = GetSections();

		// Add/remove sections as required
		int totalNumSections = m_setup[0].numSections * m_setup[1].numSections * m_setup[2].numSections;
		int sectionsCountOffset = m_sections.Count - totalNumSections;
		if(sectionsCountOffset < 0) {
			// Add new sections
			while(sectionsCountOffset < 0) {
				GameObject newSection = GameObject.Instantiate(m_sectionPrefab);
				newSection.transform.SetParent(this.transform);
				newSection.name = m_sectionPrefab.name;
				m_sections.Add(newSection);
				sectionsCountOffset++;
			}
		} else if(sectionsCountOffset > 0) {
			// Remove sections
			for(int i = 0; i < sectionsCountOffset; i++) {
				GameObject.DestroyImmediate(m_sections[i]);
				m_sections[i] = null;
			}
			m_sections.RemoveRange(0, sectionsCountOffset);
		}

		// Iterate all sections and set to each section the proper child index, position and scale
		// Section's anchor is the middle-center, start at -x/2, -y/2, -z/2
		// Neither first nor last sections have padding
		Vector3 originPos = new Vector3(
			-m_setup[0].totalSize/2f + m_sectionSize.x/2f + m_autoPadding.x/2f,
			-m_setup[1].totalSize/2f + m_sectionSize.y/2f + m_autoPadding.y/2f,
			-m_setup[2].totalSize/2f + m_sectionSize.z/2f + m_autoPadding.z/2f
		);
		Vector3 pos = originPos;

		// Fill by axes
		int sectionIdx = 0;
		for(int y = 0; y < m_setup[1].numSections; y++) {
			// Reset Z
			pos.z = originPos.z;

			for(int z = 0; z < m_setup[2].numSections; z++) {
				// Reset X
				pos.x = originPos.x;
				for(int x = 0; x < m_setup[0].numSections; x++) {
					// Reorder hierarchy so sections are sorted
					m_sections[sectionIdx].transform.SetSiblingIndex(sectionIdx);

					// Resize and put in position
					m_sections[sectionIdx].transform.localScale = m_sectionScale;
					m_sections[sectionIdx].transform.localPosition = pos - m_transformOffset;

					// Advance to next section
					sectionIdx++;

					// Advance position - neither first nor last sections have padding
					pos.x += m_sectionSize.x + m_autoPadding.x;
					if(x < m_setup[0].numSections - 1) pos.x += m_setup[0].padding;
				}

				// Advance position - neither first nor last sections have padding
				pos.z += m_sectionSize.z + m_autoPadding.z;
				if(z < m_setup[2].numSections - 1) pos.z += m_setup[2].padding;
			}

			// Advance position - neither first nor last sections have padding
			pos.y += m_sectionSize.y + m_autoPadding.y;
			if(y < m_setup[1].numSections - 1) pos.y += m_setup[1].padding;
		}
	}

	/// <summary>
	/// Draw helper stuff on the scene.
	/// </summary>
	private void OnDrawGizmosSelected() {
		// Skip if there are no sections
		if(m_sections.Count == 0) return;

		// Work in local space
		Gizmos.matrix = this.transform.localToWorldMatrix;

		// Draw paddings
		// Iterate by axis
		int sectionIdx = 0;
		Vector3 drawSize = Vector3.one;
		Vector3 drawPos = Vector3.zero;
		Vector3 sectionPos = Vector3.zero;
		for(int y = 0; y < m_setup[1].numSections; y++) {
			for(int z = 0; z < m_setup[2].numSections; z++) {
				for(int x = 0; x < m_setup[0].numSections; x++) {
					// Aux vars
					sectionPos = m_sections[sectionIdx].transform.localPosition + m_transformOffset;

					// Auto padding
					if(m_debugGizmoTypes[3] != GizmoType.NONE) {
						// Draw around the section box
						Gizmos.color = m_debugColors[3];

						// Left
						drawSize.Set(m_autoPadding.x/2f, m_sectionSize.y, m_sectionSize.z);
						drawPos = sectionPos;
						drawPos.x -= m_sectionSize.x/2f + drawSize.x/2f;
						DoCubeGizmo(drawPos, drawSize, m_debugGizmoTypes[3]);

						// Right
						drawPos = sectionPos;
						drawPos.x += m_sectionSize.x/2f + drawSize.x/2f;
						DoCubeGizmo(drawPos, drawSize, m_debugGizmoTypes[3]);

						// Top
						drawSize.Set(m_sectionSize.x, m_autoPadding.y/2f, m_sectionSize.z);
						drawPos = sectionPos;
						drawPos.y -= m_sectionSize.y/2f + drawSize.y/2f;
						DoCubeGizmo(drawPos, drawSize, m_debugGizmoTypes[3]);

						// Bottom
						drawPos = sectionPos;
						drawPos.y += m_sectionSize.y/2f + drawSize.y/2f;
						DoCubeGizmo(drawPos, drawSize, m_debugGizmoTypes[3]);

						// Front
						drawSize.Set(m_sectionSize.x, m_sectionSize.y, m_autoPadding.z/2f);
						drawPos = sectionPos;
						drawPos.z -= m_sectionSize.z/2f + drawSize.z/2f;
						DoCubeGizmo(drawPos, drawSize, m_debugGizmoTypes[3]);

						// Back
						drawPos = sectionPos;
						drawPos.z += m_sectionSize.z/2f + drawSize.z/2f;
						DoCubeGizmo(drawPos, drawSize, m_debugGizmoTypes[3]);
					}

					// Manual padding
					if(m_debugGizmoTypes[2] != GizmoType.NONE) {
						// Set color
						Gizmos.color = m_debugColors[2];

						// X Padding
						if(x < m_setup[0].numSections - 1) {
							drawSize.Set(
								m_setup[0].padding,
								m_sectionSize.y + m_autoPadding.y,
								m_sectionSize.z + m_autoPadding.z
							);
							drawPos = sectionPos;
							drawPos.x = drawPos.x + m_sectionSize.x/2f + m_autoPadding.x/2f + drawSize.x/2f; 	// Draw after the section and the auto-padding
							DoCubeGizmo(drawPos, drawSize, m_debugGizmoTypes[2]);
						}

						// Y Padding
						if(y < m_setup[1].numSections - 1) {
							drawSize.Set(
								m_sectionSize.x + m_autoPadding.x,
								m_setup[1].padding,
								m_sectionSize.z + m_autoPadding.z
							);
							drawPos = sectionPos;
							drawPos.y = drawPos.y + m_sectionSize.y/2f + m_autoPadding.y/2f + drawSize.y/2f; 	// Draw after the section and the auto-padding
							DoCubeGizmo(drawPos, drawSize, m_debugGizmoTypes[2]);
						}

						// Z Padding
						if(z < m_setup[2].numSections - 1) {
							drawSize.Set(
								m_sectionSize.x + m_autoPadding.x,
								m_sectionSize.y + m_autoPadding.y,
								m_setup[2].padding
							);
							drawPos = sectionPos;
							drawPos.z = drawPos.z + m_sectionSize.z/2f + m_autoPadding.z/2f + drawSize.z/2f; 	// Draw after the section and the auto-padding
							DoCubeGizmo(drawPos, drawSize, m_debugGizmoTypes[2]);
						}
					}

					// Move on
					sectionIdx++;
				}
			}
		}

		// Draw individual cubes
		if(m_debugGizmoTypes[1] != GizmoType.NONE) {
			Gizmos.color = m_debugColors[1];
			for(int i = 0; i < m_sections.Count; i++) {
				DoCubeGizmo(m_sections[i].transform.localPosition + m_transformOffset, m_sectionSize, m_debugGizmoTypes[1]);
			}
		}

		// Draw size borders
		if(m_debugGizmoTypes[0] != GizmoType.NONE) {
			Gizmos.color = m_debugColors[0];
			DoCubeGizmo(
				Vector3.zero,
				new Vector3(m_setup[0].totalSize, m_setup[1].totalSize, m_setup[2].totalSize) * 1.01f, // A litle bit bigger so we can see internal divisions as well
				m_debugGizmoTypes[0]
			);
		}
	}

	/// <summary>
	/// Draw a cube gizmo with the given data.
	/// </summary>
	/// <param name="_center">Center of the cube in the current Gizmo's matrix.</param>
	/// <param name="_size">Size of the cube in the current Gizmo's matrix.</param>
	/// <param name="_type">Type of rendering.</param>
	private void DoCubeGizmo(Vector3 _center, Vector3 _size, GizmoType _type) {
		// Just do it, depending on rendering type
		switch(_type) {
			case GizmoType.LINE: {
				Gizmos.DrawWireCube(_center, _size);
			} break;

			case GizmoType.SOLID: {
				Gizmos.DrawCube(_center, _size);
			} break;

			case GizmoType.SOLID_WITH_FRAME: {
				Gizmos.DrawCube(_center, _size);
				Gizmos.DrawWireCube(_center, _size);
			} break;
		}
	}
}