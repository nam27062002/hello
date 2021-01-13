// ScreenUtils.cs
// Hungry Dragon
// 
// Created by Alger Ortín Castellví on 09/04/2015.
// Copyright (c) 2015 Ubisoft. All rights reserved.

#region INCLUDES AND PREPROCESSOR --------------------------------------------------------------------------------------
using UnityEngine;
using System.Collections;
using System.Diagnostics;
#endregion

#region CLASSES --------------------------------------------------------------------------------------------------------
/// <summary>
/// Testing class.
/// </summary>
public class ScreenUtils {
	#region CONSTANTS --------------------------------------------------------------------------------------------------
	public static readonly Vector2 DEFAULT_UI_CANVAS_RESOLUTION = new Vector2(800, 600);	// Default value of the "Reference Resolution" property of the Canvas Scaler component in UI canvases
	#endregion

	#region INTERNAL MEMBERS -------------------------------------------------------------------------------------------

	#endregion
	
	#region GENERIC METHODS --------------------------------------------------------------------------------------------
	public static Vector2 WorldToCanvas(Vector3 _worldPos, Canvas _targetCanvas = null) {
		// Convert to viewport point
		Vector2 viewportPoint = Camera.main.WorldToViewportPoint(_worldPos);	// [0..1]

		// Apply canvas size
		Vector2 canvasSize = DEFAULT_UI_CANVAS_RESOLUTION;
		if(_targetCanvas != null) {
			canvasSize.x = _targetCanvas.pixelRect.width;
			canvasSize.y = _targetCanvas.pixelRect.height;
		}
		viewportPoint.x = viewportPoint.x * canvasSize.x - canvasSize.x/2;
		viewportPoint.y = viewportPoint.y * canvasSize.y - canvasSize.y/2;
		
		return viewportPoint;
	}
	#endregion
}
#endregion
