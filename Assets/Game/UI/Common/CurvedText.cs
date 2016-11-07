// CurvedText.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 07/11/2016.
// Copyright (c) 2016 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using TMPro;

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Script to curve a TMP text along a curve.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
[ExecuteInEditMode]
public class CurvedText : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// Exposed
	[SerializeField] private AnimationCurve m_curve = new AnimationCurve(
		new Keyframe(0f, 0f),
		new Keyframe(0.5f, 100f),
		new Keyframe(1f, 0f)
	);
	[SerializeField] private float m_curveScale = 1f;

	// Internal refs
	private TMP_Text m_text = null;

	// Aux vars
	private bool m_dirty = true;

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		// Get refereneces
		m_text = GetComponent<TMP_Text>();
	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {
		// Force a refresh
		m_dirty = true;
	}

	/// <summary>
	/// The component has been disabled.
	/// </summary>
	private void OnDisable() {
		m_text.ForceMeshUpdate();
	}

	/// <summary>
	/// Something has changed on the inspector.
	/// </summary>
	private void OnValidate() {
		m_dirty = true;
	}
	
	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {
		// Component must be active!
		if(!isActiveAndEnabled) return;

		// Check for dirtyness and apply changes
		// [AOC] TODO!! Don't check every frame (performance)?
		if(m_dirty || m_text.havePropertiesChanged || transform.hasChanged) {
			Apply();
			transform.hasChanged = false;
		}
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Apply the curve to the text.
	/// </summary>
	private void Apply() {
		// No longer dirty
		m_dirty = false;
		
		// Force curve wrap mode
		m_curve.preWrapMode = WrapMode.Clamp;
		m_curve.postWrapMode = WrapMode.Clamp;

		// Generate the mesh and populate the textInfo with data we can use and manipulate.
		m_text.ForceMeshUpdate();

		// Gather some aux data
		TMP_TextInfo textInfo = m_text.textInfo;

		int characterCount = textInfo.characterCount;
		if (characterCount == 0) return;

		float boundsMinX = m_text.bounds.min.x;
		float boundsMaxX = m_text.bounds.max.x;

		// Traverse and modify all vertices
		Vector3[] vertices;
		Matrix4x4 matrix;
		Vector3 horizontal = new Vector3(1, 0, 0);
		for(int i = 0; i < characterCount; i++) {
			// Skip if vertex is not visible
			if(!textInfo.characterInfo[i].isVisible) {
				continue;
			}

			// Gather some aux data
			int vertexIndex = textInfo.characterInfo[i].vertexIndex;
			int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
			vertices = textInfo.meshInfo[materialIndex].vertices;

			// Compute the baseline mid point for each character
			Vector3 offsetToMidBaseline = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, textInfo.characterInfo[i].baseLine);
			//float offsetY = m_curve.Evaluate((float)i / characterCount + i / 50f); // Random.Range(-0.25f, 0.25f);

			// Apply offset to adjust our pivot point.
			vertices[vertexIndex + 0] += -offsetToMidBaseline;
			vertices[vertexIndex + 1] += -offsetToMidBaseline;
			vertices[vertexIndex + 2] += -offsetToMidBaseline;
			vertices[vertexIndex + 3] += -offsetToMidBaseline;

			// Compute the angle of rotation for each character based on the animation curve
			float x0 = (offsetToMidBaseline.x - boundsMinX) / (boundsMaxX - boundsMinX); // Character's position relative to the bounds of the mesh.
			float x1 = x0 + 0.0001f;
			float y0 = m_curve.Evaluate(x0) * m_curveScale;
			float y1 = m_curve.Evaluate(x1) * m_curveScale;

			//Vector3 normal = new Vector3(-(y1 - y0), (x1 * (boundsMaxX - boundsMinX) + boundsMinX) - offsetToMidBaseline.x, 0);
			Vector3 tangent = new Vector3(x1 * (boundsMaxX - boundsMinX) + boundsMinX, y1) - new Vector3(offsetToMidBaseline.x, y0);

			float dot = Mathf.Acos(Vector3.Dot(horizontal, tangent.normalized)) * 57.2957795f;
			Vector3 cross = Vector3.Cross(horizontal, tangent);
			float angle = cross.z > 0 ? dot : 360 - dot;

			// Compute transformation matrix and apply to each vertex in the character
			matrix = Matrix4x4.TRS(new Vector3(0, y0, 0), Quaternion.Euler(0, 0, angle), Vector3.one);

			vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
			vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
			vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
			vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);

			vertices[vertexIndex + 0] += offsetToMidBaseline;
			vertices[vertexIndex + 1] += offsetToMidBaseline;
			vertices[vertexIndex + 2] += offsetToMidBaseline;
			vertices[vertexIndex + 3] += offsetToMidBaseline;
		}

		// Upload the mesh with the revised information
		m_text.UpdateVertexData();
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
}