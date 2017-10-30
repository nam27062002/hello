using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CacheServerManager {

	#region singleton
	//------------------------------------------------------------------------//
	// SINGLETON IMPLEMENTATION												  //
	//------------------------------------------------------------------------//
	private static CacheServerManager s_pInstance = null;
	public static CacheServerManager SharedInstance
    {
        get
        {
            if (s_pInstance == null)
            {
				s_pInstance = new CacheServerManager();
            }
            return s_pInstance;
        }
    }
    #endregion

    int[] m_versionNumber;
    public void Init()
    {
		m_versionNumber = new int[] {0,32,0};
    }

    public int[] versionNumber
    {
    	get{return m_versionNumber;}
    }

}
