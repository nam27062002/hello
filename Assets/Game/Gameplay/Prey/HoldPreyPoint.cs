using UnityEngine;
using System.Collections;

public class HoldPreyPoint : MonoBehaviour 
{	
	protected bool m_holded = false;
	public bool holded{ get{ return m_holded;} set { m_holded = value; }}

	private void OnDrawGizmos() {
		if ( m_holded )
			Gizmos.color = Colors.yellow;	
		else
			Gizmos.color = Colors.green;

		Vector3 length = Vector3.forward * 0.5f;
		Matrix4x4 currentMatrix = Gizmos.matrix;
		Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
		Gizmos.matrix = rotationMatrix; 
		Gizmos.DrawCube(-length * 0.5f, length + Vector3.one * 0.0625f);
		Gizmos.matrix = currentMatrix;
	}
}
