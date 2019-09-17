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
		new Keyframe(0f, -1f),
		new Keyframe(0.5f, 1f),
		new Keyframe(1f, -1f)
	);
	[SerializeField] private float m_curveScale = 1f;

	[Space]
	[Range(0f, 1f)]
	[SerializeField] private float m_verticalPivot = 0.5f;
	[SerializeField] private float m_verticalOffset = 0f;

	[SerializeField] private bool m_useCustomReferenceSize = false;
	[SerializeField] private float m_customReferenceSize = 200f;

	// Internal refs
	private TMP_Text m_text = null;
	public TMP_Text text {
		get { return m_text; }
	}

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
		// Subscribe to external events
		TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
		TMPro_EventManager.TEXTMESHPRO_UGUI_PROPERTY_EVENT.Add(OnPropertyChanged);

		// Force a refresh
		m_dirty = true;
	}

	/// <summary>
	/// The component has been disabled.
	/// </summary>
	private void OnDisable() {
		// Unsubscribe to external events
		TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
		TMPro_EventManager.TEXTMESHPRO_UGUI_PROPERTY_EVENT.Remove(OnPropertyChanged);

		// Re-compute default text mesh without the curve
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
		// Doesn't work with the ExecuteInEditMode attribute, using OnRenderObject instead
	}

	/// <summary>
	/// Called after camera has rendered the scene. Use instead of Update for 
	/// the ExecuteInEditMode attribute.
	/// </summary>
	private void OnRenderObject() {
		// Check whether we need to regenerate the text mesh
		CheckDirty();
	}

	/// <summary>
	/// Check whether the text mesh needs to be generated or not and does it.
	/// </summary>
	private void CheckDirty() {
		// Component must be active!
		if(!isActiveAndEnabled) return;

		// Check for dirtyness and apply changes
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
	public void Apply() {
		// Generate the default mesh to modify it
		m_text.ForceMeshUpdate();	// [AOC] Do it before resetting the dirty flag! Otherwise the TEXT_CHANGED event will be instantly called resulting in an infinite refresh -_-

		// Populate the textInfo with data we can use and manipulate
		TMP_TextInfo textInfo = m_text.textInfo;

		// Gather some aux data
		int characterCount = textInfo.characterCount;
		if(characterCount == 0) return;

		// Do some aux vars shared between all characters
		Range boundsX = new Range(m_text.bounds.min.x, m_text.bounds.max.x);

		// Override bounds?
		if(m_useCustomReferenceSize) {
			float center = boundsX.center;
			boundsX.min = center - m_customReferenceSize/2f;
			boundsX.max = center + m_customReferenceSize/2f;
		}

		// In order to have the same curvature regardless of the number of characters, compute correction factor
		float correctedCurveScale = m_curveScale * 0.1f * boundsX.distance;	// [AOC] In order to have easier tunable numbers in the inspector, curve scale is multiplier by 10

		// Traverse characters and modify all vertices
		Vector3[] vertices;
		Matrix4x4 matrix;
		Vector3 horizontal = new Vector3(1f, 0f, 0f);
		int chars = Mathf.Min(characterCount, textInfo.characterInfo.Length);
		for(int i = 0; i < chars; i++) {
			// Skip if vertex is not visible
			if(!textInfo.characterInfo[i].isVisible) {
				continue;
			}

			// Gather some aux data
			int vertexIndex = textInfo.characterInfo[i].vertexIndex;
			int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
			vertices = textInfo.meshInfo[materialIndex].vertices;
			Vector3 midPoint = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x)/2f, (vertices[vertexIndex + 0].y + vertices[vertexIndex + 2].y)/2f);

			// Compute new position based on the animation curve, using the character's relative position to the bounds of the mesh
			float x0Delta = boundsX.InverseLerp(midPoint.x);
			float y0Delta = m_curve.Evaluate(x0Delta);
			Vector3 p0 = new Vector2(midPoint.x, y0Delta * correctedCurveScale);

			// Compute a second point following the first one to be able to compute the rotation of the character using tangent
			float x1Delta = x0Delta + 0.01f;
			float y1Delta = m_curve.Evaluate(x1Delta);
			Vector3 p1 = new Vector2(boundsX.Lerp(x1Delta), y1Delta * correctedCurveScale);

			// Apply vertical offset to both points
			p0.y += m_verticalOffset;
			p1.y += m_verticalOffset;

			// Compute the angle of rotation using tangent
			Vector3 tangent = p1 - p0;
			float dot = Mathf.Acos(Vector3.Dot(horizontal, tangent.normalized)) * Mathf.Rad2Deg;
			Vector3 cross = Vector3.Cross(horizontal, tangent);
			float angle = cross.z > 0f ? dot : 360f - dot;

			// Compute transformation matrix and apply to each vertex in the character
			matrix = Matrix4x4.TRS(new Vector3(0f, p0.y, 0f), Quaternion.Euler(0f, 0f, angle), Vector3.one);

			// Temporarily move all vertices to the center so the matrix is properly applied
			//Vector3 offset = new Vector2(midPoint.x, textInfo.characterInfo[i].baseLine);		// [AOC] Use character's base rather than raw positions
			Vector3 offset = new Vector2(midPoint.x, Mathf.Lerp(vertices[vertexIndex + 0].y, vertices[vertexIndex + 2].y, m_verticalPivot));
			vertices[vertexIndex + 0] -= offset;
			vertices[vertexIndex + 1] -= offset;
			vertices[vertexIndex + 2] -= offset;
			vertices[vertexIndex + 3] -= offset;

			// Apply matrix
			vertices[vertexIndex + 0] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 0]);
			vertices[vertexIndex + 1] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 1]);
			vertices[vertexIndex + 2] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 2]);
			vertices[vertexIndex + 3] = matrix.MultiplyPoint3x4(vertices[vertexIndex + 3]);

			// Restore temp offset
			vertices[vertexIndex + 0] += offset;
			vertices[vertexIndex + 1] += offset;
			vertices[vertexIndex + 2] += offset;
			vertices[vertexIndex + 3] += offset;

			// Debug output
			/*Debug.Log(textInfo.characterInfo[i].character + "\n" +
				"x0Delta = " + x0Delta + "\n" +
				"y0Delta = " + y0Delta + "\n" +
				"p0 = " + p0 + "\n" +
				"\n" +
				"x1Delta = " + x1Delta + "\n" +
				"y1Delta = " + y1Delta + "\n" +
				"p1 = " + p1 + "\n" +
				"\n" +
				"angle  = " + angle + "\n"
			);*/
		}

		// Upload the mesh with the revised information
		m_text.UpdateVertexData();

		// Mark the object as dirty in the editor so it gets repainted
		#if UNITY_EDITOR
			UnityEditor.EditorUtility.SetDirty(m_text);
		#endif

		// No longer dirty!
		m_dirty = false;
	}

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// A Text Mesh Pro object has changed.
	/// </summary>
	/// <param name="_obj">The text object that triggered the event.</param>
	private void OnTextChanged(Object _obj) {
		// Only if it's the target text
		if(_obj != m_text) return;

		// Mark as dirty!
		m_dirty = true;
	}

	/// <summary>
	/// A property for a Text Mesh Pro object has changed.
	/// </summary>
	/// <param name="_b"></param>
	/// <param name="_text">The text object that triggered the event.</param>
	private void OnPropertyChanged(bool _b, TextMeshProUGUI _text) {
		// Only if it's the target text
		if(_text != m_text) return;

		// Mark as dirty!
		m_dirty = true;
	}
}