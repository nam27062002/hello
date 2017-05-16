// PopupTiltCalibrationAnim.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 15/05/2017.
// Copyright (c) 2017 Ubisoft. All rights reserved.

//----------------------------------------------------------------------------//
// INCLUDES																	  //
//----------------------------------------------------------------------------//
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

//----------------------------------------------------------------------------//
// CLASSES																	  //
//----------------------------------------------------------------------------//
/// <summary>
/// Simple popup to show some feedback while the tilt control is being calibrated.
/// </summary>
[RequireComponent(typeof(PopupController))]
public class PopupTiltCalibrationAnim : MonoBehaviour {
	//------------------------------------------------------------------------//
	// CONSTANTS															  //
	//------------------------------------------------------------------------//
	public const string PATH = "UI/Popups/InGame/PF_PopupTiltCalibrationAnim";
	
	//------------------------------------------------------------------------//
	// MEMBERS AND PROPERTIES												  //
	//------------------------------------------------------------------------//
	// References
	[SerializeField] private GameObject m_targetObj = null;
	[SerializeField] private Transform m_finalAnchor = null;

	// Setup
	[Space]
	[SerializeField] private float m_totalDuration = 3f;
	[SerializeField] private float m_actionRadius = 500f;
	[SerializeField] private RangeInt m_iterations = new RangeInt(5, 10);
	[SerializeField] private Ease m_radiusEase = Ease.Linear;

	// Internal
	private List<Vector3> m_path = new List<Vector3>();

	//------------------------------------------------------------------------//
	// GENERIC METHODS														  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// Initialization.
	/// </summary>
	private void Awake() {
		
	}

	/// <summary>
	/// First update call.
	/// </summary>
	private void Start() {

	}

	/// <summary>
	/// Component has been enabled.
	/// </summary>
	private void OnEnable() {

	}

	/// <summary>
	/// Component has been disabled.
	/// </summary>
	private void OnDisable() {

	}

	/// <summary>
	/// Called every frame
	/// </summary>
	private void Update() {

	}

	/// <summary>
	/// Destructor.
	/// </summary>
	private void OnDestroy() {
		
	}

	/// <summary>
	/// A change has occurred on the inspector.
	/// </summary>
	private void OnValidate() {
		m_totalDuration = Mathf.Max(1f, m_totalDuration);
		m_actionRadius = Mathf.Max(1f, m_actionRadius);
	}

	/// <summary>
	/// Draw some debug stuff.
	/// </summary>
	private void OnDrawGizmos() {
		#if UNITY_EDITOR
		// Draw action area
		Handles.matrix = m_finalAnchor.localToWorldMatrix;
		Handles.color = Colors.WithAlpha(Color.green, 0.25f);
		Handles.DrawSolidDisc(Vector3.zero, Vector3.back, m_actionRadius);

		if(m_path.Count > 0) {
			Handles.color = Color.red;
			Handles.DrawAAPolyLine(5f, m_path.ToArray());
			for(int i = 0; i < m_path.Count; i++) {
				Handles.DrawSolidDisc(m_path[i], Vector3.back, 5f);
			}
		}
		#endif
	}

	//------------------------------------------------------------------------//
	// OTHER METHODS														  //
	//------------------------------------------------------------------------//

	//------------------------------------------------------------------------//
	// CALLBACKS															  //
	//------------------------------------------------------------------------//
	/// <summary>
	/// The popup is about to open.
	/// </summary>
	public void OnOpenPreAnimation() {
		// Setup and launch animation
		// Aux vars
		AnimationCurve flashEaseCurve = new AnimationCurve();
		flashEaseCurve.AddKey(0f, 0f);
		flashEaseCurve.AddKey(0.25f, 1f);
		flashEaseCurve.AddKey(1f, 0f);

		// Create sequence
		//Sequence sq = DOTween.Sequence().SetUpdate(UpdateType.Normal, true);
		m_path.Clear();

		// Random position moves, closer to the center everytime
		int iterations = m_iterations.GetRandom();
		float radius = m_actionRadius;
		float speed = m_totalDuration/(float)(iterations - 1);
		Vector2 pos = Vector2.zero;
		Vector2 lastPos = Vector2.zero;
		for(int i = iterations; i >= 0; i--) {
			// Compute new radius, shorter every time
			float delta = (float)i/(float)iterations;
			radius = DOVirtual.EasedValue(0f, m_actionRadius, delta, m_radiusEase);

			// Compute new position at a random angle
			// At least the radius distance, otherwise movement is too short and looks weird
			int protectionLoops = 100;
			do {
				pos = Random.insideUnitCircle.normalized * radius;

				// At least X or Y to the opposite quadrant
				if(Mathf.Sign(pos.x) == Mathf.Sign(lastPos.x)
					&& Mathf.Sign(pos.y) == Mathf.Sign(lastPos.y)) {
					if(Random.value < 0.5f) {
						pos.x *= -1f;
					} else {
						pos.y *= -1f;
					}
				}

				protectionLoops--;
			} while((pos - lastPos).sqrMagnitude < radius * radius && protectionLoops > 0);

			// Record pos!
			lastPos = pos;
			m_path.Add(new Vector3(pos.x, pos.y, 0f));
			//Debug.Log("Iteration " + i + " (" + delta + ")\nradius " + radius + ", pos " + pos);

			// If it's the first iteration, set position immediately
			if(i == iterations) {
				m_targetObj.transform.SetLocalPosX(pos.x);
				m_targetObj.transform.SetLocalPosY(pos.y);
			}
		}

		// Try with a path tween!
		m_targetObj.transform.DOLocalPath(m_path.ToArray(), m_totalDuration, PathType.CatmullRom, PathMode.Ignore, 10, Color.cyan)
			.OnComplete(() => {
				m_targetObj.transform.DOScale(1.25f, 0.4f)
					.SetEase(flashEaseCurve)
					.SetUpdate(UpdateType.Normal, true);

				m_targetObj.GetComponent<UIColorFX>().DOBrightness(0.5f, 0.4f)
					.SetEase(flashEaseCurve)
					.SetUpdate(UpdateType.Normal, true)
					.OnComplete(() => {	GetComponent<PopupController>().Close(true); });
			})
			.SetUpdate(UpdateType.Normal, true)
			.Play();
	}
}