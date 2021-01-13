using Assets.Code.Game.Spline;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

class RegionScraper
{
    [MenuItem("FGOL/Scrape Regions")]
    static void ScrapeRegions()
    {
        RegionHolder[] currentHolders = Object.FindObjectsOfType<RegionHolder>().ToArray();

        if (currentHolders.Length > 1)
            throw new Exception("Multiple Region Holder Components found");

        RegionHolder regionHolder = currentHolders.Length == 0 ? new GameObject("Region Holder").AddComponent<RegionHolder>() : currentHolders[0];

        Region[] temp = GetRegions("Current");
        Current[] oldCurrents = regionHolder.m_levelCurrents;
        regionHolder.m_levelCurrents = new Current[temp.Length];
        for (int i = 0; i < temp.Length; i++)
        {
            regionHolder.m_levelCurrents[i] = (Current)temp[i];
            for (int j = 0; j < oldCurrents.Length; j++)
            {
                if (oldCurrents[j].GetName() == regionHolder.m_levelCurrents[i].GetName())
                {
                    regionHolder.m_levelCurrents[i].m_hideSituationalText = oldCurrents[j].m_hideSituationalText;
                    regionHolder.m_levelCurrents[i].m_playEnterSFX = oldCurrents[j].m_playEnterSFX;
                }
            }
        }

        temp = GetRegions("MissionArea");
        regionHolder.m_levelMissionAreas = new MissionArea[temp.Length];
        for (int i = 0; i < temp.Length; i++)
        {
             regionHolder.m_levelMissionAreas[i] = (MissionArea)temp[i];
        }

		EditorApplication.MarkSceneDirty();
    }

    static Region[] GetRegions(string tag)
	{
        PolyMesh[] polyMeshes = Object.FindObjectsOfType<PolyMesh>();
        List<Region> regions = new List<Region>();

        for (int i = 0; i < polyMeshes.Length; i++)
        {
            if (polyMeshes[i].gameObject.CompareTag(tag))
            {
				List<Vector3> vertices = new List<Vector3>(polyMeshes[i].keyPoints);
				// Add polymesh position
				for( int j = 0;j<vertices.Count; j++ )
					vertices[j] = polyMeshes[i].gameObject.transform.TransformPoint( vertices[j] );

                regions.Add(Create(polyMeshes[i].gameObject.name, vertices, polyMeshes[i].GetComponent<BezierSplineForce>()));

                /*
                if (tag == "MissionArea")
                {
                    AddCollider(polyMeshes[i]);
                }
                */
				polyMeshes[i].meshCollider.convex = true;
                polyMeshes[i].meshCollider.isTrigger = true;
				polyMeshes[i].BuildMesh();
            }
        }

        return regions.ToArray();
	}

    static void AddCollider(PolyMesh polyMesh)
    {
        if (polyMesh.meshCollider != null)
        {
            // ComponentDestroyer.DestroyImmediate(polyMesh.gameObject.transform.GetChild(0).gameObject);
        }

        var obj = new GameObject(polyMesh.gameObject.name + "_Collider", typeof(MeshCollider));
        MissionAreaTrigger areaTrigger = obj.AddComponent<MissionAreaTrigger>();
        areaTrigger.OasisKey = "STRING_INGAME_LOCATION_" + polyMesh.gameObject.name.ToUpper();
        obj.layer = 29; //Layer 29 == "MissionAreas"
        polyMesh.meshCollider = obj.GetComponent<MeshCollider>();
        MeshCollider collider = polyMesh.meshCollider.GetComponent<MeshCollider>();
        
        if (polyMesh.colliderParent == null)
        {
            obj.transform.parent = polyMesh.transform;
        }
        else
        {
            obj.transform.parent = polyMesh.colliderParent;
        }

        obj.transform.position = polyMesh.transform.position;
        obj.transform.localScale = polyMesh.transform.localScale;

        collider.convex = true;
        collider.isTrigger = true;

        polyMesh.BuildMesh();
    }

    static Region Create(string name, List<Vector3> vertices, BezierSplineForce splineForce = null)
    {
        var minX = vertices.Select(v => v.x).Min();
        var maxX = vertices.Select(v => v.x).Max();
        var minY = vertices.Select(v => v.y).Min();
        var maxY = vertices.Select(v => v.y).Max();
        var xs = vertices.Select(v => v.x).ToArray();
        var ys = vertices.Select(v => v.y).ToArray();

        var constants = new float[vertices.Count];
        var multiples = new float[vertices.Count];
        var j = vertices.Count - 1;

        for (var i = 0; i < vertices.Count; i++)
        {
            if (ys[j] == ys[i])
            {
                constants[i] = xs[i];
                multiples[i] = 0;
            }
            else
            {
                constants[i] = xs[i] - (ys[i] * xs[j]) / (ys[j] - ys[i]) + (ys[i] * xs[i]) / (ys[j] - ys[i]);
                multiples[i] = (xs[j] - xs[i]) / (ys[j] - ys[i]);
            }

            j = i;
        }

        if (splineForce != null)
        {
            return new Current(name, minX, maxX, minY, maxY, ys, multiples, constants, splineForce);
        }
        else
        {
            return new MissionArea(name, minX, maxX, minY, maxY, ys, multiples, constants);
        }
    }
}