using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arc{

	public Vector3 center;
	public Vector3 centerLine;
	public float radius;
	public float angle;

	public bool IsInside( Vector2 _point )
	{
		return MathUtils.TestArcVsPoint(  center, angle, radius, centerLine, _point);
	}

	public bool Overlaps( CircleArea2D _circle )
	{
		return MathUtils.TestArcVsCircle( center, angle, radius, centerLine, _circle.center, _circle.radius);
	}

	public void DrawGizmos()
	{
		// Arc Drawing
		Gizmos.DrawWireSphere(center, radius);
		Debug.DrawLine(center, center + centerLine * radius);
		Vector3 dUp = centerLine.RotateXYDegrees(angle/2.0f);
		Debug.DrawLine( center, center + (dUp * radius));
		Vector3 dDown = centerLine.RotateXYDegrees(-angle/2.0f);
		Debug.DrawLine( center, center + (dDown * radius));
	}
	
}
