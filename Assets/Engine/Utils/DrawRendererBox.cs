using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class DrawRendererBox : MonoBehaviour {




	void OnDrawGizmosSelected()
    {
        // Draw a yellow cube at the transform position
        Gizmos.color = Color.yellow;
		Renderer rend = GetComponent<Renderer>();
        rend.GetComponent<MeshFilter>().mesh.RecalculateBounds();
        Gizmos.DrawWireCube(rend.bounds.center, rend.bounds.size);
    }
}
