using UnityEngine;
using System.Collections;
using System;
using FGOL;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Assets.Code.Game.Spline
{
    public class BezierSpline : MonoBehaviour
    {
        [SerializeField]
        private Vector3[] points;

        [SerializeField]
        private BezierControlPointMode[] modes;

        [SerializeField]
        private float[] length;

        [SerializeField]
        public bool optimize = true;

        [SerializeField]
        public float optimizeDotCheck = 0.7f;

        [SerializeField]
        public int stepsPerCurve = 500;

        [Serializable]
        public class DistancePosMapEntry
        {
            public Vector3 pos;
            public float distance;

            public DistancePosMapEntry(Vector3 _pos, float _dist)
            {
                pos = _pos;
                distance = _dist;
            }
        }

        [SerializeField]
        public List<DistancePosMapEntry> distancePosLookup = new List<DistancePosMapEntry>();
        
        // temporary array so we don't have to allocate a new one everytime
        private Vector3[] curvePoints = new Vector3[4];

        // handy accessors
        public int pointCount
        {
            get { return points.Length; }
        }
        public int curveCount
        {
            get
            {
                return (points.Length - 1) / 3;
            }
        }
		public int totalSteps
		{
			get { return stepsPerCurve * curveCount; }
		}

        public void SetControlPoint(int index, Vector3 point)
        {
            // check if we're moving a central control point, in which case move the two surrounding ones next to it also
            if(index % 3 == 0)
            {
                Vector3 delta = point - points[index];  // amount of movement from current pos
                if(index > 0)
                {
                    points[index - 1] += delta;
                }
                if(index + 1 < points.Length)   // if we're not the last point, move the one to the right as well
                {
                    points[index + 1] += delta;
                }
            }

            points[index] = point;
            EnforceMode(index);

            Refresh();
        }
        public Vector3 GetControlPoint(int index)
        {
            return points[index];
        }
        // MODESELEKTOR!
        public void SetControlPointMode(int index, BezierControlPointMode mode)
        {
            // convert control point index to mode index
            modes[(index + 1)/3] = mode;

            EnforceMode(index);

            Refresh();
        }
        public BezierControlPointMode GetControlPointMode(int index)
        {
            return modes[(index + 1) / 3];
        }

        private void EnforceMode(int index)
        {
            int modeIndex = (index + 1) / 3;
            BezierControlPointMode mode = modes[modeIndex];

            if((mode == BezierControlPointMode.free) || (modeIndex == 0) || (modeIndex == modes.Length - 1))
            {
                return;
            }

            // find the point indices on either side of the central control point (that we've set the mode for)
            int middleIndex = modeIndex * 3;    // central point index

            int fixedIndex; // currently selected point
            int enforcedIndex; // point on the other side
            if(index <= middleIndex)
            {
                fixedIndex = middleIndex - 1;
                enforcedIndex = middleIndex + 1;
            }
            else
            {
                fixedIndex = middleIndex + 1;
                enforcedIndex = middleIndex - 1;
            }

            // for both aligned and mirrored, we need the vector from previous point to middle point,
            Vector3 vecToMiddle = points[middleIndex] - points[fixedIndex];

            // for aligned mode, make this vector length equal to the distance from the middle to the other point
            if(mode == BezierControlPointMode.aligned)
            {
                vecToMiddle = vecToMiddle.normalized * Vector3.Distance(points[middleIndex], points[enforcedIndex]);
            }

            // make the point on the other side mirror this situation
            points[enforcedIndex] = points[middleIndex] + vecToMiddle;
        }

        private void Refresh()
        {
            CalculateDistancePosEntries();
        }

        // Initialization in the Editor
        public void Reset()
        {
            points = new Vector3[]{
                new Vector3(1, 0, 0), 
                new Vector3(2, 0, 0), 
                new Vector3(3, 0, 0),
                new Vector3(4, 0, 0)};

            curvePoints = new Vector3[] { 
                Vector3.zero, 
                Vector3.zero, 
                Vector3.zero, 
                Vector3.zero };

            modes = new BezierControlPointMode[]{
                BezierControlPointMode.free,
                BezierControlPointMode.free
            };

            length = new float[] { 0.0f };
        }

        public Vector3 GetPoint(float t)
        {
            // spread the 't' over all the curves joined together, and supply the appropriate array of points
            int baseIndex;
            if(t >= 1f) // edge case
            {
                t = 1f;
                baseIndex = points.Length - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * curveCount;
                baseIndex = (int)t;
                t -= baseIndex;
                baseIndex *= 3;
            }

            // supply the proper array
            curvePoints[0] = points[baseIndex + 0];
            curvePoints[1] = points[baseIndex + 1];
            curvePoints[2] = points[baseIndex + 2];
            curvePoints[3] = points[baseIndex + 3];

            return transform.TransformPoint(MathUtils.GetBezierPoint(curvePoints, 3, t));
        }

        public Vector3 GetTangent(float t)
        {
            // spread the 't' over all the curves joined together, and supply the appropriate array of points
            int baseIndex;
            if(t >= 1f) // edge case
            {
                t = 1f;
                baseIndex = points.Length - 4;
            }
            else
            {
                t = Mathf.Clamp01(t) * curveCount;
                baseIndex = (int)t;
                t -= baseIndex;
                baseIndex *= 3;
            }

            // supply the proper array
            curvePoints[0] = points[baseIndex + 0];
            curvePoints[1] = points[baseIndex + 1];
            curvePoints[2] = points[baseIndex + 2];
            curvePoints[3] = points[baseIndex + 3];

            return (transform.TransformPoint(MathUtils.GetBezierTangent(curvePoints, 3, t)) - transform.position).normalized;
        }

        public float GetLength()
        {
            if(distancePosLookup.Count > 0)
            {
                return distancePosLookup[distancePosLookup.Count - 1].distance;
            }
            return 0.0f;
        }

        public void AddCurve()
        {
            Vector3 point = points[points.Length - 1];
            Array.Resize(ref points, points.Length + 3);
            point.x += 2f;
            points[points.Length - 3] = point;
            point.x += 2f;
            points[points.Length - 2] = point;
            point.x += 2f;
            points[points.Length - 1] = point;

            // when adding another curve, we get one more control point, so inflate the modes array
            Array.Resize(ref modes, modes.Length + 1);
            // assign the new mode the same as the last one
            modes[modes.Length - 1] = modes[modes.Length - 2];
            
            // when adding a new curve, ensure mode is enforced
            EnforceMode(points.Length - 4);

            Refresh();
        }

        public void RemoveLastCurve()
        {
            Array.Resize(ref points, points.Length - 3);

            // when adding another curve, we get one more control point, so inflate the modes array
            Array.Resize(ref modes, modes.Length - 1);

            Array.Resize(ref length, length.Length - 1);

            Refresh();
        }

        public void RemoveFirstCurve()
        {
            // shift all elements forward and remove the last curve
            for(int i = 0; i < points.Length - 3; i++)
            {
                points[i] = points[i + 3];
            }
            // same for modes
            for(int j = 0; j < modes.Length - 1; j++)
            {
                modes[j] = modes[j + 1];
            }
            // same for lengths
            for(int i = 0; i < length.Length - 1; i++)
            {
                length[i] = length[i + 1];
            }
            RemoveLastCurve();
            Refresh();
        }

        public void DeleteControlPoint(int index)
        {
            if(Assert.Expect(index % 3 == 0, "Cannot delete tangent point!"))
            {
                if(index == 0)
                {
                    RemoveFirstCurve();
                    return;
                }
                if(index == (points.Length - 1))
                {
                    RemoveLastCurve();
                    return;
                }
                for(int i = index - 1; i < points.Length - 3; i++)
                {
                    points[i] = points[i + 3];
                }
                Array.Resize(ref points, points.Length - 3);

                int modeIndex = (index + 1) / 3;
                for(int i = modeIndex; i < modes.Length - 1; i++)
                {
                    modes[i] = modes[i + 1];
                }
                Array.Resize(ref modes, modes.Length - 1);
                for(int i = modeIndex; i < length.Length - 1; i++)
                {
                    length[i] = length[i + 1];
                }
                Array.Resize(ref length, length.Length - 1);
                //EnforceMode(index);
                Refresh();
            }
        }

        // traverses the spline and finds the closest point on the spline to the given point, also returns the corresponding T and step value
        public Vector3 GetClosestPointToPoint(Vector3 pt, int numSteps, out float closestT, out int closestStep)
        {
            float closestDist = float.MaxValue;

            closestT = 0f;
            Vector3 result = GetPoint(0f);
            closestStep = 0;

            for(int i = 0; i <= numSteps; i++)
            {
                float t = (float)i / (float)numSteps;

                Vector3 pointOnSpline = GetPoint(t);

                float dist = (pointOnSpline - pt).sqrMagnitude;

                if(dist < closestDist)
                {
                    closestDist = dist;
                    result = pointOnSpline;

                    closestT = t;

                    closestStep = i;
                }
            }
            return result;
        }

        public void CalculateDistancePosEntries()
        {
            Vector3 lineStart = GetPoint(0f);

            // optimization variables
            Vector3 refLineStart = lineStart;

			float secondPtT = 1f / (float)totalSteps;

			Vector3 refLineEnd = GetPoint(secondPtT);
			Vector3 prevDiff = (refLineEnd - refLineStart).normalized;
			
			Vector3 lineEnd = refLineEnd;

            distancePosLookup.Clear();
            distancePosLookup.Add(new BezierSpline.DistancePosMapEntry(lineStart, 0.0f));

            // distance calculation
            float distance = (refLineEnd - refLineStart).magnitude;
            float t = 0.0f;

            // previous things
            Vector3 prevPoint = refLineEnd;
			float prevT = secondPtT;
            float prevDistance = distance;

            int controlPointsPassed = 1;

            float tPerControlPoint = 1.0f / (float)curveCount;

			for(int i = 2; i <= totalSteps; i++)
			{
                prevT = t;
				t = (float)i / (float)totalSteps;
				
				prevPoint = lineEnd;
                lineEnd = GetPoint(t);

                prevDistance = distance;
                // current distance covered
                distance += (lineEnd - prevPoint).magnitude;
                
				// find out if we're passing a main control point, in which case force add it
				float checkT = tPerControlPoint * controlPointsPassed;

                if(optimize)
                {
                    // check the angle difference between the previous angle difference
                    prevDiff = (refLineEnd - refLineStart).normalized;
                    Vector3 diff = (lineEnd - refLineEnd).normalized;

                    float dot = Vector3.Dot(diff, prevDiff);

                    if((dot < optimizeDotCheck))
                    {
                        // differentenough 
                        refLineStart = refLineEnd;

                        // add this entry!
                        //Debug.Log("ADDING " + prevPoint.ToString() + "," + prevDistance + " @ i-1  = " + (i - 1));
                        distancePosLookup.Add(new BezierSpline.DistancePosMapEntry(prevPoint, prevDistance));
                    }

                    refLineEnd = lineEnd;
                }
                else
                {
                    distancePosLookup.Add(new BezierSpline.DistancePosMapEntry(lineEnd, distance));
                }
				
                // When we cross a main control point, add it 
                // NOTE(MV): There's currently a bug where the main control point is added twice! FIX!
                if((prevT < checkT) && (checkT <= t) && (t < 1.0f))
                {
                    // add this entry!
                    Vector3 controlPointPos = GetPoint(checkT);

                    float distanceToCP = (prevDistance + (controlPointPos - prevPoint).magnitude);

                    //Debug.Log("ADDING CT " + controlPointPos.ToString() + "," + distanceToCP + " @ i-1 = " + (i - 1));
                    distancePosLookup.Add(new BezierSpline.DistancePosMapEntry(controlPointPos, distanceToCP));

                    controlPointsPassed++;

                    refLineStart = controlPointPos;
                }
			}
			distancePosLookup.Add(new BezierSpline.DistancePosMapEntry(lineEnd, distance));
		}
		
		public Vector3 GetPosForDistance(float distance)
        {
            for(int i = 0; i < distancePosLookup.Count - 1; i++)
            {
                DistancePosMapEntry prevEntry = distancePosLookup[i];
                DistancePosMapEntry nextEntry = distancePosLookup[i + 1];

                if((distance >= prevEntry.distance) && (distance < nextEntry.distance))
                {
                    Vector3 prevPos = prevEntry.pos;
                    Vector3 nextPos = nextEntry.pos;

                    float ratio = (distance - prevEntry.distance) / (nextEntry.distance - prevEntry.distance);

                    return Vector3.Lerp(prevPos, nextPos, ratio);
                }
            }
            return distancePosLookup[0].pos;
        }

        public Vector3 GetClosestPointToRay(Ray ray, out int lastPassedMainControlPointIndex, out float closestT)
        {
            float closestDist = float.MaxValue;

            int controlPtIndex = 0;

            // defaults
            lastPassedMainControlPointIndex = 0;
            closestT = 0.0f;

            Vector3 result = GetPoint(0f);
			for(int i = 0; i <= totalSteps; i++)
			{
				float t = (float)i / (float)totalSteps;
				
				// keep track of main control points that we pass
                controlPtIndex = (int)(Mathf.Clamp01(t) * curveCount);

                Vector3 pointOnSpline = GetPoint(t);

                float dist = MathUtils.DistanceToLine(ray, pointOnSpline);

                if(dist < closestDist)
                {
                    closestDist = dist;
                    result = pointOnSpline;

                    // stash this
                    lastPassedMainControlPointIndex = controlPtIndex;

                    closestT = t;
                }
            }
            return result;
        }

        public void AddPointClosestToRay(Ray ray)
        {
            int lesserIndex = 0;
            int greaterIndex = 1;
            float closestT = 0.0f;

            // get the actual best position to insert it in
            Vector3 actualClosestPt = GetClosestPointToRay(ray, out lesserIndex, out closestT);

            lesserIndex *= 3;
            greaterIndex = lesserIndex + 3;

            Vector3 direction = (points[greaterIndex] - points[lesserIndex]).normalized;
            float scalePointsApart = 2.0f;

            int tangentPoint1IndexOffset = 0;
            int mainPointIndexOffset = 1;
            int tangentPoint2IndexOffset = 2;

            // TODO: Improve this!
            Vector3 actualClosestPtTangent1 = actualClosestPt - transform.TransformVector(direction * scalePointsApart);
            Vector3 actualClosestPtTangent2 = actualClosestPt + transform.TransformVector(direction * scalePointsApart);

            Array.Resize(ref points, points.Length + 3);

            // move on up by 3 places
            for(int i = points.Length - 1; i >= (greaterIndex + 2); i--)
            {
                points[i] = points[i - 3];
            }
            points[greaterIndex + tangentPoint1IndexOffset - 1] = transform.InverseTransformPoint(actualClosestPtTangent1);
            points[greaterIndex + mainPointIndexOffset - 1] = transform.InverseTransformPoint(actualClosestPt);
            points[greaterIndex + tangentPoint2IndexOffset - 1] = transform.InverseTransformPoint(actualClosestPtTangent2);

            Array.Resize(ref modes, modes.Length + 1);
            int greaterIndexMode = (greaterIndex + mainPointIndexOffset) / 3;
            for(int i = modes.Length - 1; i >= greaterIndexMode; i--)
            {
                modes[i] = modes[i - 1];
            }
            modes[greaterIndexMode] = BezierControlPointMode.free;

            EnforceMode(greaterIndex + mainPointIndexOffset - 1);

            Refresh();
        }

        public void DrawOptimizedPoints()
        {
#if UNITY_EDITOR
            if(distancePosLookup.Count == 0)
                return;

            for(int i = 0; i < distancePosLookup.Count; i++)
            {
                DistancePosMapEntry entry = distancePosLookup[i];
                Handles.color = Color.white;
                Handles.DrawSolidDisc(entry.pos, Vector3.forward, HandleUtility.GetHandleSize(entry.pos) * 0.1f);

                if(i > 0)
                {
                    DistancePosMapEntry prevEntry = distancePosLookup[i - 1];
                    Handles.DrawLine(entry.pos, prevEntry.pos);
                }
            }
#endif
        }
    }
}
