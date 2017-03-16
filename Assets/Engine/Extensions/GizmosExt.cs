using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GizmosExt {
	public static float GetGizmoSize(Vector3 position) {
		Camera current = Camera.current;
		position = Gizmos.matrix.MultiplyPoint(position);

		if (current) {
			Transform transform = current.transform;
			Vector3 position2 = transform.position;
			float z = Vector3.Dot(position - position2, transform.TransformDirection(new Vector3(0f, 0f, 1f)));
			Vector3 a = current.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(0f, 0f, z)));
			Vector3 b = current.WorldToScreenPoint(position2 + transform.TransformDirection(new Vector3(1f, 0f, z)));
			float magnitude = (a - b).magnitude;
			return 80f / Mathf.Max(magnitude, 0.0001f);
		}

		return 20f;
	}

	/// <summary>
	/// Draw a label in the 3D scene.
	/// </summary>
	/// <param name="_text">The text to be displayed.</param>
	/// <param name="_worldPos">World position.</param>
	/// <param name="_style">Optional style. Default label style will be used if not defined.</param>
	public static void DrawText(string _text, Vector3 _worldPos, GUIStyle _style = null) {
		// Use Handles. Only usable in Editor
		#if UNITY_EDITOR
		// Only if target world position is visible in current viewport
		Vector3 screenPoint = Camera.current.WorldToScreenPoint(_worldPos);
		if(screenPoint.z <= 0) return;

		// Assign default style if not defined
		if(_style == null) _style = EditorStyles.label;

		// Do some easy maths to center the label
		GUIContent textContent = new GUIContent(_text);
		Vector2 textSize = _style.CalcSize(textContent);
		Vector3 centeredWorldPos = Camera.current.ScreenToWorldPoint(new Vector3(screenPoint.x - textSize.x * 0.5f, screenPoint.y + textSize.y * 0.5f, screenPoint.z));
		Handles.Label(centeredWorldPos, textContent, _style);
		#endif
	}

	/// <summary>
	/// Draw a label in the 3D scene.
	/// </summary>
	/// <param name="_text">The text to be displayed.</param>
	/// <param name="_worldPos">World position.</param>
	/// <param name="_color">Text color.</param>
	/// <param name="_fontSize">Font size.</param>
	/// <param name="_fontStyle">Font style.</param>
	public static void DrawText(string _text, Vector3 _worldPos, Color? _color = null, int _fontSize = 0, FontStyle _fontStyle = FontStyle.Normal) {
		// Only in Editor.
		#if UNITY_EDITOR
		// Create custom label style
		GUIStyle customStyle = new GUIStyle(EditorStyles.label);
		customStyle.fontStyle = _fontStyle;
		if(_color != null) {
			customStyle.normal.textColor = (Color)_color;
		}
		if(_fontSize > 0) {
			customStyle.fontSize = _fontSize;
		}

		// Use style function
		DrawText(_text, _worldPos, customStyle);
		#endif
	}
}
