using Assets.Code.Game.Spline;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BezierSpline))]
public class BezierSplineEditor : Editor
{
    BezierSpline spline;
    Transform thisTrans;
    Quaternion thisRot;

    Vector3[] transformedPoints = null;

    private const float directionScale = 1.0f;
    int pointsPerCurve = 4;

    private const float handleSize = 0.05f;
    private const float pickSize = 0.1f;

    private int selectedIndex = -1;

    private bool isEditMode = false;
	private bool selectAllPoints = false;

    private void OnSceneGUI()
    {
        spline = target as BezierSpline;

        // transform the points supplied with the curve's transform
        thisTrans = spline.transform;

        // set handles colour
        Handles.color = Color.gray;

        // first work out if we're in 'Local' mode or 'Global' mode, which makes us draw the position handles differently
        thisRot = (Tools.pivotRotation == PivotRotation.Local ? thisTrans.rotation : Quaternion.identity);

        if(transformedPoints == null)
        {
            transformedPoints = new Vector3[pointsPerCurve];
        }

		transformedPoints[0] = ShowPoint(0);

        for(int k = 1; k < spline.pointCount; k += 3)
        {
            for(int i = 1; i < pointsPerCurve; i++)
            {
                // get the transformed/moved handle position
                transformedPoints[i] = ShowPoint(i + k - 1);

                // draw the lines between them (except for between the midway points)
                if((i > 0) && (i != 2))
                {
                    Handles.DrawLine(transformedPoints[i], transformedPoints[i - 1]);
                }
            }

            Handles.DrawBezier(transformedPoints[0], transformedPoints[3], transformedPoints[1], transformedPoints[2], Color.white, null, 2f);

			transformedPoints[0] = transformedPoints[3];
        }
    
        Vector3 normal = Vector3.forward;

        //if(spline.optimize)
        //spline.DrawOptimizedPoints();


		for(int i = 1; i <= spline.totalSteps; i++)
        {
			float t = (float)i / (float)spline.totalSteps;
			
			Vector3 lineEnd = spline.GetPoint(t);

            Handles.color = Color.magenta;
            Handles.DrawSolidDisc(lineEnd, normal, HandleUtility.GetHandleSize(lineEnd) * 0.03f);

            Handles.color = Color.green;
            Handles.DrawLine(lineEnd, lineEnd + spline.GetTangent(t) * directionScale);
        }

        if(isEditMode)
        {
            // keep the spline selected 
            Selection.activeObject = target;

            if(Event.current.type == EventType.MouseDown)
            {
                // add new point
                Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

                Undo.RecordObject(spline, "Add middle point");
                spline.AddPointClosestToRay(worldRay);
                EditorUtility.SetDirty(spline);
				SceneView.RepaintAll();
				Repaint();
            }

            Event.current.Use();
        }
    }

