//--------------------------------------------------------------------------------
// Util.cs
//--------------------------------------------------------------------------------
// This is a home for some static functions and constants etc.
//--------------------------------------------------------------------------------

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Util
{
	// public constants
	public const float TwoPi =						2.0f*Mathf.PI;
	
	// default parameters
	public const float m_defaultMaxDampingScale =	0.125f;

	//----------------------------------------------------------------------------
	// Angle functions
	//----------------------------------------------------------------------------

	// Get an angle into 0-360 range
	public static float FixAngleDegrees(float ang)
	{
		// Is this really the best way to do this?
		while(ang >= 360.0f)
			ang -= 360.0f;
		while(ang < 0)
			ang += 360.0f;
		return ang;
	}
	
	// As above, in radians
	public static float FixAngleRadians(float ang)
	{
		while(ang >= TwoPi)
			ang -= TwoPi;
		while(ang < 0)
			ang += TwoPi;
		return ang;
	}
	
	// Get an angle into -180 to +180 range
	public static float FixAnglePlusMinusDegrees(float ang)
	{
		while(ang >= 180.0f)
			ang -= 360.0f;
		while(ang < -180.0f)
			ang += 360.0f;
		return ang;
	}
	
	// As above, in radians
	public static float FixAnglePlusMinusRadians(float ang)
	{
		while(ang >= Mathf.PI)
			ang -= TwoPi;
		while(ang < -Mathf.PI)
			ang += TwoPi;
		return ang;
	}
	
	//----------------------------------------------------------------------------
	// Blending and damping functions
	//----------------------------------------------------------------------------
	
	// Copy of MoveValueWithDamping from old engine.  The MathF class provides a MoveTowards method, but no damped version.
	public static float MoveTowardsWithDamping(float from, float to, float step, float dampingRange, float maxDampingScale = m_defaultMaxDampingScale)
	{
		if(to > from)
		{
			if((to-from) < dampingRange)
			{
				float slowdown = (to-from)/dampingRange;
				slowdown = Mathf.Clamp(slowdown, maxDampingScale, 1.0f);
				step *= slowdown;
			}
			from = Mathf.Min(from+step, to);
		}
		else if(to < from)
		{
			if((from-to) < dampingRange)
			{
				float slowdown = (from-to)/dampingRange;
				slowdown = Mathf.Clamp(slowdown, maxDampingScale, 1.0f);
				step *= slowdown;
			}
			from = Mathf.Max(from-step, to);
		}
		return from;
	}
	
	// Equivalent version for MoveAngleWithDamping / MathF.MoveTowardsAngle
	public static float MoveTowardsAngleRadiansWithDamping(float from, float to, float step, float dampingRange, float maxDampingScale = m_defaultMaxDampingScale)
	{
		float d = FixAnglePlusMinusRadians(to-from);
		float s = MoveTowardsWithDamping(0.0f, d, step, dampingRange, maxDampingScale);
		return FixAngleRadians(from+s);
	}
	
	public static float MoveTowardsAngleDegreesWithDamping(float from, float to, float step, float dampingRange, float maxDampingScale = m_defaultMaxDampingScale)
	{
		float d = FixAnglePlusMinusDegrees(to-from);
		float s = MoveTowardsWithDamping(0.0f, d, step, dampingRange, maxDampingScale);
		return FixAngleDegrees(from+s);
	}
	
	// Various versions for using on vectors.
	// Vector2 and Vector3 already contain a MoveTowards method.  Here we have damping versions, and also versions that work on a Vector3 with only the X/Y
	// components (seems to be easier to work with than Vector2).
	public static void MoveTowardsVector2WithDamping(ref Vector2 v, ref Vector2 target, float step, float dampingRange, float maxDampingScale = m_defaultMaxDampingScale)
	{
		v.x = MoveTowardsWithDamping(v.x, target.x, step, dampingRange, maxDampingScale);
		v.y = MoveTowardsWithDamping(v.y, target.y, step, dampingRange, maxDampingScale);
	}
	
	public static void MoveTowardsVector3(ref Vector3 v, ref Vector3 target, float step)
	{
		v.x = Mathf.MoveTowards(v.x, target.x, step);
		v.y = Mathf.MoveTowards(v.y, target.y, step);
		v.z = Mathf.MoveTowards(v.z, target.z, step);
	}
	
	public static void MoveTowardsVector3XY(ref Vector3 v, ref Vector3 target, float step)
	{
		v.x = Mathf.MoveTowards(v.x, target.x, step);
		v.y = Mathf.MoveTowards(v.y, target.y, step);
	}
	
	public static void MoveTowardsVector3XYWithDamping(ref Vector3 v, ref Vector3 target, float step, float dampingRange, float maxDampingScale = m_defaultMaxDampingScale)
	{
		v.x = MoveTowardsWithDamping(v.x, target.x, step, dampingRange, maxDampingScale);
		v.y = MoveTowardsWithDamping(v.y, target.y, step, dampingRange, maxDampingScale);
	}
	
	public static void MoveTowardsVector3WithDamping(ref Vector3 v, ref Vector3 target, float step, float dampingRange, float maxDampingScale = m_defaultMaxDampingScale)
	{
		v.x = MoveTowardsWithDamping(v.x, target.x, step, dampingRange, maxDampingScale);
		v.y = MoveTowardsWithDamping(v.y, target.y, step, dampingRange, maxDampingScale);
		v.z = MoveTowardsWithDamping(v.z, target.z, step, dampingRange, maxDampingScale);
	}
	
	public static void FilterValue(ref float value, ref float oldValue, float filter)
	{
		value = (value*filter) + (oldValue*(1.0f-filter));
		oldValue = value;
	}
	
	public static void FilterVector3(ref Vector3 vec, ref Vector3 oldVec, float filter)
	{
		vec.x = (vec.x*filter) + (oldVec.x*(1.0f-filter));
		vec.y = (vec.y*filter) + (oldVec.y*(1.0f-filter));
		vec.z = (vec.z*filter) + (oldVec.z*(1.0f-filter));
		oldVec = vec;
	}
	
	public static void FilterVector3XY(ref Vector3 vec, ref Vector3 oldVec, float filter)
	{
		vec.x = (vec.x*filter) + (oldVec.x*(1.0f-filter));
		vec.y = (vec.y*filter) + (oldVec.y*(1.0f-filter));
		oldVec = vec;
	}
	
	// Remap a value from one range (in0 -> in1, CLAMPED) to a different range (out0 -> out1).
	// Use this to interpolate between 2 values (out0, out1) based on where some other number (value) sits
	// between two other values (in0, in1).
	public static float Remap(float value, float in0, float in1, float out0, float out1)
	{
		float d = in1-in0;
		//FGAssert.Assert(d != 0.0f);
		float t = (value-in0)/d;
		t = Mathf.Clamp01(t);
		return Mathf.Lerp(out0, out1, t);
	}
	
	// As above, but use to interpolate between two vectors.
	public static Vector3 RemapToVector3(float value, float in0, float in1, Vector3 out0, Vector3 out1)
	{
		float d = in1-in0;
		//FGAssert.Assert(d != 0.0f);
		float t = (value-in0)/d;
		t = Mathf.Clamp01(t);
		float x = Mathf.Lerp(out0.x, out1.x, t);
		float y = Mathf.Lerp(out0.y, out1.y, t);
		float z = Mathf.Lerp(out0.z, out1.z, t);
		return new Vector3(x, y, z);
	}
	
	public static Vector3 RemapToVector3XY(float value, float in0, float in1, Vector3 out0, Vector3 out1)
	{
		float d = in1-in0;
		//FGAssert.Assert(d != 0.0f);
		float t = (value-in0)/d;
		t = Mathf.Clamp01(t);
		float x = Mathf.Lerp(out0.x, out1.x, t);
		float y = Mathf.Lerp(out0.y, out1.y, t);
		return new Vector3(x, y, out0.z);
	}
	
	//----------------------------------------------------------------------------
	// Debug draw functions
	//----------------------------------------------------------------------------

	public static void DrawDebugPoint(Vector3 p, Color color, float r=0.5f)
	{
		Vector3 s0 = new Vector3(p.x-r, p.y, p.z);
		Vector3 e0 = new Vector3(p.x+r, p.y, p.z);
		Vector3 s1 = new Vector3(p.x, p.y-r, p.z);
		Vector3 e1 = new Vector3(p.x, p.y+r, p.z);
		Debug.DrawLine(s0, e0, color, 0.0f, false);
		Debug.DrawLine(s1, e1, color, 0.0f, false);
	}
	public static void DrawDebugPoint(Vector3 p)
	{
		DrawDebugPoint(p, Color.white);
	}
	
	public static void DrawDebugLine(Vector3 s, Vector3 e, Color color)
	{
		Debug.DrawLine(s, e, color, 0.0f, false);
	}
	
	public static void DrawDebugLine(Vector3 s, Vector3 e)
	{
		DrawDebugLine(s, e, Color.white);
	}

	public static void DrawDebugCircle(Vector3 p, float r, Color color, int divisions=32)
	{
		float ang = 0;
		float angStep = TwoPi/divisions;
		Vector3 lastPos = Vector3.zero;
		Vector3 firstPos = Vector3.zero;

		for(int i=0; i<divisions; i++)
		{
			Vector3 s = new Vector3(p.x + r*Mathf.Cos(ang), p.y + r*Mathf.Sin(ang), p.z);
			if(i > 0)
				Debug.DrawLine(s, lastPos, color, 0.0f, false);
			else
				firstPos = s;
				
			lastPos = s;
			ang += angStep;
		}
		Debug.DrawLine(firstPos, lastPos, color, 0.0f, false);
	}
	public static void DrawDebugCircle(Vector3 p, float r)
	{
		DrawDebugCircle(p, r, Color.white);
	}
	
	public static void DrawDebugCapsule2D(Vector3 p0, Vector3 p1, float r, Color color, int divisions=32)
	{
		if(p0==p1)
		{
			DrawDebugCircle(p0, r, color, divisions);
			return;
		}
		Vector3 dir = (p1-p0).NormalizedXY();
		float baseAng = Mathf.Atan2(dir.y, dir.x);
	
		float ang = 0;
		divisions /= 2;							// only need half as many divisions per side of the capsule, we do both sides in parallel
		float angStep = Mathf.PI/divisions;		// only doing 180 degrees per side
		Vector3 lastPos0 = Vector3.zero;
		Vector3 lastPos1 = Vector3.zero;
		Vector3 firstPos0 = Vector3.zero;
		Vector3 firstPos1 = Vector3.zero;
		
		for(int i=0; i<divisions; i++)
		{
			float ang0 = baseAng + ang + (Mathf.PI*0.5f);
			float ang1 = baseAng + ang - (Mathf.PI*0.5f);
			Vector3 s0 = new Vector3(p0.x + r*Mathf.Cos(ang0), p0.y + r*Mathf.Sin(ang0), p0.z);
			Vector3 s1 = new Vector3(p1.x + r*Mathf.Cos(ang1), p1.y + r*Mathf.Sin(ang1), p1.z);
			if(i > 0)
			{
				Debug.DrawLine(s0, lastPos0, color, 0.0f, false);
				Debug.DrawLine(s1, lastPos1, color, 0.0f, false);
			}
			else
			{
				firstPos0 = s0;
				firstPos1 = s1;
			}
			
			lastPos0 = s0;
			lastPos1 = s1;
			ang += angStep;
		}
		Debug.DrawLine(firstPos0, lastPos1, color, 0.0f, false);
		Debug.DrawLine(firstPos1, lastPos0, color, 0.0f, false);
	}
	
	public static void DrawDebugCapsule2D(Vector3 p0, Vector3 p1, float r)
	{
		DrawDebugCapsule2D(p0, p1, r, Color.white);
	}
	
	public static void DrawDebugEllipse(Vector3 p, float rx, float ry, Color color, int divisions=32)
	{
		float ang = 0;
		float angStep = TwoPi/divisions;
		Vector3 lastPos = Vector3.zero;
		Vector3 firstPos = Vector3.zero;

		for(int i=0; i<divisions; i++)
		{
			Vector3 s = new Vector3(p.x + rx*Mathf.Cos(ang), p.y + ry*Mathf.Sin(ang), p.z);
			if(i > 0)
				Debug.DrawLine(s, lastPos, color, 0.0f, false);
			else
				firstPos = s;
				
			lastPos = s;
			ang += angStep;
		}
		Debug.DrawLine(firstPos, lastPos, color, 0.0f, false);
	}
	public static void DrawDebugEllipse(Vector3 p, float rx, float ry)
	{
		DrawDebugEllipse(p, rx, ry, Color.white);
	}
	
	public static void DrawDebugBounds2D(Bounds b, Color color)
	{
		Vector3 tl = new Vector3(b.min.x, b.max.y, b.center.z);
		Vector3 tr = new Vector3(b.max.x, b.max.y, b.center.z);
		Vector3 bl = new Vector3(b.min.x, b.min.y, b.center.z);
		Vector3 br = new Vector3(b.max.x, b.min.y, b.center.z);
		Debug.DrawLine(tl, tr, color, 0.0f, false);
		Debug.DrawLine(tr, br, color, 0.0f, false);
		Debug.DrawLine(br, bl, color, 0.0f, false);
		Debug.DrawLine(bl, tl, color, 0.0f, false);
	}
	public static void DrawDebugBounds2D(Bounds b)
	{
		DrawDebugBounds2D(b, Color.white);
	}
	
	public static void DrawDebugBounds2D(FastBounds2D b, Color color)
	{
		Vector3 tl = new Vector3(b.x0, b.y1, 0.0f);
		Vector3 tr = new Vector3(b.x1, b.y1, 0.0f);
		Vector3 bl = new Vector3(b.x0, b.y0, 0.0f);
		Vector3 br = new Vector3(b.x1, b.y0, 0.0f);
		Debug.DrawLine(tl, tr, color, 0.0f, false);
		Debug.DrawLine(tr, br, color, 0.0f, false);
		Debug.DrawLine(br, bl, color, 0.0f, false);
		Debug.DrawLine(bl, tl, color, 0.0f, false);
	}
	
	public static void DrawDebugBounds2D(FastBounds2D b)
	{
		DrawDebugBounds2D(b, Color.white);
	}
	
	public static void DrawDebugTransform(Transform t, float r=1.0f)
	{
		Vector3 s = t.position;
		Debug.DrawLine(s, s + (t.right.normalized * r), Color.red, 0.0f, false);
		Debug.DrawLine(s, s + (t.up.normalized * r), Color.green, 0.0f, false);
		Debug.DrawLine(s, s + (t.forward.normalized * r), Color.blue, 0.0f, false);
	}
	
	public static void DrawDebug3PointSpline(Vector3 p0, Vector3 p1, Vector3 p2, Color color, int divisions = 32)
	{
		float step = 1.0f/divisions;
		Vector3 lastPos = p0;
		float t = step;
		for(int i=0; i<divisions; i++)
		{
			Vector3 pos = GetSimpleSplinePoint(p0, p1, p2, t);
			Debug.DrawLine(lastPos, pos, color, 0.0f, false);
			lastPos = pos;
			t += step;
		}
	}
	public static void DrawDebug3PointSpline(Vector3 p0, Vector3 p1, Vector3 p2)
	{
		DrawDebug3PointSpline(p0, p1, p2, Color.white);
	}
	
	public static void DrawDebugContactPoint(ContactPoint p, float pointRadius = 0.2f, float normalLength = 1.0f)
	{
		if(pointRadius > 0.0f)
			DrawDebugPoint(p.point, Color.white, pointRadius);
		if(normalLength > 0.0f)
			DrawDebugLine(p.point, p.point + (p.normal * normalLength), Color.yellow);
	}
	
	public static void DrawDebugCollisionContacts(Collision coll, float pointRadius = 0.2f)
	{
		ContactPoint[] c = coll.contacts;
		for(int i=0, l=c.Length; i<l; i++)
			DrawDebugContactPoint(c[i], pointRadius);
	}
	
	//----------------------------------------------------------------------------
	// Random functions
	//----------------------------------------------------------------------------

	// RandRange function that returns an int between rmin and rmax inclusive, compatible with the old skool engine convention.
	// Unity's Random.Range is not inclusive of the max value.
	public static int RandRange(int rmin, int rmax)
	{
		return (rmin==rmax) ? rmin : Random.Range(rmin, rmax+1);
	}
	
	// Float version, this just calls the Unity one, just there for consistency so you can call Util.RandRange on either ints or floats
	public static float RandRange(float rmin, float rmax)
	{
		return (rmin==rmax) ? rmin : Random.Range(rmin, rmax);
	}
	
	// return a random float between 0 and 1 inclusive
	// update: is this necessary?  think Random.value does this
	public static float Rand01()
	{
		return Random.Range(0.0f, 1.0f);
	}
	
	public static float RandAngleDegrees()
	{
		float r = Random.Range(0.0f, 360.0f);
		return (r==360.0f) ? 0.0f : r;
	}
	
	public static float RandAngleRadians()
	{
		float r = Random.Range(0.0f, TwoPi);
		return (r==TwoPi) ? 0.0f : r;
	}
	
	public static bool RandBool()
	{
		return Random.value < 0.5f;
	}
	
	// version of Random.InsideUnitCircle that returns a Vector3 with Z at zero (as opposed to a Vector2)
	public static Vector3 RandInsideUnitCircle()
	{
		Vector2 v2 = Random.insideUnitCircle;
		Vector3 v3 = new Vector3(v2.x, v2.y, 0.0f);
		return v3;
	}
	
	public static Vector3 RandOnUnitCircle()
	{
		return RandInsideUnitCircle().normalized;
	}

    //----------------------------------------------------------------------------
    // Memory helper functions
    //----------------------------------------------------------------------------

    public static float MegaBytesToBytes(float megaBytes)
    {
        return megaBytes * (1024f * 1024f);
    }

    public static float BytesToMegaBytes(long bytes)
    {
        return bytes / (1024f * 1024f);
    }

    public static float KiloBytesToBytes(float kiloBytes)
    {
        return kiloBytes * 1024f;
    }

    public static float BytesToKiloBytes(long bytes)
    {
        return bytes / (1024f);
    }

    //----------------------------------------------------------------------------
    // Vector/math functions
    //----------------------------------------------------------------------------

    // Thing to determine if an object's velocity is roughly pointing in the direction of another object.
    // We return normalize(targetPos - objectPos) DOT normalize(objectVelocity).
    // So we return 1 if moving directly to the point, 0 if moving perpendicular to it, -1 if moving directly away from it.
    public static float DotFacingPoint(Vector3 objectPos, Vector3 objectVel, Vector3 targetPos)
	{
		Vector3 dirToTarget = (targetPos-objectPos).normalized;
		Vector3 dirVel = objectVel.normalized;
		return Vector3.Dot(dirToTarget, dirVel);
	}

	// Function to calculate the launch direction for a projectile so it will try to intercept a moving target.
	// Does not currently solve this properly, just gives an improved direction versus aiming straight at the target.
	// This is nicked from Grabatron.  Adapted for 2D XY only, using Vector3.
	public static Vector3 GetInterceptDirectionXY(Vector3 from, float speed, Vector3 destPos, Vector3 destVel)
	{
		//FGAssert.Assert(speed > 0.0f);
		
		// only interested in 2D XY pos
		from.z = 0.0f;
		destPos.z = 0.0f;
		destVel.z = 0.0f;
		
		// figure out time taken if we shoot straight at target point
		float t = 1.0f / (destPos-from).magnitude;
	
		// see where the target would end up if it moved for that much time without changing speed/dir
		Vector3 guessPos = destPos + (destVel * t);
		
		// aim at that point instead
		return (guessPos-from).normalized;
	}
	
	// version that just gets the guessed position (for use with MovementControl.MoveTowardsPoint)
	public static Vector3 GetInterceptPositionXY(Vector3 from, float speed, Vector3 destPos, Vector3 destVel)
	{
		//FGAssert.Assert(speed > 0.0f);
		from.z = 0.0f;
		destPos.z = 0.0f;
		destVel.z = 0.0f;
		float t = 1.0f / (destPos-from).magnitude;
		return destPos + (destVel*t);
	}
	
	// Function to calculate the angular velocity required to blend between two rotations.
	// rotSpeed is in degrees per second, dampingRange is in degrees.
	//
	// Use this instead of Quaternion.RotateTowards in cases where you want to blend rotation but avoid writing directly to the
	// transform.rotation.  On things that use rigidbody and colliders (i.e. physics objects), writing to transform.rotation is
	// slow and also error-prone (you could rotate the object into solid collision), it is faster and less glitchy to use
	// this function and manipulate the angular velocity instead.
	public static Vector3 GetAngularVelocityForRotationBlend(Quaternion from, Quaternion to, float rotSpeed=500.0f, float dampingRange=20.0f)
	{
		Quaternion qDelta = to * Quaternion.Inverse(from);
		Vector3 deltaAxis;
		float deltaAng;
		qDelta.ToAngleAxis(out deltaAng, out deltaAxis);			// this gives us the axis to rotate around, and the current angle difference in DEGREES
		float signedDeltaAng = FixAnglePlusMinusDegrees(deltaAng);
		deltaAng = Mathf.Abs(signedDeltaAng);
		
		if ((dampingRange > 0.0f) && (deltaAng < dampingRange))
			rotSpeed *= (deltaAng/dampingRange);
			
		if(deltaAng == 0.0f)
			return Vector3.zero;
		rotSpeed *= Mathf.Deg2Rad;									// angular velocity is in RADIANS per second
		return deltaAxis * ((signedDeltaAng < 0.0f) ? -rotSpeed : rotSpeed);
	}
	
	// Thing to get a curve between 3 points.  todo: improve to work with arbitrary array of points, or whatever.
	public static Vector3 GetSimpleSplinePoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
	{
		Vector3 pc = p1*2.0f - (p0+p2)*0.5f;	// get control point to use instead of p1, so curve will pass through p1 instead of just getting dragged part way towards it
		Vector3 lp0 = Vector3.Lerp(p0, pc, t);	// get lerped point between start and control point
		Vector3 lp1 = Vector3.Lerp(pc, p2, t);	// get lerped point between control point and end
		return Vector3.Lerp(lp0, lp1, t);		// get final lerped point between those two
	}
	
	// Functions to rotate a 2D vector around the Z axis (or 3D vector ignoring Z)
	public static Vector2 RotateRadians(this Vector2 v, float ang)
	{
		float sin = Mathf.Sin(ang);
		float cos = Mathf.Cos(ang);
		float x = v.x;
		float y = v.y;
		return new Vector2(x*cos - y*sin, x*sin + y*cos);
	}
	
	public static Vector2 RotateDegrees(this Vector2 v, float ang)
	{
		ang *= Mathf.Deg2Rad;
		float sin = Mathf.Sin(ang);
		float cos = Mathf.Cos(ang);
		float x = v.x;
		float y = v.y;
		return new Vector2(x*cos - y*sin, x*sin + y*cos);
	}
	
	public static Vector3 RotateXYRadians(this Vector3 v, float ang)
	{
		float sin = Mathf.Sin(ang);
		float cos = Mathf.Cos(ang);
		float x = v.x;
		float y = v.y;
		return new Vector3(x*cos - y*sin, x*sin + y*cos);
	}
	
	public static Vector3 RotateXYDegrees(this Vector3 v, float ang)
	{
		ang *= Mathf.Deg2Rad;
		float sin = Mathf.Sin(ang);
		float cos = Mathf.Cos(ang);
		float x = v.x;
		float y = v.y;
		return new Vector3(x*cos - y*sin, x*sin + y*cos);
	}
	
	
	//----------------------------------------------------------------------------
	// Unity GameObject/Component helper functions
	//----------------------------------------------------------------------------
	// [AOC] Moved to GameObjectExt


	/*[PAC]
	// extension method Transform.GetRootPrefab()
	// Works like Transform.root but returns the first Transform that is on an object with a PrefabInstance component.
	// Can use this on things like a collider on a child object of a human that is parented to a boat, and we want to
	// find the human, not the boat.
	public static Transform GetRootPrefab(this Transform t)
	{
		while(true)
		{
			if(t.GetComponent<PrefabInstance>() != null)
				return t;
			Transform parent = t.parent;
			if(parent == null)
				return t;
			t = parent;
		}
	}
	*/
	
	// get a list of renderers that includes mesh and skinned mesh renderers but no other types (like particle renderers).
	// We use this to get models to be faded out, or have swallow shader applied etc., without messing up on attached particle emitters
	public static Renderer[] GetModelRenderers(this GameObject obj)
	{
		MeshRenderer[] mr = obj.GetComponentsInChildren<MeshRenderer>();
		SkinnedMeshRenderer[] smr = obj.GetComponentsInChildren<SkinnedMeshRenderer>();
		
		if((mr != null) && (mr.Length == 0))
			mr = null;
		if((smr != null) && (smr.Length == 0))
			smr = null;
		
		// if we have only mesh or skinnedmesh renderers, return a single array
		if(mr == null)
			return smr;
		else if(smr==null)
			return mr;
		
		// if we have both types, build an array containing both lists.  Maybe there is a built in way to concatenate arrays?
		int len = mr.Length + smr.Length;
		Renderer[] r = new Renderer[len];
		int idx = 0;
		if(mr != null)
		{
			foreach(Renderer t in mr)
				r[idx++] = t;
		}
		if(smr != null)
		{
			foreach(Renderer t in smr)
				r[idx++] = t;
		}
		return r;
	}
	
	#if UNITY_STANDALONE && FGOL_DESKTOP
	public static void SetModelCastShadows(this GameObject obj, Renderer[] rs=null)
	{
		if(rs==null)
			rs = obj.GetModelRenderers();
		foreach(Renderer r in rs)
		{
			r.castShadows = true;
			r.receiveShadows = false;
		}
	}
	
	public static void SetModelReceiveShadows(this GameObject obj, Renderer[] rs=null)
	{
		if(rs==null)
			rs = obj.GetModelRenderers();
		foreach(Renderer r in rs)
		{
			r.castShadows = false;
			r.receiveShadows = true;
		}
	}
	
	public static void SetModelIgnoreShadows(this GameObject obj, Renderer[] rs=null)
	{
		if(rs==null)
			rs = obj.GetModelRenderers();
		foreach(Renderer r in rs)
		{
			r.castShadows = false;
			r.receiveShadows = false;
		}
	}
	#else
	public static void SetModelCastShadows(this GameObject obj, Renderer[] rs=null) 	{}
	public static void SetModelReceiveShadows(this GameObject obj, Renderer[] rs=null)	{}
	public static void SetModelIgnoreShadows(this GameObject obj, Renderer[] rs=null)	{}
	#endif
	
	// extension methods for Transform, for setting individual components of position etc
	public static float SetPosX(this Transform t, float x)
	{
		Vector3 v = t.position;
		v.x = x;
		t.position = v;
		return x;	// return the value we set, for convenience
	}
	public static float SetPosY(this Transform t, float y)
	{
		Vector3 v = t.position;
		v.y = y;
		t.position = v;
		return y;
	}
	public static float SetPosZ(this Transform t, float z)
	{
		Vector3 v = t.position;
		v.z = z;
		t.position = v;
		return z;
	}
	public static float SetLocalPosX(this Transform t, float x)
	{
		Vector3 v = t.localPosition;
		v.x = x;
		t.localPosition = v;
		return x;	// return the value we set, for convenience
	}
	public static float SetLocalPosY(this Transform t, float y)
	{
		Vector3 v = t.localPosition;
		v.y = y;
		t.localPosition = v;
		return y;
	}
	public static float SetLocalPosZ(this Transform t, float z)
	{
		Vector3 v = t.localPosition;
		v.z = z;
		t.localPosition = v;
		return z;
	}
	public static float SetLocalScale(this Transform t, float s)
	{
		t.localScale = new Vector3(s, s, s);
		return s;
	}
	public static float GetGlobalScaleQuick(this Transform t)	// gets the global scale of a transform by multiplying the X component of this and all parents.
	{															// So only works on things with uniform scale (x/y/z the same) all the way up the hierarchy
		float s = 1.0f;
		while(t != null)
		{
			s *= t.localScale.x;
			t = t.parent;
		}
		return s;
	}
	
	public static void CopyFrom(this Transform t, Transform from, bool includeScale=true)
	{
		t.position = from.position;
		t.rotation = from.rotation;
		if(includeScale)
			t.localScale = from.localScale;
	}
	
	// extension methods for getting a single roll angle from direction vectors (Vector2 and XY components of Vector3)
	public static float ToAngleDegrees(this Vector2 v)
	{
		return Mathf.Atan2(v.y, v.x)*Mathf.Rad2Deg;
	}

	public static float ToAngleDegrees(this Vector3 v)
	{
		return Mathf.Atan2(v.y, v.x)*Mathf.Rad2Deg;
	}

	public static float ToAngleRadians(this Vector2 v)
	{
		return Mathf.Atan2(v.y, v.x);
	}
	public static float ToAngleDegreesXY(this Vector3 v)
	{
		return Mathf.Atan2(v.y, v.x)*Mathf.Rad2Deg;
	}
	public static float ToAngleRadiansXY(this Vector3 v)
	{
		return Mathf.Atan2(v.y, v.x);
	}
	
	// Returns a normalized vector that has had its z cleared, so result is guaranteed to be in the XY plane.
	// Does not handle zero length, so if x & y components are zero, it will return a zero vector.
	public static Vector3 NormalizedXY(this Vector3 v)
	{
		Vector3 n = v;
		n.z = 0.0f;
		return n.normalized;
	}
	// This version checks for zero length, and returns an UP vector in that case.
	public static Vector3 NormalizedXYSafe(this Vector3 v)
	{
		float x = v.x;
		float y = v.y;
		float m2 = x*x + y*y;
		if(m2 < Mathf.Epsilon)
			return Vector3.up;
		float d = 1.0f/Mathf.Sqrt(m2);
		return new Vector3(x*d, y*d, 0.0f);
	}
	
	
	// int to hex string - a simple extension method of int, rather than typing the gibberish string.Format stuff.
	public static string ToHexString(this int n)
	{
		return string.Format("{0:X}", n);
	}

	// Methods for float, for adding or subtracting a value, then clamping to some limit, and returning a bool to say whether it was clamped.
	// Doesn't appear to be possible to do with extension methods.
	// Use for very common situations of updating timers etc., e.g. for some thing that counts down to zero and then does something:
	// if(Util.SubClamp(ref timer, dt))
	//		...
	public static bool SubClamp(ref float f, float subVal, float clampVal=0.0f)
	{
		f -= subVal;
		if(f <= clampVal)
		{
			f = clampVal;
			return true;
		}
		return false;
	}
	
	public static bool AddClamp(ref float f, float addVal, float clampVal=1.0f)
	{
		f += addVal;
		if(f >= clampVal)
		{
			f = clampVal;
			return true;
		}
		return false;
	}

	// convert stats for gameplay accessories from percentage increase, to value to scale by (e.g. for something to increase by +5%, convert stat from 5 to 1.05)
	public static float ScaleFromPerc(this float f)
	{
		return 1.0f + (f*0.01f);
	}
	public static float ScaleFromPercClamped(this float f)
	{
		return 1.0f + Mathf.Clamp(f*0.01f, -1.0f, 1.0f);
	}
	
	
	//----------------------------------------------------------------------------
	// date/time related functions
	//----------------------------------------------------------------------------
	
	// nicked this hilarious function off the internet
	public static System.DateTime GetEasterSunday(int year)
	{
		int day = 0;
		int month = 0;
		int g = year%19;
		int c = year/100;
		int h = (c-(int)(c/4) - (int)((8*c +13) / 25) + 19*g +15) %30;
		int i = h - (int)(h/28) * (1 - (int)(h/28) * (int)(29 / (h+1)) * (int)((21 - g) / 11));
		day = i - ((year + (int)(year / 4) + i + 2 - c + (int)(c/4)) % 7) + 28;
		month = 3;
		if(day > 31)
		{
			month++;
			day -= 31;
		}
		return new System.DateTime(year, month, day);
	}
	
	public static bool IsTodayEasterSunday()
	{
		System.DateTime dt = System.DateTime.Now;
		return (dt == GetEasterSunday(dt.Year));
	}
	
	public static bool IsTodayChristmasDay()
	{
		System.DateTime dt = System.DateTime.Now;
		return (dt.Month == 12) && (dt.Day == 25);
	}
	
	public static bool IsTodayStPatricksDay()
	{
		// March 17th.  But do we want to change this for the whole week?
		System.DateTime dt = System.DateTime.Now;
		return (dt.Month == 3) && (dt.Day == 17);
	}
	
	//----------------------------------------------------------------------------
	// Helpers for serialization / parsing
	//----------------------------------------------------------------------------

	// If the named field exists, overwrite the output value, otherwise leave it unchanged
	public static void OverrideInt(Dictionary<string, string> dic, string stat, ref int i)
	{
		if(dic.ContainsKey(stat))
			int.TryParse(dic[stat], out i);
	}
	
	public static void OverrideFloat(Dictionary<string, string> dic, string stat, ref float f)
	{
		if(dic.ContainsKey(stat))
			float.TryParse(dic[stat], out f);
	}

	public static void OverrideBool(Dictionary<string, string> dic, string stat, ref bool b)
	{
		if(dic.ContainsKey(stat))
			bool.TryParse(dic[stat], out b);
	}
	
	// extension method of string to remove "(Clone)" from the end if it is present, otherwise returns an unchanged string.
	public static string RemoveCloneSuffix(this string s)
	{
		if(s.EndsWith("(Clone)"))
			return s.Substring(0, s.Length-7);
		return s;
	}

	public static void SetLayerRecursively(GameObject obj, string newLayer)
	{
		Util.SetLayerRecursively(obj, LayerMask.NameToLayer(newLayer));
	}
	
	public static void SetLayerRecursively(GameObject obj, int newLayer)
	{
		if (null == obj)
		{
			return;
		}        
		
		obj.layer = newLayer;       
		
		foreach (Transform child in obj.transform)
		{
			if (null == child)
			{
				continue;
			}
			
			SetLayerRecursively(child.gameObject, newLayer);
		}
	}	

	public static Vector3? RayPlaneIntersect(Ray r, Plane p)
    {
        float dot = Vector3.Dot(r.direction, p.normal);

        // check that the dot product is ok
        if(float.IsInfinity(dot) || float.IsNaN(dot))
        {
            return null;
        }

        // intersection of plane Ax+By+Cz+D=0 with ray P + t*v, where P = (x0,y0,z0) and v = (Vx,Vy,Vz)
        // Solve for t,
        // t = -(Ax0 + By0+Cz0+D)/(AVx + BVy + CVz) 
        // or in vector form 
        // t = -(P.N + D)/(V.N)
        float dot1 = Vector3.Dot(r.origin, p.normal);

        float t = -(dot1 + p.distance) / dot;

        if(t >= 0)
        {
            return r.GetPoint(t);
        }

        return null;
    }   

    public static YieldInstruction StartCoroutineWithoutMonobehaviour(string name, IEnumerator coroutine)
    {
        GameObject go = new GameObject(name);
        HelperMonoBehaviour mono = go.AddComponent<HelperMonoBehaviour>();
        return mono.StartCoroutine(HelperCoroutine(coroutine, mono));
    }

    private static IEnumerator HelperCoroutine(IEnumerator coroutine, HelperMonoBehaviour helperObject)
    {
        yield return helperObject.StartCoroutine(coroutine);
        MonoBehaviour.Destroy(helperObject.gameObject);
    }

    // Empty helper class. We just need it to extend from MonoBehaviour because Unity doesn't allow to add a plain MonoBehaviour component to a game object
    private class HelperMonoBehaviour : MonoBehaviour
    {
    }
}
