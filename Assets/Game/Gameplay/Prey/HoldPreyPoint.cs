using UnityEngine;
using System.Collections;

public class HoldPreyPoint : MonoBehaviour 
{	
	private void OnDrawGizmos() {
		Gizmos.color = Colors.magenta;
		Vector3 length = Vector3.forward * 0.5f;
		Matrix4x4 currentMatrix = Gizmos.matrix;
		Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
		Gizmos.matrix = rotationMatrix; 
		Gizmos.DrawCube(-length * 0.5f, length + Vector3.one * 0.0625f);
		Gizmos.matrix = currentMatrix;
	}
}
