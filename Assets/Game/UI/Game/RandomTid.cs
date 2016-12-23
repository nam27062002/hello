using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent (typeof (Localizer))]
public class RandomTid : MonoBehaviour {

	public List<string> m_tids;

	public void OnEnable()
	{
		if ( m_tids.Count > 0 )
		{
			string newTid = m_tids[ Random.Range( 0, m_tids.Count)];
			GetComponent<Localizer>().Localize( newTid );
		}
	}
}
