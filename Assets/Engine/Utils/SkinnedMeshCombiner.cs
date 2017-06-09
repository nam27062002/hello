using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkinnedMeshCombiner : MonoBehaviour {

	public SkinnedMeshRenderer m_renderer;
	public List<MeshFilter> m_meshes;
	public List<string> m_bonesToAttach;

	// Use this for initialization
	void Start () {
		Mesh m = Combine( m_renderer, m_meshes, m_bonesToAttach);
		m_renderer.sharedMesh = m;
		for( int i = 0; i<m_meshes.Count; ++i )
			DestroyObject( m_meshes[i].gameObject );
	}

	Mesh Combine( SkinnedMeshRenderer skinnedMesh, List<MeshFilter> parts, List<string> bonesAnchors)
	{
			// Bone Weigth
		List<BoneWeight> boneWeights = new List<BoneWeight>();
		boneWeights.AddRange( skinnedMesh.sharedMesh.boneWeights );

			// bind poses
		List<Matrix4x4> bindposes = new List<Matrix4x4>();
		for( int i = 0; i < skinnedMesh.bones.Length; i++ ) {
			bindposes.Add( skinnedMesh.bones[i].worldToLocalMatrix );
        }

        Transform[] bones = skinnedMesh.bones;
		for( int i = 0; i<parts.Count; i++ )
		{
			MeshFilter part = parts[i];

			// Check if materials are different
			Renderer r = part.GetComponent<Renderer>();

			// Search proper bone
			bool found = false;
			string boneId = bonesAnchors[i];
			BoneWeight bWeight = new BoneWeight();
			for( int j = 0; j<bones.Length && !found; j++ )
        	{
				if ( bones[j].name.CompareTo( boneId ) == 0)
        		{
        			bWeight.boneIndex0 = j;
        			bWeight.weight0 = 1;
        			found = true;
        		}
        	}
        	// Add weights
			for( int j = 0; j<part.mesh.vertexCount; j++ ) 
			{
	    		boneWeights.Add(bWeight);
			}
		}

		CombineInstance[] combine = new CombineInstance[ parts.Count + 1];
		int[] meshIndex = new int[parts.Count + 1];

		combine[0].mesh = skinnedMesh.sharedMesh;
		combine[0].transform = skinnedMesh.transform.localToWorldMatrix;
		meshIndex[0] = skinnedMesh.sharedMesh.vertexCount;

		for( int i = 0; i<parts.Count; i++ )
		{
			combine[i+1].mesh = parts[i].sharedMesh;
			combine[i+1].transform = parts[i].transform.localToWorldMatrix;
			meshIndex[i+1] = parts[i].sharedMesh.vertexCount;
		}

		Mesh _newMesh = new Mesh();
		_newMesh.CombineMeshes(combine, true, true);
		_newMesh.boneWeights = boneWeights.ToArray();
		_newMesh.bindposes = bindposes.ToArray();

		return _newMesh;
	} 


}
