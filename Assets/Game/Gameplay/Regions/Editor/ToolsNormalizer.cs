using Assets.Code.Game.Spline;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

class ToolsNormalizer
{
    [MenuItem("Hungry Dragon/Normalizer/Normalize Collisions")]
    static void NormalizeColliders()
    {
		List<PolyMesh> objectsToModify = new List<PolyMesh>();
		List<Object> objects = new List<Object>();
		int groundLayer = LayerMask.NameToLayer("Ground");
		int groundVisibleLayer = LayerMask.NameToLayer("GroundVisible");
		PolyMesh[] polyMeshes = Object.FindObjectsOfType<PolyMesh>();
		for (int i = 0; i < polyMeshes.Length; i++)
        {
			int objectLayer = polyMeshes[i].gameObject.layer;
			if ( objectLayer == groundLayer || objectLayer == groundVisibleLayer )
        	{
				objectsToModify.Add(polyMeshes[i]);
				objects.Add( polyMeshes[i].gameObject );
        	}
        }

        if ( objectsToModify.Count > 0 )
        {
			Undo.RecordObjects( objects.ToArray(), "Normalize Collisions" );
			for( int i = 0; i<objectsToModify.Count; i++ )
				objectsToModify[i].NormalizeMesh();	
			EditorApplication.MarkSceneDirty();
		}
    }
}