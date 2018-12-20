using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggSeasonalDeco : MonoBehaviour {

    public Transform m_reparentRoot;

	public void OnReparent(GameObject go)
    {
        AutoParenter[] autoParenters = go.GetComponentsInChildren<AutoParenter>();
        if ( autoParenters != null )
        {
            int l = autoParenters.Length;
            for (int k = 0; k < l; k++)
            {
				autoParenters[k].parentRoot = m_reparentRoot;
				if(autoParenters[k].when == AutoParenter.When.MANUAL) {
					autoParenters[k].Reparent();
				}
            }
        }
    }
}
