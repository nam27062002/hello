using Assets.Code.Game.Spline;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

class SelectNonUniformScaledMeshes
{
    
	[MenuItem("Hungry Dragon/Tools/Highlight Non uniform scaled meshes")]
	static void HighlightNonUniformScale()
    {
		MeshFilter[] meshesFilters = Object.FindObjectsOfType<MeshFilter>();
		List<GameObject> toSelect = new List<GameObject>();
		for (int i = 0; i < meshesFilters.Length; i++)
        {
        	bool badScale = false;
			Vector3 scale = meshesFilters[i].transform.localScale;
			if ( !MathUtils.FuzzyEquals(scale.x, scale.y) || !MathUtils.FuzzyEquals(scale.x, scale.z) || !MathUtils.FuzzyEquals(scale.y, scale.z) )
			{
				badScale = true;
			}

			if ( badScale )
			{
				// Highlight object
				toSelect.Add( meshesFilters[i].gameObject );
			}
        }

		SkinnedMeshRenderer[] skinnedRenderers = Object.FindObjectsOfType<SkinnedMeshRenderer>();
		for (int i = 0; i < skinnedRenderers.Length; i++)
        {
        	bool badScale = false;
			Vector3 scale = skinnedRenderers[i].transform.localScale;
			if ( !MathUtils.FuzzyEquals(scale.x, scale.y) || !MathUtils.FuzzyEquals(scale.x, scale.z) || !MathUtils.FuzzyEquals(scale.y, scale.z) )
			{
				badScale = true;
			}

			if ( badScale )
			{
				// Highlight object
				toSelect.Add( skinnedRenderers[i].gameObject );
			}
        }

        Selection.objects = toSelect.ToArray();
    }

	
}