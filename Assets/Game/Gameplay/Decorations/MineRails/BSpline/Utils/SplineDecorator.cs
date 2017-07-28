using UnityEngine;
using BSpline;

public class SplineDecorator : MonoBehaviour {

	public BezierSpline spline;

	public int distance;

	public bool lookForward;

	public Transform item;

	private void Awake () {
		if (distance <= 0 || item == null) {
			return;
		}
		float step = 3f/9f;
		for (float t = 0f; t <= 1f; t += step) {
			Transform itemTr = Instantiate(item) as Transform;
			Vector3 position = spline.GetPoint(t);
			Vector3 derivative = spline.GetVelocity(t);

			itemTr.localPosition = position;
			if (lookForward) {
				itemTr.LookAt(position + derivative.normalized);
			}
			itemTr.parent = transform;
		}
	}
}