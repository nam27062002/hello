using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugMatrix : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Matrix4x4 mat = transform.localToWorldMatrix;

            Debug.Log("Row 0: " + mat.GetRow(0));
            Debug.Log("Row 1: " + mat.GetRow(1));
            Debug.Log("Row 2: " + mat.GetRow(2));
            Debug.Log("Row 3: " + mat.GetRow(3));
//            float sx = mat[0]
            Debug.Log("Scale X: " + Vector4.Magnitude(mat.GetRow(0)));
            Debug.Log("Scale Y: " + Vector4.Magnitude(mat.GetRow(1)));
            Debug.Log("Scale Z: " + Vector4.Magnitude(mat.GetRow(2)));
        }
    }
}