    public override void OnInspectorGUI()
    {
        //Debug.Log("INSPECTOR GUI");
        bool showBar = false;
        spline = target as BezierSpline;
        if((selectedIndex > 0) && (selectedIndex < spline.pointCount))
        {
            GUILayout.Label("Selected point");

            // Check for position changes
            EditorGUI.BeginChangeCheck();

            Vector3 newPos = EditorGUILayout.Vector3Field("Pos:", spline.GetControlPoint(selectedIndex));

            if(EditorGUI.EndChangeCheck())
            {
                // tell undo
                Undo.RecordObject(spline, "Move SplinePoint");
                spline.SetControlPoint(selectedIndex, newPos);
                EditorUtility.SetDirty(spline);
            }

            // now check for Mode changes
            EditorGUI.BeginChangeCheck();
            BezierControlPointMode mode = (BezierControlPointMode)EditorGUILayout.EnumPopup("Constraint:", spline.GetControlPointMode(selectedIndex));
            if(EditorGUI.EndChangeCheck())
            {
                // tell undo
                Undo.RecordObject(spline, "Change constraint");
                spline.SetControlPointMode(selectedIndex, mode);
                EditorUtility.SetDirty(spline);
            }

			showBar = true;

        }

        // main points only
        if((selectedIndex < spline.pointCount) && (selectedIndex % 3 == 0))
        {
            if(GUILayout.Button("Delete point"))
            {
                Undo.RecordObject(spline, "Delete point");
                spline.DeleteControlPoint(selectedIndex);
                EditorUtility.SetDirty(spline);
                selectedIndex = -1;
            }

            if ( GUILayout.Button("Add New Point") )
            {
				Undo.RecordObject(spline, "Add Point");
				spline.AddControlPointAfter( selectedIndex);
				EditorUtility.SetDirty(spline);
            }
            showBar = true;
        }

        if ( showBar )
			EditorGUILayout.TextArea("",GUI.skin.horizontalSlider);

        if(GUILayout.Button("Add Point At End"))
        {
            Undo.RecordObject(spline, "Add Point");
            spline.AddCurve();
            EditorUtility.SetDirty(spline);
        }
        if(GUILayout.Button("Remove Last Point"))
        {
            Undo.RecordObject(spline, "Remove Last Point");
            spline.RemoveLastCurve();
            EditorUtility.SetDirty(spline);
        }
        if(GUILayout.Button("Remove First Point"))
        {
            Undo.RecordObject(spline, "Remove First Point");
            spline.RemoveFirstCurve();
            EditorUtility.SetDirty(spline);
        }

		bool t = (GUILayout.Toggle(selectAllPoints, "Select All Points"));
		if ( t != selectAllPoints )
		{
			selectAllPoints = t;
			EditorUtility.SetDirty(spline);
		}

        isEditMode = (GUILayout.Toggle(isEditMode, "Edit mode"));

		EditorGUI.BeginChangeCheck();
        bool optimise = GUILayout.Toggle(spline.optimize, "Optimize");
		if(EditorGUI.EndChangeCheck())
		{
			spline.optimize = optimise;
			spline.CalculateDistancePosEntries();
		}

		GUILayout.Label("Optimize Angle Check");
		
		// Check for position changes
		EditorGUI.BeginChangeCheck();
		
		float dotCheck = EditorGUILayout.FloatField("Dot: ", spline.optimizeDotCheck);
		if(EditorGUI.EndChangeCheck())
		{
			// tell undo
			Undo.RecordObject(spline, "Change Angle Check");
            spline.optimizeDotCheck = dotCheck;
            spline.CalculateDistancePosEntries();
			EditorUtility.SetDirty(spline);
		}

        EditorGUI.BeginChangeCheck();
        int steps = EditorGUILayout.IntField("Steps: ", spline.stepsPerCurve);
        if(EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(spline, "Changed num steps");
            spline.stepsPerCurve = steps;
            spline.CalculateDistancePosEntries();
            EditorUtility.SetDirty(spline);
        }
    }

    private Vector3 ShowPoint(int index)
    {
        // get the world space coords of local point at index 'index'
        Vector3 p = thisTrans.TransformPoint(spline.GetControlPoint(index));

        // draw a little handle button over this point

        // handy utility function that gives us a fixed screen size for any point
        float size = HandleUtility.GetHandleSize(p);

        if(index % 3 == 0)
        {
            Handles.color = Color.red;
        }
        else
        {
            Handles.color = Color.yellow;
        }

        if(Handles.Button(p, thisRot, handleSize * size, pickSize * size, Handles.DotCap))
        {
            selectedIndex = index;
            Repaint();
        }

		if((selectedIndex == index) || selectAllPoints)
        {
            // draw position handles and handle their changes (in world space)
            EditorGUI.BeginChangeCheck();

            switch(Tools.current)
            {
                case Tool.Move:
                    p = Handles.DoPositionHandle(p, thisRot);
                    break;
                case Tool.Rotate:
                    thisRot = Handles.DoRotationHandle(thisRot, p);
                    break;
            }

            if(EditorGUI.EndChangeCheck())
            {
                // Notify Undo and SetDirty
                Undo.RecordObject(spline, "Move SplinePoint");

                // inverse transform them back to local space
                spline.SetControlPoint(index, thisTrans.InverseTransformPoint(p));

                EditorUtility.SetDirty(spline);
            }
        }
        return p;
    }
}
