﻿using UnityEngine;

public class SplineWalker : MonoBehaviour {

	public enum SplineWalkerMode {
		Once,
		Loop,
		PingPong
	}

	public BSpline.BezierSpline spline;
	public bool lookForward;

	public float duration;
	public SplineWalkerMode mode;



	private bool goingForward = true;
	private float progress;


	private void Update () {
		if (goingForward) {
			progress += Time.deltaTime / duration;
			if (progress > 1f) {
				if (mode == SplineWalkerMode.Once) {
					progress = 1f;
				} else if (mode == SplineWalkerMode.Loop) {
					progress -= 1f;
				} else {
					progress = 2f - progress;
					goingForward = false;
				}
			}
		} else {
			progress -= Time.deltaTime / duration;
			if (progress < 0f) {
				progress = -progress;
				goingForward = true;
			}
		}

		Vector3 position = spline.GetPoint(progress, true);
		transform.localPosition = position;
		if (lookForward) {
			transform.LookAt(position + spline.GetDirection(progress, true));
		}
	}
}