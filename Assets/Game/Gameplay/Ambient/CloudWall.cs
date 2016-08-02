// CloudWall.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 02/08/2016.
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
/// Class to help create and modify a cloud wall.
/// </summary>
public class CloudWall : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	[SerializeField] private Vector3 m_size = new Vector3(1f, 1f, 1f);
	[SerializeField][Range(1, 100)] private int m_numSections = 1;
	[SerializeField] private GameObject m_wallSectionPrefab = null;
	
	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {

	}

	/// <summary>
	/// Find all sections in the wall.
	/// </summary>
	/// <returns>The list of sections forming this wall.</returns>
	List<GameObject> GetSections() {
		// Consider a section any children object with a mesh filter (quad)
		MeshFilter[] children = this.GetComponentsInChildren<MeshFilter>();
		List<GameObject> sections = new List<GameObject>(children.Length);
		for(int i = 0; i < children.Length; i++) {
			sections.Add(children[i].gameObject);
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
	/// Generate the wall!
	/// </summary>
	public void Generate() {
		// Clear if prefab is not set
		if(m_wallSectionPrefab == null) {
			Debug.Log("CloudWall: Section prefab not set, clearing the wall.");
			Clear();
			return;
		}

		// Detect current sections
		List<GameObject> sections = GetSections();

		// Add/remove sections as required
		int sectionsOffset = sections.Count - m_numSections;
		if(sectionsOffset < 0) {
			// Add new sections
			while(sectionsOffset < 0) {
				GameObject newSection = GameObject.Instantiate(m_wallSectionPrefab);
				newSection.transform.SetParent(this.transform);
				newSection.name = m_wallSectionPrefab.name;
				sections.Add(newSection);
				sectionsOffset++;
			}
		} else if(sectionsOffset > 0) {
			// Remove sections
			for(int i = 0; i < sectionsOffset; i++) {
				GameObject.DestroyImmediate(sections[i]);
				sections[i] = null;
			}
			sections.RemoveRange(0, sectionsOffset);
		}

		// Iterate all sections and set to each section the proper child index, position and scale
		Vector3 sectionSize = new Vector3(m_size.x/m_numSections, m_size.y, m_size.z);
		Vector3 pos = new Vector3(-m_size.x/2f + sectionSize.x/2f, 0f, 0f);	// Section's anchor is the middle-center, start at -x
		for(int i = 0; i < sections.Count; i++) {
			// Reorder hierarchy so sections are sorted from left to right
			sections[i].transform.SetSiblingIndex(i);

			// Resize and put in position
			sections[i].transform.localScale = sectionSize;	// Quads are 1x1 units, so scale correspond to world units ^_^
			sections[i].transform.localPosition = pos;

			// Advance position
			pos.x += sectionSize.x;
		}
	}

	/// <summary>
	/// Draw helper stuff on the scene.
	/// </summary>
	private void OnDrawGizmos() {
		// Draw size borders
		Gizmos.color = Colors.maroon;
		Gizmos.matrix = this.transform.localToWorldMatrix;
		Gizmos.DrawWireCube(Vector3.zero, m_size);
	}
}